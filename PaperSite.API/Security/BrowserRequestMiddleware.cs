using Microsoft.AspNetCore.Mvc.Controllers;
using PaperSite.API.Controllers;
using PaperSite.Application.Common.Responses;

namespace PaperSite.API.Security;

public sealed class BrowserRequestMiddleware(RequestDelegate next)
{
    public static readonly string[] AllowedOrigins =
        ["https://www.kaghaz20.ir", "https://kaghaz20.ir", "http://localhost:3000"];

    public async Task InvokeAsync(HttpContext context)
    {
        // Never let intermediaries cache API responses, including preflight and Set-Cookie.
        context.Response.Headers.CacheControl = "no-store";
        var action = context.GetEndpoint()?.Metadata.GetMetadata<ControllerActionDescriptor>();
        var bankCallback = action?.ControllerTypeInfo.AsType() == typeof(PaymentController)
            && action.ActionName == nameof(PaymentController.MellatCallback);
        var unsafeMethod = context.Request.Method is "POST" or "PUT" or "PATCH" or "DELETE";
        if (unsafeMethod && !bankCallback
            && !AllowedOrigins.Contains(context.Request.Headers.Origin.ToString(), StringComparer.Ordinal))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(BaseResponse<object>.Failure("مبدأ درخواست مجاز نیست."));
            return;
        }
        await next(context);
    }
}
