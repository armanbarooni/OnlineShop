using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Application.DTOs.ProductVariant;
using OnlineShop.Application.Features.ProductVariant.Commands.Create;
using OnlineShop.Application.Features.ProductVariant.Commands.Update;
using OnlineShop.Application.Features.ProductVariant.Commands.Delete;
using OnlineShop.Application.Features.ProductVariant.Queries.GetAll;
using OnlineShop.Application.Features.ProductVariant.Queries.GetById;
using OnlineShop.Application.Features.ProductVariant.Queries.GetByProductId;

namespace OnlineShop.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductVariantController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ProductVariantController> _logger;

        public ProductVariantController(IMediator mediator, ILogger<ProductVariantController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting all product variants");
            var result = await _mediator.Send(new GetAllProductVariantsQuery(), cancellationToken);
            
            if (result.IsSuccess)
                return Ok(result);
            
            return BadRequest(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting product variant by ID: {ProductVariantId}", id);
            var result = await _mediator.Send(new GetProductVariantByIdQuery { Id = id }, cancellationToken);
            
            if (result.IsSuccess)
                return Ok(result);
            
            return NotFound(result);
        }

        [HttpGet("product/{productId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByProductId([FromRoute] Guid productId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting product variants for product ID: {ProductId}", productId);
            var result = await _mediator.Send(new GetProductVariantsByProductIdQuery { ProductId = productId }, cancellationToken);
            
            if (result.IsSuccess)
                return Ok(result);
            
            return BadRequest(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateProductVariantDto dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating product variant: {SKU}", dto.SKU);
            var command = new CreateProductVariantCommand { ProductVariant = dto };
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
                return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
            
            return BadRequest(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateProductVariantDto dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating product variant: {ProductVariantId}", id);
            dto.Id = id;
            var command = new UpdateProductVariantCommand { ProductVariant = dto };
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
                return Ok(result);
            
            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting product variant: {ProductVariantId}", id);
            var result = await _mediator.Send(new DeleteProductVariantCommand(id), cancellationToken);
            
            if (result.IsSuccess)
                return NoContent();
            
            return NotFound(result);
        }
    }
}
