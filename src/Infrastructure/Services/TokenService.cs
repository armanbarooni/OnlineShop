using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OnlineShop.Application.Contracts.Services;
using OnlineShop.Application.DTOs.Auth;
using OnlineShop.Domain.Entities;
using OnlineShop.Infrastructure.Persistence;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OnlineShop.Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public TokenService(
            IConfiguration configuration,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _configuration = configuration;
            _userManager = userManager;
            _context = context;
        }

        public async Task<AuthResponseDto> GenerateTokensAsync(string emailOrPhoneNumber, IEnumerable<string> roles)
        {
            // Try to find by email first, then by username (phone number)
            var user = await _userManager.FindByEmailAsync(emailOrPhoneNumber);
            if (user == null)
            {
                user = await _userManager.FindByNameAsync(emailOrPhoneNumber);
            }
            
            if (user == null)
                throw new InvalidOperationException("User not found");

            var jwtSection = _configuration.GetSection("Jwt");
            var issuer = jwtSection["Issuer"]!;
            var audience = jwtSection["Audience"]!;
            var secret = jwtSection["Secret"]!;
            var expiryMinutes = int.TryParse(jwtSection["ExpiryMinutes"], out var m) ? m : 60;
            var refreshDays = int.TryParse(jwtSection["RefreshExpiryDays"], out var d) ? d : 14;

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Email ?? user.UserName ?? emailOrPhoneNumber),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(ClaimTypes.Name, user.UserName ?? emailOrPhoneNumber),
                new(ClaimTypes.NameIdentifier, user.Id.ToString())
            };
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(expiryMinutes);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
            var refreshToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            // Store refresh token in database
            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(refreshDays),
                UserId = user.Id
            };

            _context.RefreshTokens.Add(refreshTokenEntity);
            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = expires,
                Email = user.Email ?? user.UserName ?? emailOrPhoneNumber,
                Roles = roles
            };
        }

        public async Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken)
        {
            var tokenEntity = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken && 
                                         !rt.IsRevoked && 
                                         rt.ExpiresAt > DateTime.UtcNow);

            if (tokenEntity == null)
                return null;

            var user = tokenEntity.User;
            var roles = await _userManager.GetRolesAsync(user);

            // Revoke the old refresh token
            tokenEntity.IsRevoked = true;
            tokenEntity.RevokedAt = DateTime.UtcNow;

            // Generate new tokens
            return await GenerateTokensAsync(user.Email!, roles);
        }

        public async Task RevokeTokenAsync(string refreshToken)
        {
            var tokenEntity = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (tokenEntity != null)
            {
                tokenEntity.IsRevoked = true;
                tokenEntity.RevokedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task RevokeAllUserTokensAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return;

            var tokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == user.Id && !rt.IsRevoked)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }
    }
}
