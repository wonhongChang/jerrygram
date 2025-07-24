using System.Diagnostics;

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
            
            // Add request ID to response headers for tracing
            context.Response.Headers.Append("X-Request-ID", requestId);
            
            // Log request
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
                
                // Log response
                var level = context.Response.StatusCode >= 400 ? LogLevel.Warning : LogLevel.Information;
                
                _logger.Log(level,
                    "Request {RequestId}: {Method} {Path} completed with {StatusCode} in {ElapsedMilliseconds}ms",
                    requestId,
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds);

                // Log slow requests
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