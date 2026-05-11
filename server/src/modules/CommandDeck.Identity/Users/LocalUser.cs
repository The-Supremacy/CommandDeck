using CommandDeck.Identity.Users.Events;
using CommandDeck.SharedKernel.Domain;

namespace CommandDeck.Identity.Users;

public sealed class LocalUser : AggregateRoot<Guid>
{
    private LocalUser(
        Guid id,
        string provider,
        string subject,
        string? displayName,
        EmailAddress? email,
        DateTimeOffset createdAt)
        : base(id)
    {
        Provider = provider;
        Subject = subject;
        DisplayName = displayName;
        Email = email;
        CreatedAt = createdAt;
        LastSeenAt = createdAt;

        RaiseDomainEvent(new LocalUserCreatedDomainEvent(id, provider, subject));
    }

    private LocalUser()
    {
    }

    public string Provider { get; private set; } = string.Empty;

    public string Subject { get; private set; } = string.Empty;

    public string? DisplayName { get; private set; }

    public EmailAddress? Email { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset LastSeenAt { get; private set; }

    public static LocalUser Create(
        string provider,
        string subject,
        string? displayName,
        string? email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(provider);
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);

        return new LocalUser(
            Guid.NewGuid(),
            provider,
            subject,
            displayName,
            EmailAddress.FromNullable(email),
            DateTimeOffset.UtcNow);
    }

    public void MarkSeen(string? displayName, string? email)
    {
        DisplayName = displayName;
        Email = EmailAddress.FromNullable(email);
        LastSeenAt = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new LocalUserSeenDomainEvent(Id, Provider, Subject));
    }
}
