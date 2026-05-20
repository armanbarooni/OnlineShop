using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using OnlineShop.Infrastructure.Services;
using OnlineShop.WebAPI.Configuration;

namespace OnlineShop.WebAPI.Workers
{
    public class MahakSyncWorker : BackgroundService
    {
        private readonly ILogger<MahakSyncWorker> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IOptionsMonitor<BackgroundSyncOptions> _backgroundSyncOptions;

        public MahakSyncWorker(
            ILogger<MahakSyncWorker> logger,
            IServiceScopeFactory serviceScopeFactory,
            IOptionsMonitor<BackgroundSyncOptions> backgroundSyncOptions)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _backgroundSyncOptions = backgroundSyncOptions;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("MahakSyncWorker is starting.");

            var options = _backgroundSyncOptions.CurrentValue;
            var initialDelaySeconds = Math.Max(0, options.IncomingInitialDelaySeconds);
            if (initialDelaySeconds > 0)
            {
                await Task.Delay(TimeSpan.FromSeconds(initialDelaySeconds), stoppingToken);
            }

            var intervalMinutes = Math.Max(1, options.IncomingIntervalMinutes);
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(intervalMinutes));

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("MahakSyncWorker running sync at: {time}", DateTimeOffset.Now);

                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var syncService = scope.ServiceProvider.GetRequiredService<MahakSyncService>();
                    await syncService.SyncAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in MahakSyncWorker.");
                }

                if (!await timer.WaitForNextTickAsync(stoppingToken))
                {
                    break;
                }
            }

            _logger.LogInformation("MahakSyncWorker is stopping.");
        }
    }
}
