using CommandDeck.SharedKernel.Domain;

namespace CommandDeck.Identity.Users;

public sealed class EmailAddress : SingleValueObject<string>
{
    private EmailAddress(string value)
        : base(value)
    {
    }

    public static EmailAddress Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidEmailAddressException("Email address is required.");
        }

        if (value != value.Trim())
        {
            throw new InvalidEmailAddressException("Email address must not contain surrounding whitespace.");
        }

        int atIndex = value.IndexOf('@', StringComparison.Ordinal);
        if (atIndex <= 0 || atIndex == value.Length - 1 || value.IndexOf('@', atIndex + 1) >= 0)
        {
            throw new InvalidEmailAddressException(
                "Email address must contain one local part and one domain part.");
        }

        return new EmailAddress(value);
    }

    public static EmailAddress? FromNullable(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : Create(value);
    }

}
