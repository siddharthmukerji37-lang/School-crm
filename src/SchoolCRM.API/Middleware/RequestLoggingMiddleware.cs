using System.Security.Claims;
using System.Text.Json;
using SchoolCRM.Shared.Models;

namespace SchoolCRM.API.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requestId = Guid.NewGuid().ToString();
        context.Items["RequestId"] = requestId;
        context.Response.Headers.Append("X-Request-Id", requestId);

        var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Anonymous";
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        var method = context.Request.Method;
        var path = context.Request.Path;

        _logger.LogInformation("[{RequestId}] {Method} {Path} from {IP} by {UserId}",
            requestId, method, path, ip, userId);

        var sw = System.Diagnostics.Stopwatch.StartNew();
        await _next(context);
        sw.Stop();

        var statusCode = context.Response.StatusCode;
        _logger.LogInformation("[{RequestId}] {Method} {Path} responded {StatusCode} in {Elapsed}ms",
            requestId, method, path, statusCode, sw.ElapsedMilliseconds);
    }
}

public class ApiResponseMiddleware
{
    private readonly RequestDelegate _next;

    public ApiResponseMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);
    }
}
