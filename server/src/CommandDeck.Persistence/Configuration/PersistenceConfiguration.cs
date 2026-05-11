using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CommandDeck.Identity.Infrastructure.Persistence;
using CommandDeck.Persistence;

namespace CommandDeck.Persistence.Configuration;

public static class PersistenceConfiguration
{
    private const string ConnectionStringName = "commanddeck-host";

    public static TBuilder AddPersistence<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        string connectionString = builder.Configuration[$"ConnectionStrings:{ConnectionStringName}"]
            ?? throw new InvalidOperationException(
                $"Connection string 'ConnectionStrings:{ConnectionStringName}' is required.");

        builder.Services.AddDbContext<CommandDeckDbContext>(options =>
        {
            options.UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "host"));
        });
        builder.Services.AddScoped<IIdentityDbContext>(
            services => services.GetRequiredService<CommandDeckDbContext>());

        return builder;
    }
}
