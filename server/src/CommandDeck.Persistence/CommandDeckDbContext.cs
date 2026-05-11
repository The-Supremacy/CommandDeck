using Microsoft.EntityFrameworkCore;
using CommandDeck.Identity.Access;
using CommandDeck.Identity.Infrastructure.Persistence;
using CommandDeck.Identity.Users;
using CommandDeck.Persistence.DomainEvents;
using CommandDeck.SharedKernel.Domain;

namespace CommandDeck.Persistence;

public sealed class CommandDeckDbContext(DbContextOptions<CommandDeckDbContext> options)
    : DbContext(options), IIdentityDbContext
{
    public DbSet<LocalUser> LocalUsers => Set<LocalUser>();

    public DbSet<ApplicationAccess> ApplicationAccess => Set<ApplicationAccess>();

    public DbSet<StoredDomainEvent> DomainEvents => Set<StoredDomainEvent>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        CaptureDomainEvents();

        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IIdentityDbContext).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StoredDomainEvent).Assembly);
    }

    private void CaptureDomainEvents()
    {
        var domainEventEntries = ChangeTracker.Entries<IAggregateRoot>()
            .Where(x => x.Entity.DomainEvents.Count > 0)
            .Select(x => new
            {
                Aggregate = x.Entity,
                Events = x.Entity.DequeueDomainEvents()
            })
            .ToArray();

        foreach (var entry in domainEventEntries)
        {
            foreach (IDomainEvent domainEvent in entry.Events)
            {
                DomainEvents.Add(StoredDomainEvent.FromDomainEvent(
                    domainEvent,
                    entry.Aggregate.Id.ToString() ?? string.Empty));
            }
        }
    }
}
