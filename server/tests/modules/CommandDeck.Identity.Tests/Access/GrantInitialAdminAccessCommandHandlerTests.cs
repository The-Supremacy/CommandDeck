using CommandDeck.Identity.Access;
using CommandDeck.Identity.Tests.Support;
using Shouldly;

namespace CommandDeck.Identity.Tests.Access;

public sealed class GrantInitialAdminAccessCommandHandlerTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WhenConfigurationIsComplete_CreatesActiveApplicationAccess()
    {
        var identity = new InMemoryIdentityContext();
        var handler = new GrantInitialAdminAccessCommandHandler(identity, identity);

        await handler.Handle(
            new GrantInitialAdminAccessCommand("oidc", "subject-1"),
            CancellationToken.None);
        var user = await identity.GetByProviderSubjectAsync(
            "oidc",
            "subject-1",
            CancellationToken.None);

        bool hasAccess = await identity.HasActiveAccessAsync(user!.Id, CancellationToken.None);

        hasAccess.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WhenRunRepeatedly_IsIdempotent()
    {
        var identity = new InMemoryIdentityContext();
        var handler = new GrantInitialAdminAccessCommandHandler(identity, identity);

        await handler.Handle(
            new GrantInitialAdminAccessCommand("oidc", "subject-1"),
            CancellationToken.None);
        await handler.Handle(
            new GrantInitialAdminAccessCommand("oidc", "subject-1"),
            CancellationToken.None);
        GrantInitialAdminAccessResult result = await handler.Handle(
            new GrantInitialAdminAccessCommand("oidc", "subject-1"),
            CancellationToken.None);

        identity.ApplicationAccess.Count.ShouldBe(1);
        result.ShouldBe(GrantInitialAdminAccessResult.AlreadyActive);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WhenAccessWasRevokedWithoutForce_DoesNotReactivateAccess()
    {
        var identity = new InMemoryIdentityContext();
        var handler = new GrantInitialAdminAccessCommandHandler(identity, identity);
        await handler.Handle(
            new GrantInitialAdminAccessCommand("oidc", "subject-1"),
            CancellationToken.None);
        ApplicationAccess access = identity.ApplicationAccess.Single();
        access.Revoke();

        GrantInitialAdminAccessResult result = await handler.Handle(
            new GrantInitialAdminAccessCommand("oidc", "subject-1"),
            CancellationToken.None);

        result.ShouldBe(GrantInitialAdminAccessResult.RevokedAccessRequiresForce);
        access.IsActive.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WhenAccessWasRevokedWithForce_ReactivatesAccess()
    {
        var identity = new InMemoryIdentityContext();
        var handler = new GrantInitialAdminAccessCommandHandler(identity, identity);
        await handler.Handle(
            new GrantInitialAdminAccessCommand("oidc", "subject-1"),
            CancellationToken.None);
        ApplicationAccess access = identity.ApplicationAccess.Single();
        access.Revoke();

        GrantInitialAdminAccessResult result = await handler.Handle(
            new GrantInitialAdminAccessCommand("oidc", "subject-1", Force: true),
            CancellationToken.None);

        result.ShouldBe(GrantInitialAdminAccessResult.Reactivated);
        access.IsActive.ShouldBeTrue();
    }
}
