using Microsoft.EntityFrameworkCore;
using CommandDeck.Identity.Access;
using CommandDeck.Identity.Infrastructure.Persistence;
using CommandDeck.Identity.Infrastructure.Tests.Support;
using CommandDeck.Identity.Users;
using CommandDeck.Persistence;
using Shouldly;

namespace CommandDeck.Identity.Infrastructure.Tests.Persistence;

public sealed class IdentityRepositoryTests(PostgreSqlFixture postgreSqlFixture)
    : IClassFixture<PostgreSqlFixture>
{
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetByProviderSubjectAsync_WhenProviderSubjectExists_ReusesExistingUser()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.EnsureDeletedAsync(CancellationToken.None);
        await dbContext.Database.EnsureCreatedAsync(CancellationToken.None);
        var users = new LocalUserRepository(dbContext);
        LocalUser first = LocalUser.Create("oidc", "subject-1", "Ada", "ada@example.test");
        users.Add(first);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        LocalUser? second = await users.GetByProviderSubjectAsync(
            "oidc",
            "subject-1",
            CancellationToken.None);

        second.ShouldNotBeNull();
        second.Id.ShouldBe(first.Id);
        (await dbContext.LocalUsers.CountAsync(CancellationToken.None)).ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task SaveChangesAsync_WhenAggregateHasDomainEvents_PersistsDomainEventRows()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.EnsureDeletedAsync(CancellationToken.None);
        await dbContext.Database.EnsureCreatedAsync(CancellationToken.None);
        var users = new LocalUserRepository(dbContext);
        LocalUser user = LocalUser.Create("oidc", "subject-1", "Ada", "ada@example.test");
        users.Add(user);

        await dbContext.SaveChangesAsync(CancellationToken.None);

        var storedEvent = await dbContext.DomainEvents.SingleAsync(CancellationToken.None);
        storedEvent.AggregateType.ShouldBe("identity.local-user");
        storedEvent.AggregateId.ShouldBe(user.Id.ToString());
        storedEvent.EventType.ShouldBe("identity.local-user-created");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task HandleAsync_WhenApplicationAccessIsActive_ReturnsTrue()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.EnsureDeletedAsync(CancellationToken.None);
        await dbContext.Database.EnsureCreatedAsync(CancellationToken.None);
        var users = new LocalUserRepository(dbContext);
        var accessRepository = new ApplicationAccessRepository(dbContext);
        LocalUser user = LocalUser.Create("oidc", "subject-1", "Ada", "ada@example.test");
        users.Add(user);
        accessRepository.Add(ApplicationAccess.GrantTo(user.Id));
        await dbContext.SaveChangesAsync(CancellationToken.None);

        bool hasAccess = await accessRepository.HasActiveAccessAsync(user.Id, CancellationToken.None);

        hasAccess.ShouldBeTrue();
    }

    private CommandDeckDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<CommandDeckDbContext>()
            .UseNpgsql(postgreSqlFixture.ConnectionString)
            .Options;

        return new CommandDeckDbContext(options);
    }
}
