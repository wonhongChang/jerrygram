using Application.Events;
using System.Security.Claims;

namespace WebApi.Extensions
{
    public static class HttpContextExtensions
    {
        public static string GetUserId(this HttpContext context)
        {
            return context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
        }

        public static string GetSessionId(this HttpContext context)
        {
            // Use TraceIdentifier as session ID (Session requires explicit configuration)
            return context.TraceIdentifier;
        }

        public static string GetIpAddress(this HttpContext context)
        {
            // Check for forwarded IP first (for load balancer scenarios)
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',').FirstOrDefault()?.Trim() ?? "unknown";
            }

            return context.Connection?.RemoteIpAddress?.ToString() ?? "unknown";
        }

        public static string GetUserAgent(this HttpContext context)
        {
            return context.Request?.Headers["User-Agent"].FirstOrDefault() ?? "unknown";
        }

        public static string GetCorrelationId(this HttpContext context)
        {
            return context.Request?.Headers["X-Correlation-ID"].FirstOrDefault() ??
                   context.TraceIdentifier;
        }

        public static void EnrichEvent<T>(this HttpContext context, T eventData) where T : BaseEvent
        {
            eventData.UserId = context.GetUserId();
            eventData.SessionId = context.GetSessionId();
            eventData.IpAddress = context.GetIpAddress();
            eventData.UserAgent = context.GetUserAgent();
            eventData.CorrelationId = context.GetCorrelationId();
        }
    }
}
