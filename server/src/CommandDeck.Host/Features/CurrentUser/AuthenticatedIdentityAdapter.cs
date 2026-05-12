using System.Security.Claims;
using CommandDeck.Host.Authentication;
using CommandDeck.Identity.Contracts.CurrentUser;

namespace CommandDeck.Host.Features.CurrentUser;

public static class AuthenticatedIdentityAdapter
{
    public static AuthenticatedIdentity? FromClaimsPrincipal(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        string? subject = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? principal.FindFirst(AppSessionClaimTypes.Subject)?.Value;
        if (string.IsNullOrWhiteSpace(subject))
        {
            return null;
        }

        string? provider = principal.FindFirst(AppSessionClaimTypes.Provider)?.Value
            ?? principal.FindFirst(AppSessionClaimTypes.Issuer)?.Value;
        if (string.IsNullOrWhiteSpace(provider))
        {
            return null;
        }

        return new AuthenticatedIdentity(
            provider,
            subject,
            principal.FindFirst(ClaimTypes.Name)?.Value
                ?? principal.FindFirst(AppSessionClaimTypes.Name)?.Value,
            principal.FindFirst(ClaimTypes.Email)?.Value
                ?? principal.FindFirst(AppSessionClaimTypes.Email)?.Value);
    }
}
