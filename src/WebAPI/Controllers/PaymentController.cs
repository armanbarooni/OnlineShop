using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Application.DTOs.Payment;
using OnlineShop.Application.Features.Payment.Commands.InitiatePayment;
using OnlineShop.Application.Features.Payment.Commands.VerifyPayment;
using System.Security.Claims;

namespace OnlineShop.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IMediator mediator, ILogger<PaymentController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost("initiate")]
        [Authorize]
        public async Task<IActionResult> InitiatePayment([FromBody] InitiatePaymentDto dto, CancellationToken cancellationToken)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            {
                return Unauthorized(new { message = "کاربر یافت نشد" });
            }

            var command = new InitiatePaymentCommand
            {
                UserId = userId,
                Request = dto
            };

            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyPayment([FromBody] VerifyPaymentDto dto, CancellationToken cancellationToken)
        {
            var command = new VerifyPaymentCommand
            {
                Request = dto
            };

            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        // Mock Gateway Callback Endpoint (For Testing)
        [HttpGet("mock-gateway")]
        public IActionResult MockGatewayCallback([FromQuery] string authority, [FromQuery] long amount, [FromQuery] string status)
        {
            // In a real scenario, the bank redirects the user to the frontend callback URL with these params.
            // Here, we simulate that redirection.
            // You should implement the VerifyPayment logic separately.
            
            // Redirect to Frontend Callback
            // return Redirect($"http://localhost:3000/payment/callback?authority={authority}&status={status}");
            
            return Ok(new { Message = "Payment processed by Mock Gateway", Authority = authority, Status = status, Amount = amount });
        }
    }
}
