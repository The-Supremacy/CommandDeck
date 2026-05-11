using CommandDeck.Identity.Contracts.Authorization;
using CommandDeck.Identity.Contracts.CurrentUser;

namespace CommandDeck.Identity.Authorization;

public sealed class ApplicationAccessAuthorizer(ICurrentUserProvider currentUserProvider)
    : IApplicationAccessAuthorizer
{
    public async Task<bool> HasApplicationAccessAsync(
        AuthenticatedIdentity? identity,
        CancellationToken cancellationToken)
    {
        CurrentUserContext currentUser = await currentUserProvider.GetCurrentUserAsync(
            identity,
            cancellationToken);

        return currentUser.IsAuthenticated && currentUser.HasApplicationAccess;
    }
}
