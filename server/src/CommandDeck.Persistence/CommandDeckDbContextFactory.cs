using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CommandDeck.Persistence;

public sealed class CommandDeckDbContextFactory : IDesignTimeDbContextFactory<CommandDeckDbContext>
{
    public CommandDeckDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CommandDeckDbContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=commanddeck;Username=postgres;Password=postgres",
            npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "host"));

        return new CommandDeckDbContext(optionsBuilder.Options);
    }
}
