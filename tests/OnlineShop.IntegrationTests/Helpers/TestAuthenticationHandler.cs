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

            // Create a test user with admin role
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "d869562d-b70b-4b55-9589-08a7c7584a24"),
                new Claim(ClaimTypes.Name, "admin@test.com"),
                new Claim(ClaimTypes.Email, "admin@test.com"),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim("sub", "d869562d-b70b-4b55-9589-08a7c7584a24")
            };

            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Test");

            Logger.LogDebug("Test authentication successful");
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
