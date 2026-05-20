using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Options;
using OnlineShop.WebAPI.Configuration;

namespace OnlineShop.WebAPI.Middlewares
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;
        private readonly IOptionsMonitor<PerformanceOptions> _performanceOptions;

        public RequestLoggingMiddleware(
            RequestDelegate next,
            ILogger<RequestLoggingMiddleware> logger,
            IOptionsMonitor<PerformanceOptions> performanceOptions)
        {
            _next = next;
            _logger = logger;
            _performanceOptions = performanceOptions;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Headers.ContainsKey("X-Internal-KeepAlive"))
            {
                await _next(context);
                return;
            }

            var options = _performanceOptions.CurrentValue;
            var stopwatch = Stopwatch.StartNew();
            var requestId = Guid.NewGuid().ToString("N")[..8];
            var requestPath = context.Request.Path.Value ?? string.Empty;
            var isStaticRequest = IsStaticFileRequest(requestPath);
            var shouldLogRequestDetails = !isStaticRequest || options.LogStaticFileRequests;

            var hasAuthHeader = context.Request.Headers.ContainsKey("Authorization") &&
                                !string.IsNullOrWhiteSpace(context.Request.Headers.Authorization);

            if (shouldLogRequestDetails)
            {
                _logger.LogInformation(
                    "Request {RequestId}: {Method} {Path} from {RemoteIp} - AuthHeaderPresent: {HasAuthHeader}",
                    requestId,
                    context.Request.Method,
                    context.Request.Path,
                    context.Connection.RemoteIpAddress?.ToString(),
                    hasAuthHeader);
            }

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
                var elapsedMs = stopwatch.ElapsedMilliseconds;
                var isSlowRequest = elapsedMs >= Math.Max(100, options.SlowRequestThresholdMs);

                if (isSlowRequest)
                {
                    _logger.LogWarning(
                        "Slow request {RequestId}: {Method} {Path} completed with {StatusCode} in {ElapsedMs}ms",
                        requestId,
                        context.Request.Method,
                        context.Request.Path,
                        context.Response.StatusCode,
                        elapsedMs);
                }
                else if (shouldLogRequestDetails)
                {
                    _logger.LogInformation(
                        "Request {RequestId} completed: {StatusCode} in {ElapsedMs}ms",
                        requestId,
                        context.Response.StatusCode,
                        elapsedMs);
                }
            }
        }

        private static bool IsStaticFileRequest(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            if (path.StartsWith("/fa/assets/", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return Path.HasExtension(path);
        }
    }
}
