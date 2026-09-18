using System.IdentityModel.Tokens.Jwt;

namespace PaperSite.API.Security;

public sealed class AuthCookie(IConfiguration configuration, IWebHostEnvironment environment)
{
    public const string Name = "paper_token";

    private CookieOptions Options() => new()
    {
        HttpOnly = true,
        Secure = !environment.IsDevelopment(),
        SameSite = SameSiteMode.Lax,
        Path = "/",
        Domain = environment.IsProduction()
            ? configuration["AuthenticationCookie:Domain"] ?? ".kaghaz20.ir" : null
    };

    public void Append(HttpResponse response, string? token)
    {
        if (string.IsNullOrEmpty(token)) return;
        var options = Options();
        options.Expires = new DateTimeOffset(new JwtSecurityTokenHandler().ReadJwtToken(token).ValidTo);
        response.Cookies.Append(Name, token, options);
        response.Headers.CacheControl = "no-store";
    }

    public void Delete(HttpResponse response)
    {
        response.Cookies.Delete(Name, Options());
        response.Headers.CacheControl = "no-store";
    }
}
