using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CommandDeck.Host.Tests.Authentication;

// Test-only authentication scaffold. Production Host composition must not
// accept these headers as an authentication mechanism.
public sealed class TestAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public new const string Scheme = "Test";
    public const string ProviderHeader = "X-Test-Provider";
    public const string SubjectHeader = "X-Test-Subject";
    public const string DisplayNameHeader = "X-Test-DisplayName";
    public const string EmailHeader = "X-Test-Email";
    public const string AccessHeader = "X-Test-HasAccess";
    public const string InvalidHeader = "X-Test-Invalid";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (Request.Headers.ContainsKey(InvalidHeader))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid test authentication context."));
        }

        if (!Request.Headers.TryGetValue(SubjectHeader, out var subject))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new List<Claim>
        {
            new("provider", Request.Headers[ProviderHeader].FirstOrDefault() ?? "test"),
            new(ClaimTypes.NameIdentifier, subject.ToString()),
        };

        AddOptionalClaim(claims, ClaimTypes.Name, Request.Headers[DisplayNameHeader].FirstOrDefault());
        AddOptionalClaim(claims, ClaimTypes.Email, Request.Headers[EmailHeader].FirstOrDefault());
        AddOptionalClaim(claims, "application_access", Request.Headers[AccessHeader].FirstOrDefault());

        var identity = new ClaimsIdentity(claims, Scheme);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    private static void AddOptionalClaim(List<Claim> claims, string type, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            claims.Add(new Claim(type, value));
        }
    }
}
