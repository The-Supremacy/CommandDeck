using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using CommandDeck.Identity.Access;

namespace CommandDeck.Host.HostedServices;

public sealed class InitialApplicationAccessHostedService(
    IServiceScopeFactory scopeFactory,
    IOptions<InitialApplicationAccessOptions> options) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        InitialApplicationAccessOptions value = options.Value;
        if (string.IsNullOrWhiteSpace(value.Provider)
            || string.IsNullOrWhiteSpace(value.Subject))
        {
            return;
        }

        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.Send(
            new GrantInitialApplicationAccessCommand(value.Provider, value.Subject),
            cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
