using NSubstitute;
using CommandDeck.Identity.Access;
using CommandDeck.Identity.Authorization;
using CommandDeck.Identity.Contracts.CurrentUser;
using CommandDeck.Identity.Tests.Support;
using Shouldly;

namespace CommandDeck.Identity.Tests.Authorization;

public sealed class ApplicationAccessAuthorizerTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public async Task HasApplicationAccessAsync_WhenAccessIsActive_ReturnsTrue()
    {
        var provider = Substitute.For<ICurrentUserProvider>();
        var identity = new AuthenticatedIdentity("oidc", "subject-1", null, null);
        provider.GetCurrentUserAsync(identity, CancellationToken.None)
            .Returns(new CurrentUserContext(true, Guid.NewGuid(), null, null, true));
        var authorizer = new ApplicationAccessAuthorizer(provider);

        bool hasAccess = await authorizer.HasApplicationAccessAsync(
            identity,
            CancellationToken.None);

        hasAccess.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HasApplicationAccessAsync_WhenAccessIsMissing_ReturnsFalse()
    {
        var provider = Substitute.For<ICurrentUserProvider>();
        var identity = new AuthenticatedIdentity("oidc", "subject-1", null, null);
        provider.GetCurrentUserAsync(identity, CancellationToken.None)
            .Returns(new CurrentUserContext(true, Guid.NewGuid(), null, null, false));
        var authorizer = new ApplicationAccessAuthorizer(provider);

        bool hasAccess = await authorizer.HasApplicationAccessAsync(
            identity,
            CancellationToken.None);

        hasAccess.ShouldBeFalse();
    }
}
