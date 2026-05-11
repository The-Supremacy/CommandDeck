using Mediator;
using CommandDeck.Identity.Access;
using CommandDeck.Identity.Contracts.CurrentUser;
using CommandDeck.Identity.Users;

namespace CommandDeck.Identity.CurrentUser;

public sealed record ResolveCurrentUserCommand(
    AuthenticatedIdentity? Identity) : ICommand<CurrentUserContext>;

public sealed class ResolveCurrentUserCommandHandler(
    ILocalUserRepository localUserRepository,
    IApplicationAccessRepository applicationAccessRepository)
    : ICommandHandler<ResolveCurrentUserCommand, CurrentUserContext>
{
    public async ValueTask<CurrentUserContext> Handle(
        ResolveCurrentUserCommand command,
        CancellationToken cancellationToken)
    {
        AuthenticatedIdentity? identity = command.Identity;
        if (identity is null || string.IsNullOrWhiteSpace(identity.Subject))
        {
            return CurrentUserContext.Unauthenticated;
        }

        LocalUser? localUser = await localUserRepository.GetByProviderSubjectAsync(
            identity.Provider,
            identity.Subject,
            cancellationToken);

        if (localUser is null)
        {
            localUser = LocalUser.Create(
                identity.Provider,
                identity.Subject,
                identity.DisplayName,
                identity.Email);
            localUserRepository.Add(localUser);
        }
        else
        {
            localUser.MarkSeen(identity.DisplayName, identity.Email);
        }

        bool hasAccess = await applicationAccessRepository.HasActiveAccessAsync(localUser.Id, cancellationToken);

        return new CurrentUserContext(
            true,
            localUser.Id,
            localUser.DisplayName,
            localUser.Email?.Value,
            hasAccess);
    }
}
