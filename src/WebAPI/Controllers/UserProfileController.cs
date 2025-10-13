using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserProfile;
using OnlineShop.Application.Features.UserProfile.Command.Create;
using OnlineShop.Application.Features.UserProfile.Command.Update;
using OnlineShop.Application.Features.UserProfile.Command.Delete;
using OnlineShop.Application.Features.UserProfile.Queries.GetByUserId;
using OnlineShop.Application.Features.UserProfile.Queries.GetAll;
using OnlineShop.Application.Features.UserProfile.Queries.GetById;

namespace OnlineShop.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserProfileController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserProfileController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Result<IEnumerable<UserProfileDto>>>> GetAll()
        {
            var result = await _mediator.Send(new GetAllUserProfilesQuery());
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Result<UserProfileDto>>> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetUserProfileByIdQuery { Id = id });
            if (!result.IsSuccess)
                return NotFound(result);
            return Ok(result);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<Result<UserProfileDto?>>> GetByUserId(Guid userId)
        {
            var result = await _mediator.Send(new GetUserProfileByUserIdQuery { UserId = userId });
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<Result<UserProfileDto>>> CreateProfile([FromBody] CreateUserProfileDto profile)
        {
            // Get current user ID from claims
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null || !Guid.TryParse(userId, out var userGuid))
                return Unauthorized("User not authenticated");

            profile.UserId = userGuid;

            var result = await _mediator.Send(new CreateUserProfileCommand { UserProfile = profile });
            if (!result.IsSuccess)
                return BadRequest(result);

            return CreatedAtAction(nameof(GetByUserId), new { userId = userGuid }, result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Result<UserProfileDto>>> UpdateProfile(Guid id, [FromBody] UpdateUserProfileDto profile)
        {
            if (id != profile.Id)
                return BadRequest("ID mismatch");

            var result = await _mediator.Send(new UpdateUserProfileCommand { UserProfile = profile });
            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteProfile(Guid id)
        {
            var result = await _mediator.Send(new DeleteUserProfileCommand { Id = id });
            if (!result.IsSuccess)
                return NotFound(result);

            return NoContent();
        }
    }
}
