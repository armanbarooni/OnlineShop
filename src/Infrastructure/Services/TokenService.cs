using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using OnlineShop.Application.Contracts.Services;
using OnlineShop.Application.DTOs.Auth;
using OnlineShop.Domain.Entities;
using OnlineShop.Infrastructure.Persistence;
using OnlineShop.Infrastructure.Security;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Text;

namespace OnlineShop.Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TokenService> _logger;

        public TokenService(
            IConfiguration configuration,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            ILogger<TokenService> logger)
        {
            _configuration = configuration;
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        public async Task<AuthResponseDto> GenerateTokensAsync(string emailOrPhoneNumber, IEnumerable<string> roles)
        {
            // Try to find by email first, then by username, then by phone number
            var user = await _userManager.FindByEmailAsync(emailOrPhoneNumber);
            if (user == null)
            {
                user = await _userManager.FindByNameAsync(emailOrPhoneNumber);
            }
            if (user == null)
            {
                // Try to find by phone number
                user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == emailOrPhoneNumber);
            }
            
            if (user == null)
                throw new InvalidOperationException("User not found");

            var jwtSection = _configuration.GetSection("Jwt");
            var issuer = jwtSection["Issuer"]!;
            var audience = jwtSection["Audience"]!;
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

            var key = JwtSigningKeyProvider.CreateKey(_configuration);
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

            _logger.LogInformation(
                "Generated auth tokens for user {UserId}. AccessExpiresAt={AccessExpiresAt:o}, RefreshExpiresAt={RefreshExpiresAt:o}, RefreshTokenHash={RefreshTokenHash}",
                user.Id,
                expires,
                refreshTokenEntity.ExpiresAt,
                GetTokenHash(refreshToken));

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
            var tokenHash = GetTokenHash(refreshToken);
            _logger.LogInformation("Refresh token lookup started. RefreshTokenHash={RefreshTokenHash}", tokenHash);

            var tokenEntity = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (tokenEntity == null)
            {
                _logger.LogWarning("Refresh token rejected: not_found. RefreshTokenHash={RefreshTokenHash}", tokenHash);
                return null;
            }

            if (tokenEntity.IsRevoked)
            {
                _logger.LogWarning(
                    "Refresh token rejected: revoked. RefreshTokenHash={RefreshTokenHash}, UserId={UserId}, RevokedAt={RevokedAt:o}, ExpiresAt={ExpiresAt:o}",
                    tokenHash,
                    tokenEntity.UserId,
                    tokenEntity.RevokedAt,
                    tokenEntity.ExpiresAt);
                return null;
            }

            if (tokenEntity.ExpiresAt <= DateTime.UtcNow)
            {
                _logger.LogWarning(
                    "Refresh token rejected: expired. RefreshTokenHash={RefreshTokenHash}, UserId={UserId}, ExpiresAt={ExpiresAt:o}, UtcNow={UtcNow:o}",
                    tokenHash,
                    tokenEntity.UserId,
                    tokenEntity.ExpiresAt,
                    DateTime.UtcNow);
                return null;
            }

            if (tokenEntity.User == null)
            {
                _logger.LogWarning("Refresh token rejected: user_missing. RefreshTokenHash={RefreshTokenHash}, UserId={UserId}", tokenHash, tokenEntity.UserId);
                return null;
            }

            var user = tokenEntity.User;
            var roles = await _userManager.GetRolesAsync(user);

            // Revoke the old refresh token
            tokenEntity.IsRevoked = true;
            tokenEntity.RevokedAt = DateTime.UtcNow;

            var loginIdentifier = user.Email ?? user.UserName ?? user.PhoneNumber;
            if (string.IsNullOrWhiteSpace(loginIdentifier))
            {
                _logger.LogWarning("Refresh token rejected: user_identifier_missing. RefreshTokenHash={RefreshTokenHash}, UserId={UserId}", tokenHash, user.Id);
                return null;
            }

            // Generate new tokens with the identifier that exists for this user.
            var tokens = await GenerateTokensAsync(loginIdentifier, roles);
            _logger.LogInformation("Refresh token accepted. OldRefreshTokenHash={OldRefreshTokenHash}, UserId={UserId}", tokenHash, user.Id);
            return tokens;
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

        private static string GetTokenHash(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return "<empty>";
            }

            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToHexString(hash)[..16];
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
