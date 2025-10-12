using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.ProductInventory;
using OnlineShop.Application.Features.ProductInventory.Command.Create;
using OnlineShop.Application.Features.ProductInventory.Command.Update;
using OnlineShop.Application.Features.ProductInventory.Command.Delete;
using OnlineShop.Application.Features.ProductInventory.Queries.GetById;
using OnlineShop.Application.Features.ProductInventory.Queries.GetByProductId;
using OnlineShop.Application.Features.ProductInventory.Queries.GetAll;

namespace OnlineShop.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductInventoryController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ProductInventoryController> _logger;

        public ProductInventoryController(IMediator mediator, ILogger<ProductInventoryController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting all product inventories");
            var result = await _mediator.Send(new GetAllProductInventoriesQuery(), cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} product inventories", result.Data?.Count() ?? 0);
                return Ok(result);
            }
            
            _logger.LogWarning("Failed to retrieve product inventories: {Error}", result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting product inventory by ID: {InventoryId}", id);
            var result = await _mediator.Send(new GetProductInventoryByIdQuery { Id = id }, cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved product inventory: {InventoryId}", id);
                return Ok(result);
            }
            
            _logger.LogWarning("Product inventory not found: {InventoryId}", id);
            return NotFound(result);
        }

        [HttpGet("product/{productId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByProductId(Guid productId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting product inventory for product: {ProductId}", productId);
            var result = await _mediator.Send(new GetProductInventoryByProductIdQuery { ProductId = productId }, cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved product inventory for product: {ProductId}", productId);
                return Ok(result);
            }
            
            _logger.LogWarning("Product inventory not found for product: {ProductId}", productId);
            return NotFound(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateProductInventoryDto dto, CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("Creating product inventory for product: {ProductId} by user: {UserId}", dto.ProductId, userId);
            
            var command = new CreateProductInventoryCommand { ProductInventory = dto };
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Product inventory created successfully: {InventoryId} for product: {ProductId} by user: {UserId}", 
                    result.Data?.Id, dto.ProductId, userId);
                return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
            }
            
            _logger.LogWarning("Failed to create product inventory for product: {ProductId} by user: {UserId}. Error: {Error}", 
                dto.ProductId, userId, result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductInventoryDto dto, CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            dto.Id = id;
            _logger.LogInformation("Updating product inventory: {InventoryId} by user: {UserId}", id, userId);
            
            var command = new UpdateProductInventoryCommand { ProductInventory = dto };
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Product inventory updated successfully: {InventoryId} by user: {UserId}", id, userId);
                return Ok(result);
            }
            
            _logger.LogWarning("Failed to update product inventory: {InventoryId} by user: {UserId}. Error: {Error}", id, userId, result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("Deleting product inventory: {InventoryId} by user: {UserId}", id, userId);
            
            var command = new DeleteProductInventoryCommand { Id = id };
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Product inventory deleted successfully: {InventoryId} by user: {UserId}", id, userId);
                return NoContent();
            }
            
            _logger.LogWarning("Failed to delete product inventory: {InventoryId} by user: {UserId}. Error: {Error}", id, userId, result.ErrorMessage);
            return NotFound(result);
        }
    }
}
