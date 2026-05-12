using System.Security.Claims;
using CommandDeck.Host.Authentication;
using CommandDeck.Host.Features.CurrentUser;
using Shouldly;

namespace CommandDeck.Host.Tests.CurrentUser;

public sealed class AuthenticatedIdentityAdapterTests
{
    [Fact]
    [Trait("Category", "Application")]
    public void FromClaimsPrincipal_uses_provider_and_subject_as_identity_key()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(AppSessionClaimTypes.Provider, "https://id.example.test/realms/commanddeck"),
                new Claim(ClaimTypes.NameIdentifier, "subject-1"),
            ],
            "Test"));

        var identity = AuthenticatedIdentityAdapter.FromClaimsPrincipal(principal);

        identity.ShouldNotBeNull();
        identity.Provider.ShouldBe("https://id.example.test/realms/commanddeck");
        identity.Subject.ShouldBe("subject-1");
    }

    [Fact]
    [Trait("Category", "Application")]
    public void FromClaimsPrincipal_returns_null_when_provider_is_missing()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "subject-1")],
            "Test"));

        AuthenticatedIdentityAdapter.FromClaimsPrincipal(principal).ShouldBeNull();
    }
}
