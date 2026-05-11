using CommandDeck.Identity.Contracts.CurrentUser;

namespace CommandDeck.Identity.Contracts.Authorization;

public interface IApplicationAccessAuthorizer
{
    Task<bool> HasApplicationAccessAsync(
        AuthenticatedIdentity? identity,
        CancellationToken cancellationToken);
}
