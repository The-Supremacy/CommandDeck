namespace CommandDeck.Identity.Contracts.CurrentUser;

public sealed record CurrentUserContext(
    bool IsAuthenticated,
    Guid? LocalUserId,
    string? DisplayName,
    string? Email,
    bool HasApplicationAccess)
{
    public static CurrentUserContext Unauthenticated { get; } =
        new(false, null, null, null, false);
}
