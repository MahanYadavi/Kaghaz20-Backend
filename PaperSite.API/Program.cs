using PaperSite.API.Security;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PaperSite.API.Middlewares;
using PaperSite.Application.Common.Responses;
using PaperSite.Application.Configuration;
using PaperSite.Application.DTOs.Auth;
using PaperSite.Application.Interfaces;
using PaperSite.Application.Validators.Auth;
using PaperSite.Domain.Entities;
using PaperSite.Infrastructure;
using PaperSite.Infrastructure.Persistence;
using PaperSite.Infrastructure.Repositories;
using PaperSite.Infrastructure.Services;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = 1_048_576);

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState.Values
                .SelectMany(x => x.Errors)
                .Select(x => string.IsNullOrWhiteSpace(x.ErrorMessage) ? "Invalid request value" : x.ErrorMessage)
                .ToList();

            var response = BaseResponse<object>.Failure("Validation failed", errors);
            return new BadRequestObjectResult(response);
        };
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddSingleton<AuthCookie>();
builder.Services.AddCors(options => options.AddPolicy("FrontendCors", policy =>
    policy.WithOrigins(BrowserRequestMiddleware.AllowedOrigins)
        .AllowCredentials().AllowAnyHeader()
        .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE")
        .WithExposedHeaders("Retry-After")));
builder.Services.Configure<SmsSettings>(
    builder.Configuration.GetSection("Sms")
);
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;

    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
    options.ForwardLimit = 1;
    // Official ArvanCloud ranges; maintain via ReverseProxy:KnownNetworks.
    var networks = builder.Configuration.GetSection("ReverseProxy:KnownNetworks").Get<string[]>()
        ?? ["185.143.232.0/22", "188.229.116.16/30", "94.101.182.0/27", "2.144.3.128/28",
            "37.32.16.0/27", "37.32.17.0/27", "37.32.18.0/27", "37.32.19.0/27",
            "185.215.232.0/22", "178.131.120.48/28", "94.101.183.0/28",
            "78.157.36.112/28", "95.38.61.80/28", "193.24.119.0/29"];
    if (networks.Length == 0) throw new InvalidOperationException("Trusted proxy networks must not be empty.");
    foreach (var network in networks)
        options.KnownIPNetworks.Add(System.Net.IPNetwork.Parse(network));
});
builder.Services.AddOptions<SmsSettings>()
    .Bind(builder.Configuration.GetSection("Sms"))
    .Validate(x => !string.IsNullOrWhiteSpace(x.ApiKey), "Sms:ApiKey is required.")
    .Validate(x => int.TryParse(x.TemplateId, out _), "Sms:TemplateId must be numeric.")
        .Validate(
        x => int.TryParse(x.PaymentSuccessTemplateId, out _),
        "Sms:PaymentSuccessTemplateId must be numeric."
    )
    .ValidateOnStart();
builder.Services.AddHttpClient<ISmsService, SmsService>(client =>
{
    client.BaseAddress = new Uri("https://api.sms.ir/");
    client.Timeout = TimeSpan.FromSeconds(10);
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);

    options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PaperSite API",
        Version = "v1",
        Description = "Production-ready MVP backend for paper and packaging online store"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter a valid JWT bearer token."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<MellatPaymentSettings>(builder.Configuration.GetSection("MellatPayment"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
    ?? throw new InvalidOperationException("JwtSettings configuration is missing.");

if (string.IsNullOrWhiteSpace(jwtSettings.Secret) || jwtSettings.Secret.Length < 32)
{
    throw new InvalidOperationException("JwtSettings:Secret must be configured with at least 32 characters.");
}

if (string.IsNullOrWhiteSpace(jwtSettings.Issuer) || string.IsNullOrWhiteSpace(jwtSettings.Audience))
{
    throw new InvalidOperationException("JwtSettings:Issuer and JwtSettings:Audience must be configured.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.SaveToken = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
        ClockSkew = TimeSpan.Zero
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // The handler parses Bearer AFTER this event. Never override a supplied header.
            if (!context.Request.Headers.ContainsKey("Authorization"))
                context.Token = context.Request.Cookies[AuthCookie.Name];
            return Task.CompletedTask;
        },
        OnChallenge = async context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(BaseResponse<object>.Failure("احراز هویت الزامی است"));
        },
        OnForbidden = async context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(BaseResponse<object>.Failure("شما اجازه دسترسی به این منبع را ندارید"));
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy => policy.RequireRole(Role.Admin));
    options.AddPolicy("CustomerPolicy", policy => policy.RequireRole(Role.Customer));
});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJWtService, JwtService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<ILogService, LogService>();
builder.Services.AddHttpClient<IMellatPaymentService, MellatPaymentService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>();
builder.Services.AddRateLimiter(ApiRateLimits.Configure);
var app = builder.Build();

if (!app.Environment.IsDevelopment() && string.Equals(builder.Configuration["AllowedHosts"], "*", StringComparison.Ordinal))
    throw new InvalidOperationException("AllowedHosts must be restricted in production.");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseForwardedHeaders();

app.UseHttpsRedirection();
if (!app.Environment.IsDevelopment()) app.UseHsts();
app.UseRouting();
app.Use(async (context, next) =>
{
    context.Response.Headers.CacheControl = "no-store";
    await next(context);
});
app.UseCors("FrontendCors");
app.UseMiddleware<BrowserRequestMiddleware>();
app.UseMiddleware<OtpValidationMiddleware>();
app.UseAuthentication();
app.UseRateLimiter();
app.UseMiddleware<RequestResponseLoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
