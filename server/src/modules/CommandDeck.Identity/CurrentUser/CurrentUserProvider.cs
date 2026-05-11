using Mediator;
using CommandDeck.Identity.Contracts.CurrentUser;

namespace CommandDeck.Identity.CurrentUser;

public sealed class CurrentUserProvider(IMediator mediator) : ICurrentUserProvider
{
    public async Task<CurrentUserContext> GetCurrentUserAsync(
        AuthenticatedIdentity? identity,
        CancellationToken cancellationToken)
    {
        return await mediator.Send(
            new ResolveCurrentUserCommand(identity),
            cancellationToken);
    }
}
