using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using PaperSite.Application.Common.Responses;
namespace PaperSite.API.Security;

public static class ApiRateLimits
{
    public static void Configure(RateLimiterOptions options)
    {
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        var seconds = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retry)
            ? Math.Max(1, (int)Math.Ceiling(retry.TotalSeconds)) : 60;
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.Headers.RetryAfter = seconds.ToString(System.Globalization.CultureInfo.InvariantCulture);
        await context.HttpContext.Response.WriteAsJsonAsync(
            BaseResponse<object>.Failure($"برای دریافت کد جدید {seconds} ثانیه صبر کنید."), cancellationToken);
    };

    options.GlobalLimiter = new RequestRateLimiter(PartitionedRateLimiter.CreateChained(
        // Check the phone first, so a rejected repeat does not consume the shared IP quota.
        PartitionedRateLimiter.Create<HttpContext, string>(context =>
            OtpValidationMiddleware.IsSendOtp(context)
                ? RateLimitPartition.Get((string)context.Items[OtpValidationMiddleware.MobileKey]!,
                    _ => new CooldownLimiter(TimeSpan.FromSeconds(120)))
                : RateLimitPartition.GetNoLimiter<string>("not-otp")),
        PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        if (OtpValidationMiddleware.IsSendOtp(context))
            return RateLimitPartition.GetFixedWindowLimiter(
                "otp-ip:" + (context.Connection.RemoteIpAddress?.ToString() ?? "unknown"),
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 10, Window = TimeSpan.FromHours(1), QueueLimit = 0,
                    AutoReplenishment = true
                });
        var key = context.User.Identity?.IsAuthenticated == true
            ? context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            : context.Connection.RemoteIpAddress?.ToString();
        return RateLimitPartition.GetFixedWindowLimiter(key ?? "unknown", _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 120,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true
        });
    })));

    options.AddPolicy("AuthLimiter", context =>
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var path = context.Request.Path.ToString().ToLowerInvariant();

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: $"{ip}:{path}",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });

    // Overrides the controller-wide AuthLimiter; OTP is handled by the global chain above.
    options.AddPolicy("OtpLimiter", _ => RateLimitPartition.GetNoLimiter<string>("otp"));
    }
}
