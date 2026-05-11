namespace CommandDeck.SharedKernel.Domain;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class DomainEventTypeAttribute(
    string name,
    string aggregateType,
    int version) : Attribute
{
    public string Name { get; } = name;

    public string AggregateType { get; } = aggregateType;

    public int Version { get; } = version;
}

public interface IDomainEvent
{
    Guid EventId { get; }

    DateTimeOffset OccurredAt { get; }
}

public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();

    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}
