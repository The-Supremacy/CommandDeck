using CommandDeck.Identity.Access;
using CommandDeck.Identity.Users;

namespace CommandDeck.Identity.Tests.Support;

internal sealed class InMemoryIdentityContext :
    ILocalUserRepository,
    IApplicationAccessRepository
{
    private readonly List<ApplicationAccess> _applicationAccess = [];
    private readonly List<LocalUser> _users = [];

    public IReadOnlyCollection<ApplicationAccess> ApplicationAccess => _applicationAccess.AsReadOnly();

    public IReadOnlyCollection<LocalUser> Users => _users.AsReadOnly();

    public Task<LocalUser?> GetByProviderSubjectAsync(
        string provider,
        string subject,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(_users.SingleOrDefault(x => x.Provider == provider && x.Subject == subject));
    }

    public void Add(LocalUser user)
    {
        _users.Add(user);
    }

    public Task<ApplicationAccess?> GetByLocalUserIdAsync(
        Guid localUserId,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(_applicationAccess.SingleOrDefault(x => x.LocalUserId == localUserId));
    }

    public Task<bool> HasActiveAccessAsync(
        Guid localUserId,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(_applicationAccess.Any(x => x.LocalUserId == localUserId && x.IsActive));
    }

    public void Add(ApplicationAccess access)
    {
        _applicationAccess.Add(access);
    }
}
