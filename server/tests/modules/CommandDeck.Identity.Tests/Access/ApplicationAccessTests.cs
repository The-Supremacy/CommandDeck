using CommandDeck.Identity.Access;
using CommandDeck.Identity.Access.Events;
using Shouldly;

namespace CommandDeck.Identity.Tests.Access;

public sealed class ApplicationAccessTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_WhenAccessIsGranted_RecordsGrantedEvent()
    {
        var access = ApplicationAccess.GrantTo(Guid.NewGuid());

        access.IsActive.ShouldBeTrue();
        access.DomainEvents.Single().ShouldBeOfType<ApplicationAccessGrantedDomainEvent>();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Revoke_WhenAccessIsActive_RecordsRevokedEvent()
    {
        var access = ApplicationAccess.GrantTo(Guid.NewGuid());
        access.ClearDomainEvents();

        access.Revoke();

        access.IsActive.ShouldBeFalse();
        access.DisabledAt.ShouldNotBeNull();
        access.DomainEvents.Single().ShouldBeOfType<ApplicationAccessRevokedDomainEvent>();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Grant_WhenAccessIsRevoked_RecordsGrantedEvent()
    {
        var access = ApplicationAccess.GrantTo(Guid.NewGuid());
        access.Revoke();
        access.ClearDomainEvents();

        access.Grant();

        access.IsActive.ShouldBeTrue();
        access.DisabledAt.ShouldBeNull();
        access.DomainEvents.Single().ShouldBeOfType<ApplicationAccessGrantedDomainEvent>();
    }
}
