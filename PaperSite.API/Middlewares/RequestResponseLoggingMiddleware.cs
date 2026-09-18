using Microsoft.AspNetCore.Mvc.Controllers;
using PaperSite.Application.Interfaces;
using PaperSite.Domain.Entities;
using System.Diagnostics;
using System.Globalization;
using System.Security.Claims;

namespace PaperSite.API.Middlewares;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ILogService logService)
    {
        var endpoint = context.GetEndpoint();
        var controllerAction = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();

        if (controllerAction is null)
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            await SaveLogAsync(
                logService,
                context,
                controllerAction.ControllerName,
                stopwatch.ElapsedMilliseconds);
        }
    }

    private async Task SaveLogAsync(
        ILogService logService,
        HttpContext context,
        string controllerName,
        long executionTimeMs)
    {
        try
        {
            var now = DateTime.Now;
            var persianCalendar = new PersianCalendar();

            var log = new Log
            {
                controllerName = controllerName,
                requestJson = string.Empty,
                responseJson = string.Empty,
                persianDate = $"{persianCalendar.GetYear(now):0000}/{persianCalendar.GetMonth(now):00}/{persianCalendar.GetDayOfMonth(now):00}",
                Time = now.ToString("HH:mm:ss", CultureInfo.InvariantCulture),
                statusCode = context.Response.StatusCode,
                ipAddress = context.Connection.RemoteIpAddress?.ToString(),
                ExecutionTimeMs = executionTimeMs,
                userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
            };

            await logService.AddAsync(log, context.RequestAborted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save request/response log for {Method} {Path}", context.Request.Method, context.Request.Path);
        }
    }
}
