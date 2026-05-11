using System.Security.Claims;
using CommandDeck.Identity.Contracts.CurrentUser;

namespace CommandDeck.Host.Features.CurrentUser;

public static class AuthenticatedIdentityAdapter
{
    private const string ProviderClaimType = "provider";
    private const string SubjectClaimType = "sub";
    private const string IssuerClaimType = "iss";
    private const string NameClaimType = "name";
    private const string EmailClaimType = "email";

    public static AuthenticatedIdentity? FromClaimsPrincipal(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        string? subject = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? principal.FindFirst(SubjectClaimType)?.Value;
        if (string.IsNullOrWhiteSpace(subject))
        {
            return null;
        }

        return new AuthenticatedIdentity(
            principal.FindFirst(ProviderClaimType)?.Value
                ?? principal.FindFirst(IssuerClaimType)?.Value
                ?? "oidc",
            subject,
            principal.FindFirst(ClaimTypes.Name)?.Value
                ?? principal.FindFirst(NameClaimType)?.Value,
            principal.FindFirst(ClaimTypes.Email)?.Value
                ?? principal.FindFirst(EmailClaimType)?.Value);
    }
}
