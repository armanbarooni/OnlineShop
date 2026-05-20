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
        private readonly IConfiguration _configuration;

        public PaymentController(IMediator mediator, ILogger<PaymentController> logger, IConfiguration configuration)
        {
            _mediator = mediator;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// شروع فرآیند پرداخت - کاربر باید لاگین باشد
        /// </summary>
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

        /// <summary>
        /// Callback endpoint از زرین‌پال - زرین‌پال کاربر رو با GET به اینجا ریدایرکت می‌کنه
        /// Query params: Authority, Status (OK/NOK)
        /// </summary>
        [HttpGet("callback")]
        [AllowAnonymous]
        public async Task<IActionResult> PaymentCallback(
            [FromQuery] string Authority,
            [FromQuery] string Status,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "ZarinPal callback received. Authority: {Authority}, Status: {Status}",
                Authority, Status);

            var frontendResultUrl = _configuration["ZarinPal:FrontendResultUrl"]
                ?? "https://yoursite.com/payment/result";

            // اگر Status برابر NOK باشد، یعنی تراکنش ناموفق بوده یا کاربر لغو کرده
            if (!string.Equals(Status, "OK", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Payment cancelled or failed by user. Authority: {Authority}", Authority);

                return Redirect($"{frontendResultUrl}?status=failed&authority={Authority}&message=پرداخت+لغو+شد");
            }

            // Verify payment with ZarinPal
            var command = new VerifyPaymentCommand
            {
                Request = new VerifyPaymentDto
                {
                    Authority = Authority,
                    Status = Status
                }
            };

            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess && result.Data?.IsSuccess == true)
            {
                _logger.LogInformation(
                    "Payment verified successfully. Authority: {Authority}, RefId: {RefId}",
                    Authority, result.Data.RefId);

                return Redirect(
                    $"{frontendResultUrl}?status=success&refId={result.Data.RefId}&orderId={result.Data.OrderId}&message=پرداخت+موفق");
            }
            else
            {
                var errorMessage = result.Data?.Message ?? result.ErrorMessage ?? "خطای نامشخص";
                _logger.LogWarning(
                    "Payment verification failed. Authority: {Authority}, Message: {Message}",
                    Authority, errorMessage);

                return Redirect(
                    $"{frontendResultUrl}?status=failed&authority={Authority}&message={Uri.EscapeDataString(errorMessage)}");
            }
        }

        /// <summary>
        /// API endpoint برای verify مستقیم (مثلاً از طرف فرانت‌اند)
        /// </summary>
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
    }
}
