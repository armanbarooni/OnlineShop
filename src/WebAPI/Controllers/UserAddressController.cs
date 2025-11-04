using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserAddress;
using OnlineShop.Application.Features.UserAddress.Command.Create;
using OnlineShop.Application.Features.UserAddress.Command.Update;
using OnlineShop.Application.Features.UserAddress.Command.SetDefault;
using OnlineShop.Application.Features.UserAddress.Command.Delete;
using OnlineShop.Application.Features.UserAddress.Queries.GetByUserId;
using OnlineShop.Application.Features.UserAddress.Queries.GetAll;
using OnlineShop.Application.Features.UserAddress.Queries.GetById;
using OnlineShop.Application.Features.UserAddress.Queries.GetDefaultAddress;
using OnlineShop.Application.Features.UserAddress.Queries.SearchUserAddresses;

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

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Result<IEnumerable<UserAddressDto>>>> GetAll()
        {
            var result = await _mediator.Send(new GetAllUserAddressesQuery());
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Result<UserAddressDto>>> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetUserAddressByIdQuery { Id = id });
            if (!result.IsSuccess)
                return NotFound(result);
            return Ok(result);
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

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAddress(Guid id)
        {
            var result = await _mediator.Send(new DeleteUserAddressCommand { Id = id });
            if (!result.IsSuccess)
                return NotFound(result);

            return NoContent();
        }

        [HttpGet("default")]
        public async Task<ActionResult<Result<UserAddressDto>>> GetDefault()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null || !Guid.TryParse(userId, out var userGuid))
                return Unauthorized("User not authenticated");

            var result = await _mediator.Send(new GetDefaultUserAddressQuery { UserId = userGuid });
            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("search")]
        public async Task<ActionResult<Result<IEnumerable<UserAddressDto>>>> Search([FromQuery] string? q)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null || !Guid.TryParse(userId, out var userGuid))
                return Unauthorized("User not authenticated");

            var result = await _mediator.Send(new SearchUserAddressesQuery 
            { 
                UserId = userGuid,
                SearchQuery = q
            });
            
            return Ok(result);
        }
    }
}
