using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using OnlineShop.Domain.Entities;

namespace OnlineShop.IntegrationTests.Helpers
{
    public class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Only authenticate if Authorization header is present
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                Logger.LogDebug("No Authorization header found");
                return AuthenticateResult.NoResult();
            }

            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            Logger.LogDebug($"Authorization header found: {authHeader}");

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                Logger.LogDebug("Invalid authorization header format");
                return AuthenticateResult.NoResult();
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();
            
            // Determine user role based on token content
            string role = "User"; // Default role
            string userId = "70f4a38e-b17e-4fb3-a7af-65046ee8b8c9"; // Real user ID from database
            string email = "user@test.com";
            
            // Token-based role detection - check for specific patterns
            if (token.Contains("admin_test_token"))
            {
                role = "Admin";
                userId = "d72b45e6-5396-439e-a05c-d48570bac58d"; // Real admin ID from database
                email = "admin@test.com";
            }
            else if (token.Contains("user_test_token"))
            {
                role = "User";
                userId = "70f4a38e-b17e-4fb3-a7af-65046ee8b8c9"; // Real user ID from database
                email = "user@test.com";
            }
            else if (token == "test-token")
            {
                // Default test token - treat as User
                role = "User";
                userId = "70f4a38e-b17e-4fb3-a7af-65046ee8b8c9"; // Real user ID from database
                email = "user@test.com";
            }

            // Use the real user IDs from the database
            Logger.LogDebug($"Using user ID: {userId} for email: {email}");

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
            return AuthenticateResult.Success(ticket);
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
