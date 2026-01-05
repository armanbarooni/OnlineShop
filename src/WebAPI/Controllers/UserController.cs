using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Application.DTOs.User;
using OnlineShop.Application.DTOs.Order; // Need this namespace for OrderDto
using OnlineShop.Application.Features.User.Commands.CreateAddress;
using OnlineShop.Application.Features.User.Commands.DeleteAddress;
using OnlineShop.Application.Features.User.Commands.UpdateProfile;
using OnlineShop.Application.Features.User.Queries.GetAddresses;
using OnlineShop.Application.Features.User.Queries.GetOrder;
using OnlineShop.Application.Features.User.Queries.GetOrders;
using OnlineShop.Application.Features.User.Queries.GetProfile;
using System.Security.Claims;

namespace OnlineShop.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<UserController> _logger;

        public UserController(IMediator mediator, ILogger<UserController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        private Guid GetUserId()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            {
                throw new UnauthorizedAccessException("کاربر معتبر نیست");
            }
            return userId;
        }

        // --- Profile ---

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
        {
            var command = new GetUserProfileQuery { UserId = GetUserId() };
            var result = await _mediator.Send(command, cancellationToken);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto, CancellationToken cancellationToken)
        {
            var command = new UpdateProfileCommand { UserId = GetUserId(), Request = dto };
            var result = await _mediator.Send(command, cancellationToken);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        // --- Orders ---

        [HttpGet("orders")]
        public async Task<IActionResult> GetOrders(CancellationToken cancellationToken)
        {
            var command = new GetUserOrdersQuery { UserId = GetUserId() };
            var result = await _mediator.Send(command, cancellationToken);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("orders/{id}")]
        public async Task<IActionResult> GetOrderDetails(Guid id, CancellationToken cancellationToken)
        {
            var command = new GetUserOrderQuery { UserId = GetUserId(), OrderId = id };
            var result = await _mediator.Send(command, cancellationToken);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        // --- Addresses ---

        [HttpGet("addresses")]
        public async Task<IActionResult> GetAddresses(CancellationToken cancellationToken)
        {
            var command = new GetUserAddressesQuery { UserId = GetUserId() };
            var result = await _mediator.Send(command, cancellationToken);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("addresses")]
        public async Task<IActionResult> CreateAddress([FromBody] CreateAddressDto dto, CancellationToken cancellationToken)
        {
            var command = new CreateAddressCommand { UserId = GetUserId(), Request = dto };
            var result = await _mediator.Send(command, cancellationToken);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("addresses/{id}")]
        public async Task<IActionResult> DeleteAddress(Guid id, CancellationToken cancellationToken)
        {
            var command = new DeleteAddressCommand { UserId = GetUserId(), AddressId = id };
            var result = await _mediator.Send(command, cancellationToken);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
