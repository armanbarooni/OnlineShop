using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Application.DTOs.Auth;
using OnlineShop.Domain.Entities;
using OnlineShop.Infrastructure.Services;

namespace OnlineShop.WebAPI.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class AuthController : ControllerBase
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly ITokenService _tokenService;
		private readonly ILogger<AuthController> _logger;

		public AuthController(
			UserManager<ApplicationUser> userManager,
			SignInManager<ApplicationUser> signInManager,
			ITokenService tokenService,
			ILogger<AuthController> logger)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_tokenService = tokenService;
			_logger = logger;
		}

		[HttpPost("login")]
		[ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		public async Task<IActionResult> Login([FromBody] LoginDto dto)
		{
			_logger.LogInformation("Login attempt for email: {Email}", dto.Email);

			if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
			{
				_logger.LogWarning("Login failed - missing email or password for {Email}", dto.Email);
				return Unauthorized("Email and password are required");
			}

			var user = await _userManager.FindByEmailAsync(dto.Email);
			if (user == null)
			{
				_logger.LogWarning("Login failed - user not found for email: {Email}", dto.Email);
				return Unauthorized("Invalid credentials");
			}

			var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: false);
			if (!result.Succeeded)
			{
				_logger.LogWarning("Login failed - invalid password for user: {UserId}", user.Id);
				return Unauthorized("Invalid credentials");
			}

			// Update last login
			user.LastLoginAt = DateTime.UtcNow;
			await _userManager.UpdateAsync(user);

			var roles = await _userManager.GetRolesAsync(user);
			var tokens = await _tokenService.GenerateTokensAsync(dto.Email, roles);
			
			_logger.LogInformation("Login successful for user: {UserId} with roles: {Roles}", user.Id, string.Join(",", roles));
			return Ok(tokens);
		}

		[HttpPost("refresh")]
		[ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto dto)
		{
			_logger.LogInformation("Token refresh attempt");

			if (string.IsNullOrWhiteSpace(dto.RefreshToken))
			{
				_logger.LogWarning("Token refresh failed - missing refresh token");
				return Unauthorized("Refresh token is required");
			}

			var tokens = await _tokenService.RefreshTokenAsync(dto.RefreshToken);
			if (tokens == null)
			{
				_logger.LogWarning("Token refresh failed - invalid or expired refresh token");
				return Unauthorized("Invalid or expired refresh token");
			}

			_logger.LogInformation("Token refresh successful for user: {Email}", tokens.Email);
			return Ok(tokens);
		}

		[HttpPost("register")]
		[ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> Register([FromBody] RegisterDto dto)
		{
			_logger.LogInformation("User registration attempt for email: {Email}", dto.Email);

			if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
			{
				_logger.LogWarning("Registration failed - missing email or password for {Email}", dto.Email);
				return BadRequest("Email and password are required");
			}

			var existingUser = await _userManager.FindByEmailAsync(dto.Email);
			if (existingUser != null)
			{
				_logger.LogWarning("Registration failed - user already exists for email: {Email}", dto.Email);
				return BadRequest("User with this email already exists");
			}

			var user = new ApplicationUser
			{
				UserName = dto.Email,
				Email = dto.Email,
				FirstName = dto.FirstName ?? string.Empty,
				LastName = dto.LastName ?? string.Empty
			};

			var result = await _userManager.CreateAsync(user, dto.Password);
			if (!result.Succeeded)
			{
				_logger.LogWarning("Registration failed - validation errors for {Email}: {Errors}", 
					dto.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
				return BadRequest(result.Errors.Select(e => e.Description));
			}

			// Assign default role
			await _userManager.AddToRoleAsync(user, "User");

			var roles = await _userManager.GetRolesAsync(user);
			var tokens = await _tokenService.GenerateTokensAsync(dto.Email, roles);

			_logger.LogInformation("User registration successful for user: {UserId} with email: {Email}", user.Id, dto.Email);
			return CreatedAtAction(nameof(Login), tokens);
		}

		[HttpPost("logout")]
		[Authorize]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public async Task<IActionResult> Logout([FromBody] RefreshTokenDto dto)
		{
			var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			_logger.LogInformation("Logout attempt for user: {UserId}", userId);

			if (!string.IsNullOrWhiteSpace(dto.RefreshToken))
			{
				await _tokenService.RevokeTokenAsync(dto.RefreshToken);
				_logger.LogInformation("Refresh token revoked for user: {UserId}", userId);
			}

			_logger.LogInformation("Logout successful for user: {UserId}", userId);
			return Ok(new { message = "Logged out successfully" });
		}
	}
}
