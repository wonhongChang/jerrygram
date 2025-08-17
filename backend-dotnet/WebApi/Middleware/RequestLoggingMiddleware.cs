using Serilog.Context;
using System.Diagnostics;
using WebApi.Extensions;

namespace WebApi.Middleware
{
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
            var stopwatch = Stopwatch.StartNew();
            var requestId = Guid.NewGuid().ToString();

            var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? requestId;

            var userId = context.GetUserId();
            var sessionId = context.GetSessionId();
            var ipAddress = context.GetIpAddress();
            var userAgent = context.GetUserAgent();

            context.Response.Headers.Append("X-Request-ID", requestId);
            context.Response.Headers["X-Correlation-ID"] = correlationId;

            // Serilog LogContext
            using (LogContext.PushProperty("CorrelationId", correlationId))
            using (LogContext.PushProperty("RequestId", requestId))
            using (LogContext.PushProperty("UserId", userId))
            using (LogContext.PushProperty("SessionId", sessionId))
            using (LogContext.PushProperty("RequestMethod", context.Request.Method))
            using (LogContext.PushProperty("RequestPath", context.Request.Path))
            using (LogContext.PushProperty("UserAgent", userAgent))
            using (LogContext.PushProperty("RemoteIpAddress", ipAddress))
            {
                _logger.LogInformation(
                    "Request {RequestId}: {Method} {Path} started",
                    requestId,
                    context.Request.Method,
                    context.Request.Path);

                try
                {
                    await _next(context);
                }
                finally
                {
                    stopwatch.Stop();

                    var level = context.Response.StatusCode >= 400 ? LogLevel.Warning : LogLevel.Information;

                    _logger.Log(level,
                        "Request {RequestId} (CorrelationId: {CorrelationId}): {Method} {Path} completed with {StatusCode} in {ElapsedMilliseconds}ms",
                        requestId,
                        correlationId,
                        context.Request.Method,
                        context.Request.Path,
                        context.Response.StatusCode,
                        stopwatch.ElapsedMilliseconds);

                    if (stopwatch.ElapsedMilliseconds > 3000) // More than 3 seconds
                    {
                        _logger.LogWarning(
                            "Slow request detected {RequestId}: {Method} {Path} took {ElapsedMilliseconds}ms",
                            requestId,
                            context.Request.Method,
                            context.Request.Path,
                            stopwatch.ElapsedMilliseconds);
                    }
                }
            }
        }
    }
}