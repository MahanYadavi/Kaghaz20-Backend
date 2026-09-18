using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Options;
using PaperSite.API.Controllers;
using PaperSite.Application.Common.Responses;
using PaperSite.Application.DTOs.Auth;

namespace PaperSite.API.Security;

public sealed class OtpValidationMiddleware(RequestDelegate next)
{
    public const string MobileKey = "ValidatedOtpMobile";
    public static bool IsSendOtp(HttpContext context)
    {
        var action = context.GetEndpoint()?.Metadata.GetMetadata<ControllerActionDescriptor>();
        return HttpMethods.IsPost(context.Request.Method)
            && action?.ControllerTypeInfo.AsType() == typeof(AuthController)
            && action.ActionName == nameof(AuthController.SendOtp);
    }

    public async Task InvokeAsync(HttpContext context, IValidator<SendOtpRequest> validator,
        IOptions<JsonOptions> jsonOptions)
    {
        if (!IsSendOtp(context)) { await next(context); return; }
        if (!context.Request.HasJsonContentType())
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(BaseResponse<object>.Failure("Validation failed", ["بدنه JSON الزامی است."]));
            return;
        }
        context.Request.EnableBuffering();
        SendOtpRequest? request = null;
        try
        {
            request = await JsonSerializer.DeserializeAsync<SendOtpRequest>(context.Request.Body,
                jsonOptions.Value.JsonSerializerOptions, context.RequestAborted);
        }
        catch (JsonException) { }
        finally { context.Request.Body.Position = 0; }
        var errors = request is null ? new[] { "شماره موبایل معتبر نیست." }
            : (await validator.ValidateAsync(request, context.RequestAborted)).Errors.Select(x => x.ErrorMessage).ToArray();
        if (errors.Length > 0)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(BaseResponse<object>.Failure("Validation failed", errors));
            return;
        }
        context.Items[MobileKey] = request!.mobileNo;
        await next(context);
    }
}
