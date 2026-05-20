using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OnlineShop.WebAPI.Configuration;

namespace OnlineShop.WebAPI.Workers
{
    public class KeepAliveWorker : BackgroundService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<KeepAliveWorker> _logger;
        private readonly IOptionsMonitor<PerformanceOptions> _performanceOptions;
        private readonly IConfiguration _configuration;

        public KeepAliveWorker(
            IHttpClientFactory httpClientFactory,
            ILogger<KeepAliveWorker> logger,
            IOptionsMonitor<PerformanceOptions> performanceOptions,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _performanceOptions = performanceOptions;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var options = _performanceOptions.CurrentValue;
            if (!options.EnableKeepAlivePing)
            {
                _logger.LogInformation("KeepAliveWorker is disabled by configuration.");
                return;
            }

            var intervalSeconds = Math.Max(20, options.KeepAliveIntervalSeconds);
            var keepAlivePath = string.IsNullOrWhiteSpace(options.KeepAlivePath) ? "/api/health" : options.KeepAlivePath;
            var baseUrl = ResolveBaseUrl();
            if (baseUrl is null)
            {
                _logger.LogWarning("KeepAliveWorker is enabled but no valid base URL was found. Set BaseUrl in configuration.");
                return;
            }

            var targetUri = new Uri(new Uri(baseUrl), keepAlivePath);
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(10);

            _logger.LogInformation("KeepAliveWorker started. Target: {Target}, IntervalSeconds: {IntervalSeconds}", targetUri, intervalSeconds);

            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(intervalSeconds));
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var request = new HttpRequestMessage(HttpMethod.Get, targetUri);
                    request.Headers.Add("X-Internal-KeepAlive", "1");
                    using var response = await client.SendAsync(request, stoppingToken);

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("KeepAlive ping returned {StatusCode}", (int)response.StatusCode);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "KeepAlive ping failed.");
                }

                if (!await timer.WaitForNextTickAsync(stoppingToken))
                {
                    break;
                }
            }
        }

        private string? ResolveBaseUrl()
        {
            var configured = _configuration["BaseUrl"];
            if (Uri.TryCreate(configured, UriKind.Absolute, out var configuredUri))
            {
                return configuredUri.GetLeftPart(UriPartial.Authority).TrimEnd('/') + "/";
            }

            var aspNetCoreUrls = _configuration["ASPNETCORE_URLS"] ?? Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
            if (!string.IsNullOrWhiteSpace(aspNetCoreUrls))
            {
                var firstUrl = aspNetCoreUrls.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault();
                if (Uri.TryCreate(firstUrl, UriKind.Absolute, out var aspNetCoreUri))
                {
                    return aspNetCoreUri.GetLeftPart(UriPartial.Authority).TrimEnd('/') + "/";
                }
            }

            return null;
        }
    }
}
