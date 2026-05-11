using CommandDeck.Identity.Access;
using CommandDeck.Identity.Contracts.CurrentUser;
using CommandDeck.Identity.CurrentUser;
using CommandDeck.Identity.Tests.Support;
using Shouldly;

namespace CommandDeck.Identity.Tests.CurrentUser;

public sealed class CurrentUserProviderTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetCurrentUserAsync_WhenIdentityIsNew_CreatesLocalUserWithNoDefaultAccess()
    {
        var identityContext = new InMemoryIdentityContext();
        var handler = new ResolveCurrentUserCommandHandler(identityContext, identityContext);

        CurrentUserContext currentUser = await handler.Handle(
            new ResolveCurrentUserCommand(new AuthenticatedIdentity("oidc", "subject-1", "Ada", "ada@example.test")),
            CancellationToken.None);

        currentUser.IsAuthenticated.ShouldBeTrue();
        currentUser.LocalUserId.ShouldNotBeNull();
        currentUser.DisplayName.ShouldBe("Ada");
        currentUser.Email.ShouldBe("ada@example.test");
        currentUser.HasApplicationAccess.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetCurrentUserAsync_WhenApplicationAccessIsActive_ReturnsAccessState()
    {
        var identityContext = new InMemoryIdentityContext();
        var handler = new ResolveCurrentUserCommandHandler(identityContext, identityContext);
        var identity = new AuthenticatedIdentity("oidc", "subject-1", "Ada", "ada@example.test");
        CurrentUserContext created = await handler.Handle(
            new ResolveCurrentUserCommand(identity),
            CancellationToken.None);

        identityContext.Add(ApplicationAccess.GrantTo(created.LocalUserId!.Value));

        CurrentUserContext currentUser = await handler.Handle(
            new ResolveCurrentUserCommand(identity),
            CancellationToken.None);

        currentUser.HasApplicationAccess.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetCurrentUserAsync_WhenIdentityIsMissing_ReturnsUnauthenticated()
    {
        var identityContext = new InMemoryIdentityContext();
        var handler = new ResolveCurrentUserCommandHandler(identityContext, identityContext);

        CurrentUserContext currentUser = await handler.Handle(
            new ResolveCurrentUserCommand(null),
            CancellationToken.None);

        currentUser.ShouldBe(CurrentUserContext.Unauthenticated);
    }
}
