using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Application.Contracts.Services;
using OnlineShop.Application.DTOs.Mahak;

namespace OnlineShop.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class MahakRegionsController : ControllerBase
    {
        private readonly IMahakRegionService _regionService;
        private readonly ILogger<MahakRegionsController> _logger;

        public MahakRegionsController(
            IMahakRegionService regionService,
            ILogger<MahakRegionsController> logger)
        {
            _regionService = regionService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            try
            {
                var regions = await _regionService.GetRegionsAsync(cancellationToken);
                _logger.LogInformation("Mahak regions endpoint returned {Count} regions.", regions.Count);
                return Ok(regions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mahak regions endpoint failed.");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    message = "Mahak regions could not be loaded.",
                    detail = ex.Message
                });
            }
        }
    }
}
