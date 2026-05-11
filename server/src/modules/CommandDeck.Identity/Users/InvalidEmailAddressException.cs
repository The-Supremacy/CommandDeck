using CommandDeck.SharedKernel.Domain;

namespace CommandDeck.Identity.Users;

public sealed class InvalidEmailAddressException : DomainException
{
    public InvalidEmailAddressException(string message)
        : base(message)
    {
    }
}
