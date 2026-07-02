using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Application.Contracts.Services;
using OnlineShop.Application.DTOs.Mahak;

namespace OnlineShop.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MahakRegionsController : ControllerBase
    {
        private readonly IMahakRegionService _regionService;

        public MahakRegionsController(IMahakRegionService regionService)
        {
            _regionService = regionService;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<MahakRegionDto>>> Get(CancellationToken cancellationToken)
        {
            var regions = await _regionService.GetRegionsAsync(cancellationToken);
            return Ok(regions);
        }
    }
}
