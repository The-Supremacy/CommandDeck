using Microsoft.EntityFrameworkCore;
using CommandDeck.Identity.Users;

namespace CommandDeck.Identity.Infrastructure.Persistence;

public sealed class LocalUserRepository(IIdentityDbContext dbContext) : ILocalUserRepository
{
    public Task<LocalUser?> GetByProviderSubjectAsync(
        string provider,
        string subject,
        CancellationToken cancellationToken)
    {
        return dbContext.LocalUsers
            .SingleOrDefaultAsync(
                x => x.Provider == provider && x.Subject == subject,
                cancellationToken);
    }

    public void Add(LocalUser user)
    {
        dbContext.LocalUsers.Add(user);
    }
}
