using OnlineShop.Application.DTOs.Auth;

namespace OnlineShop.Infrastructure.Services
{
    public interface ITokenService
    {
        Task<AuthResponseDto> GenerateTokensAsync(string email, IEnumerable<string> roles);
        Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken);
        Task RevokeTokenAsync(string refreshToken);
        Task RevokeAllUserTokensAsync(string email);
    }
}
