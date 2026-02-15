using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OnlineShop.Infrastructure.Services;
using OnlineShop.WebAPI.Configuration;

namespace OnlineShop.WebAPI.Workers
{
    public class MahakOutgoingSyncWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MahakOutgoingSyncWorker> _logger;
        private readonly IOptionsMonitor<BackgroundSyncOptions> _backgroundSyncOptions;

        public MahakOutgoingSyncWorker(
            IServiceProvider serviceProvider,
            ILogger<MahakOutgoingSyncWorker> logger,
            IOptionsMonitor<BackgroundSyncOptions> backgroundSyncOptions)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _backgroundSyncOptions = backgroundSyncOptions;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var options = _backgroundSyncOptions.CurrentValue;
            _logger.LogInformation("MahakOutgoingSyncWorker starting - will sync orders to Mahak every {IntervalMinutes} minute(s)", options.OutgoingIntervalMinutes);

            var initialDelaySeconds = Math.Max(0, options.OutgoingInitialDelaySeconds);
            if (initialDelaySeconds > 0)
            {
                await Task.Delay(TimeSpan.FromSeconds(initialDelaySeconds), stoppingToken);
            }

            var intervalMinutes = Math.Max(1, options.OutgoingIntervalMinutes);
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(intervalMinutes));

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var syncService = scope.ServiceProvider
                        .GetRequiredService<MahakOutgoingSyncService>();

                    await syncService.SyncOrdersToMahakAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in MahakOutgoingSyncWorker");
                }

                if (!await timer.WaitForNextTickAsync(stoppingToken))
                {
                    break;
                }
            }
        }
    }
}
