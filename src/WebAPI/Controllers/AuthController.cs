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

		public AuthController(
			UserManager<ApplicationUser> userManager,
			SignInManager<ApplicationUser> signInManager,
			ITokenService tokenService)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_tokenService = tokenService;
		}

		[HttpPost("login")]
		[ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		public async Task<IActionResult> Login([FromBody] LoginDto dto)
		{
			if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
				return Unauthorized("Email and password are required");

			var user = await _userManager.FindByEmailAsync(dto.Email);
			if (user == null)
				return Unauthorized("Invalid credentials");

			var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: false);
			if (!result.Succeeded)
				return Unauthorized("Invalid credentials");

			// Update last login
			user.LastLoginAt = DateTime.UtcNow;
			await _userManager.UpdateAsync(user);

			var roles = await _userManager.GetRolesAsync(user);
			var tokens = await _tokenService.GenerateTokensAsync(dto.Email, roles);
			
			return Ok(tokens);
		}

		[HttpPost("refresh")]
		[ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto dto)
		{
			if (string.IsNullOrWhiteSpace(dto.RefreshToken))
				return Unauthorized("Refresh token is required");

			var tokens = await _tokenService.RefreshTokenAsync(dto.RefreshToken);
			if (tokens == null)
				return Unauthorized("Invalid or expired refresh token");

			return Ok(tokens);
		}

		[HttpPost("register")]
		[ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> Register([FromBody] RegisterDto dto)
		{
			if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
				return BadRequest("Email and password are required");

			var existingUser = await _userManager.FindByEmailAsync(dto.Email);
			if (existingUser != null)
				return BadRequest("User with this email already exists");

			var user = new ApplicationUser
			{
				UserName = dto.Email,
				Email = dto.Email,
				FirstName = dto.FirstName ?? string.Empty,
				LastName = dto.LastName ?? string.Empty
			};

			var result = await _userManager.CreateAsync(user, dto.Password);
			if (!result.Succeeded)
				return BadRequest(result.Errors.Select(e => e.Description));

			// Assign default role
			await _userManager.AddToRoleAsync(user, "User");

			var roles = await _userManager.GetRolesAsync(user);
			var tokens = await _tokenService.GenerateTokensAsync(dto.Email, roles);

			return CreatedAtAction(nameof(Login), tokens);
		}

		[HttpPost("logout")]
		[Authorize]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public async Task<IActionResult> Logout([FromBody] RefreshTokenDto dto)
		{
			if (!string.IsNullOrWhiteSpace(dto.RefreshToken))
			{
				await _tokenService.RevokeTokenAsync(dto.RefreshToken);
			}

			return Ok(new { message = "Logged out successfully" });
		}
	}
}
