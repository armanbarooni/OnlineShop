using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.ProductDetail;
using OnlineShop.Application.Features.ProductDetail.Command.Create;
using OnlineShop.Application.Features.ProductDetail.Command.Delete;
using OnlineShop.Application.Features.ProductDetail.Command.Update;
using OnlineShop.Application.Features.ProductDetail.Queries.GetAll;
using OnlineShop.Application.Features.ProductDetail.Queries.GetById;

namespace OnlineShop.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductDetailController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProductDetailController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<Result<IEnumerable<ProductDetailDto>>>> GetAll()
        {
            var result = await _mediator.Send(new GetAllProductDetailsQuery());
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Result<ProductDetailDto>>> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetProductDetailByIdQuery { Id = id });
            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<Result<ProductDetailDto>>> Create([FromBody] CreateProductDetailDto productDetail)
        {
            var result = await _mediator.Send(new CreateProductDetailCommand { ProductDetail = productDetail });
            if (!result.IsSuccess)
                return BadRequest(result);

            return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Result<ProductDetailDto>>> Update(Guid id, [FromBody] UpdateProductDetailDto productDetail)
        {
            if (id != productDetail.Id)
                return BadRequest("ID mismatch");

            var result = await _mediator.Send(new UpdateProductDetailCommand { ProductDetail = productDetail });
            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Result<bool>>> Delete(Guid id)
        {
            var result = await _mediator.Send(new DeleteProductDetailCommand { Id = id });
            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }
    }
}
