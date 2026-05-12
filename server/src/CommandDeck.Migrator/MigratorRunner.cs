using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CommandDeck.Identity.Access;
using CommandDeck.Identity.Users;
using CommandDeck.Persistence;

namespace CommandDeck.Migrator;

internal sealed class MigratorRunner(
    CommandDeckDbContext dbContext,
    ILocalUserRepository localUserRepository,
    IApplicationAccessRepository applicationAccessRepository,
    IOptions<InitialAdminOptions> options,
    ILogger<MigratorRunner> logger)
{
    public async Task<int> RunAsync(string[] args, CancellationToken cancellationToken)
    {
        logger.LogInformation("Applying CommandDeckDbContext migrations.");
        await dbContext.Database.MigrateAsync(cancellationToken);
        logger.LogInformation("CommandDeckDbContext migrations applied.");

        InitialAdminSetupRequest? request = InitialAdminSetupRequest.Create(args, options.Value);
        if (request is null)
        {
            logger.LogInformation("Initial admin setup skipped because provider or subject is not configured.");
            return 0;
        }

        logger.LogInformation(
            "Running initial admin setup for provider {Provider} and subject {Subject}.",
            request.Provider,
            request.Subject);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var handler = new GrantInitialAdminAccessCommandHandler(localUserRepository, applicationAccessRepository);
        GrantInitialAdminAccessResult result = await handler.Handle(
            new GrantInitialAdminAccessCommand(request.Provider, request.Subject, request.Force),
            cancellationToken);

        if (result == GrantInitialAdminAccessResult.RevokedAccessRequiresForce)
        {
            logger.LogError(
                "Initial admin setup found revoked application access. Re-run with --force or Identity:InitialAdmin:Force=true to reactivate it.");
            return 2;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        logger.LogInformation("Initial admin setup completed with result {Result}.", result);
        return 0;
    }
}

internal sealed record InitialAdminSetupRequest(string Provider, string Subject, bool Force)
{
    public static InitialAdminSetupRequest? Create(string[] args, InitialAdminOptions options)
    {
        if (args.Length > 0)
        {
            return CreateFromCommandLine(args);
        }

        return string.IsNullOrWhiteSpace(options.Provider) || string.IsNullOrWhiteSpace(options.Subject)
            ? null
            : new InitialAdminSetupRequest(options.Provider, options.Subject, options.Force);
    }

    private static InitialAdminSetupRequest CreateFromCommandLine(string[] args)
    {
        if (args.Length < 2
            || !string.Equals(args[0], "identity", StringComparison.OrdinalIgnoreCase)
            || !string.Equals(args[1], "grant-admin", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Unknown migrator command. Use: identity grant-admin --provider <issuer> --subject <subject> [--force].");
        }

        string? provider = null;
        string? subject = null;
        bool force = false;

        for (int index = 2; index < args.Length; index++)
        {
            string arg = args[index];
            if (string.Equals(arg, "--force", StringComparison.OrdinalIgnoreCase))
            {
                force = true;
                continue;
            }

            if (string.Equals(arg, "--provider", StringComparison.OrdinalIgnoreCase)
                && index + 1 < args.Length)
            {
                provider = args[++index];
                continue;
            }

            if (string.Equals(arg, "--subject", StringComparison.OrdinalIgnoreCase)
                && index + 1 < args.Length)
            {
                subject = args[++index];
                continue;
            }

            throw new InvalidOperationException(
                "Unknown migrator command option. Use: identity grant-admin --provider <issuer> --subject <subject> [--force].");
        }

        if (string.IsNullOrWhiteSpace(provider) || string.IsNullOrWhiteSpace(subject))
        {
            throw new InvalidOperationException(
                "Initial admin setup requires --provider <issuer> and --subject <subject>.");
        }

        return new InitialAdminSetupRequest(provider, subject, force);
    }
}
