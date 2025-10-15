using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Application.DTOs.Product;
using OnlineShop.Application.Features.Product.Command.Create;
using OnlineShop.Application.Features.Product.Command.Delete;
using OnlineShop.Application.Features.Product.Command.Update;
using OnlineShop.Application.Features.Product.Queries.GetAll;
using OnlineShop.Application.Features.Product.Queries.GetById;
using OnlineShop.Application.Features.Product.Queries.Search;
using OnlineShop.Application.Features.Product.Queries.GetRelatedProducts;
using OnlineShop.Application.Features.Product.Queries.GetFrequentlyBoughtTogether;
using OnlineShop.Application.Features.Product.Queries.GetRecentlyViewed;
using OnlineShop.Application.Features.Product.Commands.TrackProductView;

namespace OnlineShop.WebAPI.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class ProductController(IMediator mediator, ILogger<ProductController> logger) : ControllerBase
	{
		private readonly IMediator _mediator = mediator;
		private readonly ILogger<ProductController> _logger = logger;

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting all products");
            var result = await _mediator.Send(new GetAllProductsQuery(), cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} products", result.Data?.Count() ?? 0);
                return Ok(result);
            }
            
            _logger.LogWarning("Failed to retrieve products: {Error}", result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpPost("search")]
        [AllowAnonymous]
        public async Task<IActionResult> Search([FromBody] ProductSearchCriteriaDto? criteria, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Searching products with criteria");
            var result = await _mediator.Send(new ProductSearchQuery { Criteria = criteria }, cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} products from search", result.Data?.Products?.TotalCount ?? 0);
                return Ok(result);
            }
            
            _logger.LogWarning("Failed to search products: {Error}", result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting product by ID: {ProductId}", id);
            var result = await _mediator.Send(new GetProductByIdQuery { Id = id }, cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved product: {ProductId}", id);
                return Ok(result);
            }
            
            _logger.LogWarning("Product not found: {ProductId}", id);
            return NotFound(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(
            [FromBody] CreateProductDto dto,
            CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("Creating product: {ProductName} by user: {UserId}", dto.Name, userId);
            
            var command = new CreateProductCommand { Product = dto };
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Product created successfully: {ProductId} by user: {UserId}", 
                    result.Data?.Id, userId);
                return Ok(result);
            }
            
            _logger.LogWarning("Failed to create product: {ProductName} by user: {UserId}. Error: {Error}", 
                dto.Name, userId, result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(
            [FromRoute] Guid id,
            [FromBody] UpdateProductDto dto,
            CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("Updating product: {ProductId} by user: {UserId}", id, userId);
            
            dto.Id = id;
            var command = new UpdateProductCommand { Product = dto };
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Product updated successfully: {ProductId} by user: {UserId}", id, userId);
                return Ok(result);
            }
            
            _logger.LogWarning("Failed to update product: {ProductId} by user: {UserId}. Error: {Error}", 
                id, userId, result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("Deleting product: {ProductId} by user: {UserId}", id, userId);
            
            var result = await _mediator.Send(new DeleteProductCommand(id), cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Product deleted successfully: {ProductId} by user: {UserId}", id, userId);
                return NoContent();
            }
            
            _logger.LogWarning("Failed to delete product: {ProductId} by user: {UserId}. Error: {Error}", 
                id, userId, result.ErrorMessage);
            return NotFound(result);
        }

        [HttpGet("{id}/related")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRelatedProducts(
            [FromRoute] Guid id,
            [FromQuery] string relationType = "Similar",
            [FromQuery] int limit = 10,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting related products for: {ProductId}, type: {RelationType}", id, relationType);
            
            var result = await _mediator.Send(new GetRelatedProductsQuery 
            { 
                ProductId = id, 
                RelationType = relationType, 
                Limit = limit 
            }, cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} related products for: {ProductId}", 
                    result.Data?.Count ?? 0, id);
                return Ok(result);
            }
            
            _logger.LogWarning("Failed to get related products for: {ProductId}. Error: {Error}", 
                id, result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpGet("{id}/frequently-bought-together")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFrequentlyBoughtTogether(
            [FromRoute] Guid id,
            [FromQuery] int limit = 10,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting frequently bought together products for: {ProductId}", id);
            
            var result = await _mediator.Send(new GetFrequentlyBoughtTogetherQuery 
            { 
                ProductId = id, 
                Limit = limit 
            }, cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} frequently bought together products for: {ProductId}", 
                    result.Data?.Count ?? 0, id);
                return Ok(result);
            }
            
            _logger.LogWarning("Failed to get frequently bought together products for: {ProductId}. Error: {Error}", 
                id, result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpGet("recently-viewed")]
        [Authorize]
        public async Task<IActionResult> GetRecentlyViewed(
            [FromQuery] int limit = 20,
            CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("Getting recently viewed products for user: {UserId}", userId);
            
            var result = await _mediator.Send(new GetRecentlyViewedQuery 
            { 
                UserId = userId ?? string.Empty, 
                Limit = limit 
            }, cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} recently viewed products for user: {UserId}", 
                    result.Data?.Count ?? 0, userId);
                return Ok(result);
            }
            
            _logger.LogWarning("Failed to get recently viewed products for user: {UserId}. Error: {Error}", 
                userId, result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpPost("{id}/track-view")]
        [Authorize]
        public async Task<IActionResult> TrackProductView(
            [FromRoute] Guid id,
            [FromQuery] string? sessionId = null,
            CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userAgent = Request.Headers.UserAgent.ToString();
            var ipAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString();
            
            _logger.LogInformation("Tracking product view: {ProductId} by user: {UserId}", id, userId);
            
            var result = await _mediator.Send(new TrackProductViewCommand 
            { 
                UserId = userId ?? string.Empty,
                ProductId = id,
                SessionId = sessionId,
                UserAgent = userAgent,
                IpAddress = ipAddress
            }, cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Product view tracked successfully: {ProductId} by user: {UserId}", id, userId);
                return Ok(result);
            }
            
            _logger.LogWarning("Failed to track product view: {ProductId} by user: {UserId}. Error: {Error}", 
                id, userId, result.ErrorMessage);
            return BadRequest(result);
        }
    }
}
