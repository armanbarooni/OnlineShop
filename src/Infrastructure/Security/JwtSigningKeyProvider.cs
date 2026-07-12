using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace OnlineShop.Infrastructure.Security
{
    public static class JwtSigningKeyProvider
    {
        private const string DevelopmentFallbackSecret = "development-secret-placeholder-change-me";

        public static string GetSecret(IConfiguration configuration)
        {
            var secret = configuration.GetSection("Jwt")["Secret"];
            return string.IsNullOrWhiteSpace(secret) ? DevelopmentFallbackSecret : secret;
        }

        public static SymmetricSecurityKey CreateKey(IConfiguration configuration)
        {
            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetSecret(configuration)));
        }

        public static int GetConfiguredSecretLength(IConfiguration configuration)
        {
            return Encoding.UTF8.GetByteCount(configuration.GetSection("Jwt")["Secret"] ?? string.Empty);
        }
    }
}
