using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace GlobalGameJam2025_Bubbles.Infrastructure;

public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public BasicAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // 1. Check if the request has an Authorization header
        if (!Request.Headers.ContainsKey("Authorization"))
        {
            return Task.FromResult(AuthenticateResult.Fail("Missing Authorization Header"));
        }

        try
        {
            // 2. Parse the header
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            if (!"Basic".Equals(authHeader.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Scheme"));
            }

            // 3. Decode Base64 username:password
            var credentialBytes = Convert.FromBase64String(authHeader.Parameter ?? "");
            var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);
            var username = credentials[0];
            var password = credentials.Length > 1 ? credentials[1] : string.Empty;

            // 4. Validate the user credentials (simple hard-coded check here)
            if (username == Environment.GetEnvironmentVariable("BASIC_AUTH_USERNAME") && password == Environment.GetEnvironmentVariable("BASIC_AUTH_PASSWORD"))
            {
                // 5. Create an authenticated identity
                var claims = new[] { new Claim(ClaimTypes.Name, username) };
                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                return Task.FromResult(AuthenticateResult.Success(ticket));
            }
            else
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid Username or Password"));
            }
        }
        catch
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header"));
        }
    }
    
    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        // 401 status
        Response.StatusCode = 401;
        // Include the Basic auth challenge header
        Response.Headers["WWW-Authenticate"] = @"Basic realm=""MySecureArea""";

        return Task.CompletedTask;
    }
}