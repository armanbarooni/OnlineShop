using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OnlineShop.Infrastructure.Services;

namespace OnlineShop.WebAPI.Workers
{
    public class MahakOutgoingSyncWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MahakOutgoingSyncWorker> _logger;

        public MahakOutgoingSyncWorker(
            IServiceProvider serviceProvider,
            ILogger<MahakOutgoingSyncWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("MahakOutgoingSyncWorker starting - will sync orders to Mahak every 1 minute");

            // Wait 30 seconds before first run
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var syncService = scope.ServiceProvider
                        .GetRequiredService<MahakOutgoingSyncService>();
                    
                    await syncService.SyncOrdersToMahakAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in MahakOutgoingSyncWorker");
                }

                // Wait 1 minute before next sync
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
