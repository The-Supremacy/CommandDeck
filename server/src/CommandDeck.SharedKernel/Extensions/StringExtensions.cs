namespace CommandDeck.SharedKernel.Extensions;

public static class StringExtensions
{
    public static string ToSafeLocalReturnUrl(this string? value, string fallback = "/")
    {
        if (string.IsNullOrWhiteSpace(value)
            || !Uri.IsWellFormedUriString(value, UriKind.Relative)
            || value.StartsWith("//", StringComparison.Ordinal))
        {
            return fallback;
        }

        return value;
    }
}
