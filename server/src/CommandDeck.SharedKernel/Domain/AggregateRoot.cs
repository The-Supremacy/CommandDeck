namespace CommandDeck.SharedKernel.Domain;

public interface IAggregateRoot
{
    // Infrastructure uses this non-generic surface to collect domain events from
    // tracked aggregate roots without knowing each aggregate's identifier type.
    object Id { get; }

    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    IReadOnlyCollection<IDomainEvent> DequeueDomainEvents();
}

public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    protected AggregateRoot()
    {
    }

    protected AggregateRoot(TId id)
        : base(id)
    {
    }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    object IAggregateRoot.Id => Id;

    public IReadOnlyCollection<IDomainEvent> DequeueDomainEvents()
    {
        IDomainEvent[] domainEvents = [.. _domainEvents];
        _domainEvents.Clear();

        return domainEvents;
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        _domainEvents.Add(domainEvent);
    }
}
