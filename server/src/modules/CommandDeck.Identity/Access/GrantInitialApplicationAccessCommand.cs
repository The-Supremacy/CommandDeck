using Mediator;
using CommandDeck.Identity.Users;

namespace CommandDeck.Identity.Access;

public sealed class InitialApplicationAccessOptions
{
    public string? Provider { get; init; }

    public string? Subject { get; init; }
}

public sealed record GrantInitialApplicationAccessCommand(
    string? Provider,
    string? Subject) : ICommand<bool>;

public sealed class GrantInitialApplicationAccessCommandHandler(
    ILocalUserRepository localUserRepository,
    IApplicationAccessRepository applicationAccessRepository)
    : ICommandHandler<GrantInitialApplicationAccessCommand, bool>
{
    public async ValueTask<bool> Handle(
        GrantInitialApplicationAccessCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Provider)
            || string.IsNullOrWhiteSpace(command.Subject))
        {
            return false;
        }

        LocalUser? user = await localUserRepository.GetByProviderSubjectAsync(
            command.Provider,
            command.Subject,
            cancellationToken);

        if (user is null)
        {
            user = LocalUser.Create(command.Provider, command.Subject, null, null);
            localUserRepository.Add(user);
        }

        ApplicationAccess? access = await applicationAccessRepository.GetByLocalUserIdAsync(
            user.Id,
            cancellationToken);

        if (access is null)
        {
            applicationAccessRepository.Add(ApplicationAccess.GrantTo(user.Id));
            return true;
        }

        access.Grant();
        return true;
    }
}
