using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.ProductImage;
using OnlineShop.Application.Features.ProductImage.Command.Create;
using OnlineShop.Application.Features.ProductImage.Command.Update;
using OnlineShop.Application.Features.ProductImage.Command.Delete;
using OnlineShop.Application.Features.ProductImage.Queries.GetById;
using OnlineShop.Application.Features.ProductImage.Queries.GetByProductId;
using OnlineShop.Application.Features.ProductImage.Queries.GetAll;

namespace OnlineShop.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductImageController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ProductImageController> _logger;

        public ProductImageController(IMediator mediator, ILogger<ProductImageController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting all product images");
            var result = await _mediator.Send(new GetAllProductImagesQuery(), cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} product images", result.Data?.Count() ?? 0);
                return Ok(result);
            }
            
            _logger.LogWarning("Failed to retrieve product images: {Error}", result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting product image by ID: {ImageId}", id);
            var result = await _mediator.Send(new GetProductImageByIdQuery { Id = id }, cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved product image: {ImageId}", id);
                return Ok(result);
            }
            
            _logger.LogWarning("Product image not found: {ImageId}", id);
            return NotFound(result);
        }

        [HttpGet("product/{productId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByProductId(Guid productId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting product images for product: {ProductId}", productId);
            var result = await _mediator.Send(new GetProductImagesByProductIdQuery { ProductId = productId }, cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} product images for product: {ProductId}", result.Data?.Count() ?? 0, productId);
                return Ok(result);
            }
            
            _logger.LogWarning("Failed to retrieve product images for product: {ProductId}. Error: {Error}", productId, result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateProductImageDto dto, CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("Creating product image for product: {ProductId} by user: {UserId}", dto.ProductId, userId);
            
            var command = new CreateProductImageCommand { ProductImage = dto };
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Product image created successfully: {ImageId} for product: {ProductId} by user: {UserId}", 
                    result.Data?.Id, dto.ProductId, userId);
                return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
            }
            
            _logger.LogWarning("Failed to create product image for product: {ProductId} by user: {UserId}. Error: {Error}", 
                dto.ProductId, userId, result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductImageDto dto, CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            dto.Id = id;
            _logger.LogInformation("Updating product image: {ImageId} by user: {UserId}", id, userId);
            
            var command = new UpdateProductImageCommand { ProductImage = dto };
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Product image updated successfully: {ImageId} by user: {UserId}", id, userId);
                return Ok(result);
            }
            
            _logger.LogWarning("Failed to update product image: {ImageId} by user: {UserId}. Error: {Error}", id, userId, result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("Deleting product image: {ImageId} by user: {UserId}", id, userId);
            
            var command = new DeleteProductImageCommand { Id = id };
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Product image deleted successfully: {ImageId} by user: {UserId}", id, userId);
                return NoContent();
            }
            
            _logger.LogWarning("Failed to delete product image: {ImageId} by user: {UserId}. Error: {Error}", id, userId, result.ErrorMessage);
            return NotFound(result);
        }
    }
}
