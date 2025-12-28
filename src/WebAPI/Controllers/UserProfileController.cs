using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
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
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<UserProfileController> _logger;

        public UserProfileController(IMediator mediator, IWebHostEnvironment environment, ILogger<UserProfileController> logger)
        {
            _mediator = mediator;
            _environment = environment;
            _logger = logger;
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

        [HttpPut]
        public async Task<ActionResult<Result<UserProfileDto>>> UpdateCurrentUserProfile([FromBody] UpdateUserProfileDto profile)
        {
            // Get current user ID from claims
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null || !Guid.TryParse(userId, out var userGuid))
                return Unauthorized("User not authenticated");

            // Get user profile by user ID
            var getProfileResult = await _mediator.Send(new GetUserProfileByUserIdQuery { UserId = userGuid });
            if (!getProfileResult.IsSuccess || getProfileResult.Data == null)
                return NotFound(new { message = "پروفایل کاربر یافت نشد" });

            // Set the profile ID from the found profile
            profile.Id = getProfileResult.Data.Id;

            var result = await _mediator.Send(new UpdateUserProfileCommand { UserProfile = profile });
            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
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

        [HttpPost("upload-picture")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult> UploadProfilePicture(IFormFile file)
        {
            // Get current user ID from claims
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null || !Guid.TryParse(userId, out var userGuid))
                return Unauthorized(new { message = "User not authenticated" });

            // Validate file
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "فایل انتخاب نشده است" });

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
                return BadRequest(new { message = "فرمت فایل مجاز نیست. فقط تصاویر JPG، PNG، GIF و WEBP مجاز است" });

            // Validate file size (max 5MB)
            const long maxFileSize = 5 * 1024 * 1024; // 5MB
            if (file.Length > maxFileSize)
                return BadRequest(new { message = "حجم فایل نباید بیشتر از 5 مگابایت باشد" });

            try
            {
                // Get user profile
                var getProfileResult = await _mediator.Send(new GetUserProfileByUserIdQuery { UserId = userGuid });
                if (!getProfileResult.IsSuccess || getProfileResult.Data == null)
                    return NotFound(new { message = "پروفایل کاربر یافت نشد" });

                var profile = getProfileResult.Data;

                // Create uploads directory if it doesn't exist
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "profile-pictures");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // Generate unique filename
                var fileName = $"{userGuid}_{DateTime.UtcNow:yyyyMMddHHmmss}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Generate URL (relative to wwwroot)
                var imageUrl = $"/uploads/profile-pictures/{fileName}";

                // Update profile with new image URL
                var updateDto = new UpdateUserProfileDto
                {
                    Id = profile.Id,
                    FirstName = profile.FirstName,
                    LastName = profile.LastName,
                    NationalCode = profile.NationalCode,
                    BirthDate = profile.BirthDate,
                    Gender = profile.Gender,
                    ProfileImageUrl = imageUrl,
                    Bio = profile.Bio,
                    Website = profile.Website
                };

                var updateResult = await _mediator.Send(new UpdateUserProfileCommand { UserProfile = updateDto });
                if (!updateResult.IsSuccess)
                {
                    // Delete uploaded file if update failed
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);
                    return BadRequest(new { message = updateResult.ErrorMessage ?? "خطا در بروزرسانی پروفایل" });
                }

                _logger.LogInformation("Profile picture uploaded successfully for user: {UserId}, URL: {ImageUrl}", userGuid, imageUrl);
                return Ok(new { 
                    imageUrl = imageUrl,
                    message = "تصویر پروفایل با موفقیت آپلود شد"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading profile picture for user: {UserId}", userGuid);
                return StatusCode(500, new { message = "خطا در آپلود تصویر پروفایل" });
            }
        }
    }
}
