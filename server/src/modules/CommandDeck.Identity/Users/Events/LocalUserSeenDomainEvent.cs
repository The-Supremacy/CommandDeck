using CommandDeck.SharedKernel.Domain;

namespace CommandDeck.Identity.Users.Events;

[DomainEventType(
    "identity.local-user-seen",
    "identity.local-user",
    1)]
public sealed record LocalUserSeenDomainEvent(
    Guid LocalUserId,
    string Provider,
    string Subject) : DomainEvent;
