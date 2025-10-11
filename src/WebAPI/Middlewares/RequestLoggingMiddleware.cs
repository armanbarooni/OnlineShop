using System.Diagnostics;

namespace OnlineShop.WebAPI.Middlewares
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
            var requestId = Guid.NewGuid().ToString("N")[..8];
            
            // Log request
            _logger.LogInformation("Request {RequestId}: {Method} {Path} from {RemoteIp} - UserAgent: {UserAgent}",
                requestId,
                context.Request.Method,
                context.Request.Path,
                context.Connection.RemoteIpAddress?.ToString(),
                context.Request.Headers.UserAgent.ToString());

            // Add request ID to response headers
            context.Response.Headers.Append("X-Request-ID", requestId);

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Request {RequestId} failed with exception: {ExceptionMessage}",
                    requestId, ex.Message);
                throw;
            }
            finally
            {
                stopwatch.Stop();
                
                // Log response
                _logger.LogInformation("Request {RequestId} completed: {StatusCode} in {ElapsedMs}ms",
                    requestId,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds);
            }
        }
    }
}
