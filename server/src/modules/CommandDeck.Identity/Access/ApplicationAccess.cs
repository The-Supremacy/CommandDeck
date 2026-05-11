using CommandDeck.Identity.Access.Events;
using CommandDeck.SharedKernel.Domain;

namespace CommandDeck.Identity.Access;

public sealed class ApplicationAccess : AggregateRoot<Guid>
{
    private ApplicationAccess(Guid id, Guid localUserId)
        : base(id)
    {
        LocalUserId = localUserId;
        IsActive = true;
        CreatedAt = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new ApplicationAccessGrantedDomainEvent(id, localUserId));
    }

    private ApplicationAccess()
    {
    }

    public Guid LocalUserId { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? DisabledAt { get; private set; }

    public static ApplicationAccess GrantTo(Guid localUserId)
    {
        if (localUserId == Guid.Empty)
        {
            throw new ArgumentException("Local user id must not be empty.", nameof(localUserId));
        }

        return new ApplicationAccess(Guid.NewGuid(), localUserId);
    }

    public void Grant()
    {
        if (IsActive)
        {
            return;
        }

        IsActive = true;
        DisabledAt = null;
        RaiseDomainEvent(new ApplicationAccessGrantedDomainEvent(Id, LocalUserId));
    }

    public void Revoke()
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        DisabledAt = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new ApplicationAccessRevokedDomainEvent(Id, LocalUserId));
    }
}
