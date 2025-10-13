using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.ProductReview;
using OnlineShop.Application.Features.ProductReview.Command.Create;
using OnlineShop.Application.Features.ProductReview.Command.Update;
using OnlineShop.Application.Features.ProductReview.Command.Approve;
using OnlineShop.Application.Features.ProductReview.Command.Reject;
using OnlineShop.Application.Features.ProductReview.Command.Delete;
using OnlineShop.Application.Features.ProductReview.Queries.GetByProductId;
using OnlineShop.Application.Features.ProductReview.Queries.GetAll;
using OnlineShop.Application.Features.ProductReview.Queries.GetById;

namespace OnlineShop.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductReviewController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProductReviewController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<Result<IEnumerable<ProductReviewDto>>>> GetAll()
        {
            var result = await _mediator.Send(new GetAllProductReviewsQuery());
            return Ok(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<Result<ProductReviewDto>>> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetProductReviewByIdQuery { Id = id });
            if (!result.IsSuccess)
                return NotFound(result);
            return Ok(result);
        }

        [HttpGet("product/{productId}")]
        [AllowAnonymous]
        public async Task<ActionResult<Result<IEnumerable<ProductReviewDto>>>> GetByProductId(Guid productId)
        {
            var result = await _mediator.Send(new GetProductReviewsByProductIdQuery { ProductId = productId });
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<Result<ProductReviewDto>>> CreateReview([FromBody] CreateProductReviewDto review)
        {
            // Get current user ID from claims
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null || !Guid.TryParse(userId, out var userGuid))
                return Unauthorized("User not authenticated");

            var result = await _mediator.Send(new CreateProductReviewCommand 
            { 
                ProductReview = review,
                UserId = userGuid
            });
            
            if (!result.IsSuccess)
                return BadRequest(result);

            return CreatedAtAction(nameof(GetByProductId), new { productId = review.ProductId }, result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Result<ProductReviewDto>>> UpdateReview(Guid id, [FromBody] UpdateProductReviewDto review)
        {
            if (id != review.Id)
                return BadRequest("ID mismatch");

            var result = await _mediator.Send(new UpdateProductReviewCommand { ProductReview = review });
            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("{id}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Result<ProductReviewDto>>> ApproveReview(Guid id, [FromBody] ApproveProductReviewDto approveDto)
        {
            var result = await _mediator.Send(new ApproveProductReviewCommand 
            { 
                Id = id,
                AdminNotes = approveDto.AdminNotes
            });
            
            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("{id}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Result<ProductReviewDto>>> RejectReview(Guid id, [FromBody] RejectProductReviewDto rejectDto)
        {
            var result = await _mediator.Send(new RejectProductReviewCommand 
            { 
                Id = id,
                RejectionReason = rejectDto.RejectionReason,
                AdminNotes = rejectDto.AdminNotes
            });
            
            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteReview(Guid id)
        {
            var result = await _mediator.Send(new DeleteProductReviewCommand { Id = id });
            if (!result.IsSuccess)
                return NotFound(result);

            return NoContent();
        }
    }
}
