using Microsoft.EntityFrameworkCore;
using CommandDeck.Identity.Access;

namespace CommandDeck.Identity.Infrastructure.Persistence;

public sealed class ApplicationAccessRepository(IIdentityDbContext dbContext)
    : IApplicationAccessRepository
{
    public Task<ApplicationAccess?> GetByLocalUserIdAsync(
        Guid localUserId,
        CancellationToken cancellationToken)
    {
        return dbContext.ApplicationAccess
            .SingleOrDefaultAsync(x => x.LocalUserId == localUserId, cancellationToken);
    }

    public Task<bool> HasActiveAccessAsync(
        Guid localUserId,
        CancellationToken cancellationToken)
    {
        return dbContext.ApplicationAccess.AnyAsync(
            x => x.LocalUserId == localUserId && x.IsActive,
            cancellationToken);
    }

    public void Add(ApplicationAccess access)
    {
        dbContext.ApplicationAccess.Add(access);
    }
}
