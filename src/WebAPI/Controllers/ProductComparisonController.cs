using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Application.DTOs.ProductComparison;
using OnlineShop.Application.Features.ProductComparison.Commands.AddToComparison;
using OnlineShop.Application.Features.ProductComparison.Commands.RemoveFromComparison;
using OnlineShop.Application.Features.ProductComparison.Commands.ClearComparison;
using OnlineShop.Application.Features.ProductComparison.Queries.GetUserComparison;
using OnlineShop.Application.Features.ProductComparison.Queries.CompareProducts;

namespace OnlineShop.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductComparisonController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ProductComparisonController> _logger;

        public ProductComparisonController(IMediator mediator, ILogger<ProductComparisonController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToComparison([FromBody] AddToComparisonDto dto, CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var command = new AddToComparisonCommand
            {
                UserId = userId,
                ProductId = dto.ProductId
            };

            var result = await _mediator.Send(command, cancellationToken);
            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpDelete("remove/{productId}")]
        public async Task<IActionResult> RemoveFromComparison(Guid productId, CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var command = new RemoveFromComparisonCommand
            {
                UserId = userId,
                ProductId = productId
            };

            var result = await _mediator.Send(command, cancellationToken);
            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearComparison(CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var command = new ClearComparisonCommand
            {
                UserId = userId
            };

            var result = await _mediator.Send(command, cancellationToken);
            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetComparison(CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var query = new GetUserComparisonQuery
            {
                UserId = userId
            };

            var result = await _mediator.Send(query, cancellationToken);
            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("compare")]
        public async Task<IActionResult> CompareProducts(CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var query = new CompareProductsQuery
            {
                UserId = userId
            };

            var result = await _mediator.Send(query, cancellationToken);
            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }
    }
}

