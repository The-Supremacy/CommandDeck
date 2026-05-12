using Mediator;
using CommandDeck.Identity.Users;

namespace CommandDeck.Identity.Access;

public sealed class InitialAdminOptions
{
    public string? Provider { get; init; }

    public string? Subject { get; init; }

    public bool Force { get; init; }
}

public enum GrantInitialAdminAccessResult
{
    Skipped,
    Granted,
    AlreadyActive,
    Reactivated,
    RevokedAccessRequiresForce
}

public sealed record GrantInitialAdminAccessCommand(
    string? Provider,
    string? Subject,
    bool Force = false) : ICommand<GrantInitialAdminAccessResult>;

public sealed class GrantInitialAdminAccessCommandHandler(
    ILocalUserRepository localUserRepository,
    IApplicationAccessRepository applicationAccessRepository)
    : ICommandHandler<GrantInitialAdminAccessCommand, GrantInitialAdminAccessResult>
{
    public async ValueTask<GrantInitialAdminAccessResult> Handle(
        GrantInitialAdminAccessCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Provider)
            || string.IsNullOrWhiteSpace(command.Subject))
        {
            return GrantInitialAdminAccessResult.Skipped;
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
            return GrantInitialAdminAccessResult.Granted;
        }

        if (access.IsActive)
        {
            return GrantInitialAdminAccessResult.AlreadyActive;
        }

        if (!command.Force)
        {
            return GrantInitialAdminAccessResult.RevokedAccessRequiresForce;
        }

        access.Grant();
        return GrantInitialAdminAccessResult.Reactivated;
    }
}
