using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Application.DTOs.Checkout;
using OnlineShop.Application.Features.Checkout.Commands.ProcessCheckout;

namespace OnlineShop.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CheckoutController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CheckoutController> _logger;

        public CheckoutController(IMediator mediator, ILogger<CheckoutController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> ProcessCheckout([FromBody] CheckoutRequestDto request, CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null || !Guid.TryParse(userId, out var userGuid))
                return Unauthorized("User not authenticated");

            _logger.LogInformation("Processing checkout for user: {UserId}, Cart: {CartId}", userGuid, request.CartId);

            var command = new ProcessCheckoutCommand
            {
                UserId = userGuid,
                Request = request
            };

            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Checkout successful for user: {UserId}, Order: {OrderId}", 
                    userGuid, result.Data?.Order.Id);
                return Ok(result);
            }

            _logger.LogWarning("Checkout failed for user: {UserId}. Error: {Error}", userGuid, result.ErrorMessage);
            return BadRequest(result);
        }
    }
}

