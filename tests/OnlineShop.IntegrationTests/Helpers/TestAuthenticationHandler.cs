using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace OnlineShop.IntegrationTests.Helpers
{
    public class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Only authenticate if Authorization header is present
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                Logger.LogDebug("No Authorization header found");
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            Logger.LogDebug($"Authorization header found: {authHeader}");

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                Logger.LogDebug("Invalid authorization header format");
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();
            
            // Determine user role based on token content or use a simple mapping
            string role = "User"; // Default role
            string userId = "d869562d-b70b-4b55-9589-08a7c7584a24";
            string email = "user@test.com";
            
            // Simple token-based role detection
            if (token.Contains("admin") || token.Length > 50) // Admin tokens are typically longer
            {
                role = "Admin";
                userId = "d869562d-b70b-4b55-9589-08a7c7584a24";
                email = "admin@test.com";
            }
            else if (token.Contains("user"))
            {
                role = "User";
                userId = "d869562d-b70b-4b55-9589-08a7c7584a24";
                email = "user@test.com";
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role),
                new Claim("sub", userId)
            };

            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Test");

            Logger.LogDebug($"Test authentication successful for {role} user: {email}");
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

    public static class TestAuthenticationExtensions
    {
        public static AuthenticationBuilder AddTestAuthentication(this AuthenticationBuilder builder)
        {
            return builder.AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("Test", options => { });
        }
    }
}
