using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using OnlineShop.Infrastructure.Services;

namespace OnlineShop.WebAPI.Workers
{
    public class MahakSyncWorker : BackgroundService
    {
        private readonly ILogger<MahakSyncWorker> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private const int DelayMinutes = 5;

        public MahakSyncWorker(ILogger<MahakSyncWorker> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("MahakSyncWorker is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("MahakSyncWorker running sync at: {time}", DateTimeOffset.Now);

                try
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var syncService = scope.ServiceProvider.GetRequiredService<MahakSyncService>();
                        await syncService.SyncAsync(stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in MahakSyncWorker.");
                }

                await Task.Delay(TimeSpan.FromMinutes(DelayMinutes), stoppingToken);
            }

            _logger.LogInformation("MahakSyncWorker is stopping.");
        }
    }
}
