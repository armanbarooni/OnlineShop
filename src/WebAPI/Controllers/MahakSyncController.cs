using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Domain.Interfaces.Repositories;
using OnlineShop.Infrastructure.Services;

namespace OnlineShop.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class MahakSyncController : ControllerBase
    {
        private readonly MahakSyncService _mahakSyncService;
        private readonly IMahakSyncLogRepository _mahakSyncLogRepository;
        private readonly ILogger<MahakSyncController> _logger;

        public MahakSyncController(
            MahakSyncService mahakSyncService,
            IMahakSyncLogRepository mahakSyncLogRepository,
            ILogger<MahakSyncController> logger)
        {
            _mahakSyncService = mahakSyncService;
            _mahakSyncLogRepository = mahakSyncLogRepository;
            _logger = logger;
        }

        /// <summary>
        /// Trigger manual sync from Mahak
        /// </summary>
        [HttpPost("sync")]
        public async Task<IActionResult> TriggerSync(CancellationToken cancellationToken)
        {
            try
            {
                await _mahakSyncService.SyncAsync(cancellationToken);
                return Ok(new { message = "Sync completed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during manual sync");
                return StatusCode(500, new { message = "Sync failed", error = ex.Message });
            }
        }

        /// <summary>
        /// Force sync images from Mahak (resets Picture and PhotoGallery versions to 0)
        /// </summary>
        [HttpPost("force-sync-images")]
        public async Task<IActionResult> ForceSyncImages(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Force syncing images from Mahak...");

                // Reset Picture and PhotoGallery versions to 0
                await _mahakSyncLogRepository.ResetRowVersionAsync("Picture", cancellationToken);
                await _mahakSyncLogRepository.ResetRowVersionAsync("PhotoGallery", cancellationToken);

                _logger.LogInformation("Reset Picture and PhotoGallery versions to 0");

                // Trigger sync
                await _mahakSyncService.SyncAsync(cancellationToken);

                return Ok(new { message = "Force image sync completed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during force image sync");
                return StatusCode(500, new { message = "Force image sync failed", error = ex.Message });
            }
        }

        /// <summary>
        /// Get sync status
        /// </summary>
        [HttpGet("status")]
        public async Task<IActionResult> GetSyncStatus(CancellationToken cancellationToken)
        {
            try
            {
                var productVersion = await _mahakSyncLogRepository.GetLastRowVersionAsync("Product", cancellationToken);
                var productDetailVersion = await _mahakSyncLogRepository.GetLastRowVersionAsync("ProductDetail", cancellationToken);
                var categoryVersion = await _mahakSyncLogRepository.GetLastRowVersionAsync("ProductCategory", cancellationToken);
                var pictureVersion = await _mahakSyncLogRepository.GetLastRowVersionAsync("Picture", cancellationToken);
                var photoGalleryVersion = await _mahakSyncLogRepository.GetLastRowVersionAsync("PhotoGallery", cancellationToken);
                var inventoryVersion = await _mahakSyncLogRepository.GetLastRowVersionAsync("ProductDetailStoreAsset", cancellationToken);

                return Ok(new
                {
                    productVersion,
                    productDetailVersion,
                    categoryVersion,
                    pictureVersion,
                    photoGalleryVersion,
                    inventoryVersion
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sync status");
                return StatusCode(500, new { message = "Failed to get sync status", error = ex.Message });
            }
        }
    }
}
