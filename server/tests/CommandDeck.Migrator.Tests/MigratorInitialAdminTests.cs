using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using CommandDeck.Identity.Access;
using CommandDeck.Identity.Users;
using CommandDeck.Persistence;
using Shouldly;
using Testcontainers.PostgreSql;

namespace CommandDeck.Migrator.Tests;

public sealed class MigratorInitialAdminTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("docker.io/library/postgres:17-alpine")
        .WithDatabase("commanddeck_migrator_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public Task InitializeAsync()
    {
        return _container.StartAsync();
    }

    public Task DisposeAsync()
    {
        return _container.DisposeAsync().AsTask();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Migrator_WhenInitialAdminIsNotConfigured_AppliesMigrationsWithoutGrantingAccess()
    {
        MigratorRunResult result = await RunMigratorAsync(
            [],
            new Dictionary<string, string?>());

        result.ExitCode.ShouldBe(0, result.Output);
        await using CommandDeckDbContext dbContext = CreateDbContext();
        (await dbContext.LocalUsers.CountAsync()).ShouldBe(0);
        (await dbContext.ApplicationAccess.CountAsync()).ShouldBe(0);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Migrator_AppliesMigrationsAndGrantsInitialAdminFromConfiguration()
    {
        MigratorRunResult result = await RunMigratorAsync(
            [],
            new Dictionary<string, string?>
            {
                ["Identity__InitialAdmin__Provider"] = "oidc",
                ["Identity__InitialAdmin__Subject"] = "subject-1"
            });

        result.ExitCode.ShouldBe(0, result.Output);
        await using CommandDeckDbContext dbContext = CreateDbContext();
        LocalUser user = await dbContext.LocalUsers.SingleAsync();
        ApplicationAccess access = await dbContext.ApplicationAccess.SingleAsync();
        user.Provider.ShouldBe("oidc");
        user.Subject.ShouldBe("subject-1");
        access.LocalUserId.ShouldBe(user.Id);
        access.IsActive.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Migrator_WhenAccessWasRevoked_FailsWithoutForceAndReactivatesWithCommandLineForce()
    {
        MigratorRunResult initial = await RunMigratorAsync(
            ["identity", "grant-admin", "--provider", "oidc", "--subject", "subject-2"],
            new Dictionary<string, string?>());
        initial.ExitCode.ShouldBe(0, initial.Output);
        await using (CommandDeckDbContext dbContext = CreateDbContext())
        {
            ApplicationAccess access = await dbContext.ApplicationAccess.SingleAsync();
            access.Revoke();
            await dbContext.SaveChangesAsync();
        }

        MigratorRunResult failure = await RunMigratorAsync(
            ["identity", "grant-admin", "--provider", "oidc", "--subject", "subject-2"],
            new Dictionary<string, string?>());
        failure.ExitCode.ShouldBe(2, failure.Output);
        await using (CommandDeckDbContext dbContext = CreateDbContext())
        {
            (await dbContext.ApplicationAccess.SingleAsync()).IsActive.ShouldBeFalse();
        }

        MigratorRunResult forced = await RunMigratorAsync(
            ["identity", "grant-admin", "--provider", "oidc", "--subject", "subject-2", "--force"],
            new Dictionary<string, string?>());
        forced.ExitCode.ShouldBe(0, forced.Output);
        await using (CommandDeckDbContext dbContext = CreateDbContext())
        {
            (await dbContext.ApplicationAccess.SingleAsync()).IsActive.ShouldBeTrue();
        }
    }

    private async Task<MigratorRunResult> RunMigratorAsync(
        IReadOnlyList<string> arguments,
        IReadOnlyDictionary<string, string?> environment)
    {
        string migratorAssemblyPath = FindRepositoryRoot()
            .Combine("server", "src", "CommandDeck.Migrator", "bin", "Debug", "net10.0", "CommandDeck.Migrator.dll");
        var startInfo = new ProcessStartInfo("dotnet")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        startInfo.ArgumentList.Add(migratorAssemblyPath);
        foreach (string argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        startInfo.Environment["ConnectionStrings__commanddeck-host"] = _container.GetConnectionString();
        startInfo.Environment["DOTNET_ENVIRONMENT"] = "Development";
        foreach (KeyValuePair<string, string?> variable in environment)
        {
            startInfo.Environment[variable.Key] = variable.Value;
        }

        using Process process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start migrator process.");
        Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
        Task<string> errorTask = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        string output = await outputTask;
        string error = await errorTask;

        return new MigratorRunResult(process.ExitCode, output + error);
    }

    private CommandDeckDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<CommandDeckDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;

        return new CommandDeckDbContext(options);
    }

    private static DirectoryInfo FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "CommandDeck.slnx")))
        {
            directory = directory.Parent;
        }

        return directory ?? throw new InvalidOperationException("Could not find repository root.");
    }

    private sealed record MigratorRunResult(int ExitCode, string Output);
}

internal static class PathExtensions
{
    public static string Combine(this DirectoryInfo directory, params string[] paths)
    {
        return Path.Combine([directory.FullName, .. paths]);
    }
}
