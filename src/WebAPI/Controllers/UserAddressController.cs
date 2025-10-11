using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserAddress;
using OnlineShop.Application.Features.UserAddress.Command.Create;
using OnlineShop.Application.Features.UserAddress.Command.Update;
using OnlineShop.Application.Features.UserAddress.Queries.GetByUserId;

namespace OnlineShop.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserAddressController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserAddressController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<Result<IEnumerable<UserAddressDto>>>> GetByUserId(Guid userId)
        {
            var result = await _mediator.Send(new GetUserAddressesByUserIdQuery { UserId = userId });
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<Result<UserAddressDto>>> CreateAddress([FromBody] CreateUserAddressDto address)
        {
            // Get current user ID from claims
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null || !Guid.TryParse(userId, out var userGuid))
                return Unauthorized("User not authenticated");

            var result = await _mediator.Send(new CreateUserAddressCommand 
            { 
                UserAddress = address,
                UserId = userGuid
            });
            
            if (!result.IsSuccess)
                return BadRequest(result);

            return CreatedAtAction(nameof(GetByUserId), new { userId = userGuid }, result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Result<UserAddressDto>>> UpdateAddress(Guid id, [FromBody] UpdateUserAddressDto address)
        {
            if (id != address.Id)
                return BadRequest("ID mismatch");

            var result = await _mediator.Send(new UpdateUserAddressCommand { UserAddress = address });
            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("{id}/set-default")]
        public async Task<ActionResult<Result<bool>>> SetAsDefault(Guid id)
        {
            // Get current user ID from claims
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null || !Guid.TryParse(userId, out var userGuid))
                return Unauthorized("User not authenticated");

            var result = await _mediator.Send(new SetDefaultUserAddressCommand 
            { 
                UserId = userGuid,
                AddressId = id
            });
            
            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
