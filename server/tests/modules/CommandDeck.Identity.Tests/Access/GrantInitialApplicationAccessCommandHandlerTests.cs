using CommandDeck.Identity.Access;
using CommandDeck.Identity.Tests.Support;
using Shouldly;

namespace CommandDeck.Identity.Tests.Access;

public sealed class GrantInitialApplicationAccessCommandHandlerTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WhenConfigurationIsComplete_CreatesActiveApplicationAccess()
    {
        var identity = new InMemoryIdentityContext();
        var handler = new GrantInitialApplicationAccessCommandHandler(identity, identity);

        await handler.Handle(
            new GrantInitialApplicationAccessCommand("oidc", "subject-1"),
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
        var handler = new GrantInitialApplicationAccessCommandHandler(identity, identity);

        await handler.Handle(
            new GrantInitialApplicationAccessCommand("oidc", "subject-1"),
            CancellationToken.None);
        await handler.Handle(
            new GrantInitialApplicationAccessCommand("oidc", "subject-1"),
            CancellationToken.None);

        identity.ApplicationAccess.Count.ShouldBe(1);
    }
}
