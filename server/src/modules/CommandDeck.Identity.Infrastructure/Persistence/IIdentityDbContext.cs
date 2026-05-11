using Microsoft.EntityFrameworkCore;
using CommandDeck.Identity.Access;
using CommandDeck.Identity.Users;

namespace CommandDeck.Identity.Infrastructure.Persistence;

public interface IIdentityDbContext
{
    DbSet<LocalUser> LocalUsers { get; }

    DbSet<ApplicationAccess> ApplicationAccess { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
