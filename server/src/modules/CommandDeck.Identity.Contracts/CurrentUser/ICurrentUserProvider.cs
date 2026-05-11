namespace CommandDeck.Identity.Contracts.CurrentUser;

public interface ICurrentUserProvider
{
    Task<CurrentUserContext> GetCurrentUserAsync(
        AuthenticatedIdentity? identity,
        CancellationToken cancellationToken);
}
