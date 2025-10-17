using MediatR;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Application.DTOs.ProductCategory;
using OnlineShop.Application.Features.ProductCategory.Command.Create;
using OnlineShop.Application.Features.ProductCategory.Command.Delete;
using OnlineShop.Application.Features.ProductCategory.Command.Update;
using OnlineShop.Application.Features.ProductCategory.Queries.GetAll;
using OnlineShop.Application.Features.ProductCategory.Queries.GetById;
using OnlineShop.Application.Features.ProductCategory.Queries.GetCategoryTree;
using OnlineShop.Application.Features.ProductCategory.Queries.GetSubCategories;
using Microsoft.AspNetCore.Authorization;

namespace OnlineShop.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductCategoryController(IMediator mediator, ILogger<ProductCategoryController> logger) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;
        private readonly ILogger<ProductCategoryController> _logger = logger;

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting all product categories");
            var result = await _mediator.Send(new GetAllProductCategoriesQuery(), cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} product categories", result.Data?.Count() ?? 0);
                return Ok(result);
            }
            
            _logger.LogWarning("Failed to retrieve product categories: {Error}", result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting product category by ID: {CategoryId}", id);
            var result = await _mediator.Send(new GetProductCategoryByIdQuery { Id=id }, cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved product category: {CategoryId}", id);
                return Ok(result);
            }
            
            _logger.LogWarning("Product category not found: {CategoryId}", id);
            return NotFound(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(
            [FromBody] CreateProductCategoryDto dto,
            CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("Creating product category: {CategoryName} by user: {UserId}", dto.Name, userId);
            
            var command = new CreateProductCategoryCommand { Dto=dto };
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Product category created successfully: {CategoryId} by user: {UserId}", 
                    result.Data?.Id, userId);
                return Ok(result);
            }
            
            _logger.LogWarning("Failed to create product category: {CategoryName} by user: {UserId}. Error: {Error}", 
                dto.Name, userId, result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(
            [FromRoute] Guid id,
            [FromBody] UpdateProductCategoryDto dto,
            CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("Updating product category: {CategoryId} by user: {UserId}", id, userId);
            
            dto.Id = id;
            var command = new UpdateProductCategoryCommand {   Dto= dto };
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Product category updated successfully: {CategoryId} by user: {UserId}", id, userId);
                return Ok(result);
            }
            
            _logger.LogWarning("Failed to update product category: {CategoryId} by user: {UserId}. Error: {Error}", 
                id, userId, result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("Deleting product category: {CategoryId} by user: {UserId}", id, userId);
            
            var result = await _mediator.Send(new DeleteProductCategoryCommand { Id = id }, cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Product category deleted successfully: {CategoryId} by user: {UserId}", id, userId);
                return NoContent();
            }
            
            _logger.LogWarning("Failed to delete product category: {CategoryId} by user: {UserId}. Error: {Error}", 
                id, userId, result.ErrorMessage);
            return NotFound(result);
        }

        [HttpGet("tree")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCategoryTree(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting category tree");
            var result = await _mediator.Send(new GetCategoryTreeQuery(), cancellationToken);
            
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            
            return BadRequest(result);
        }

        [HttpGet("{id}/subcategories")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSubCategories([FromRoute] Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting subcategories for category: {CategoryId}", id);
            var result = await _mediator.Send(new GetSubCategoriesQuery(id), cancellationToken);
            
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            
            return BadRequest(result);
        }
    }
}


