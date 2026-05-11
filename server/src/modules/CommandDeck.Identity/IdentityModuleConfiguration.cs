using Microsoft.Extensions.DependencyInjection;
using CommandDeck.Identity.Authorization;
using CommandDeck.Identity.CurrentUser;
using CommandDeck.Identity.Contracts.Authorization;
using CommandDeck.Identity.Contracts.CurrentUser;

namespace CommandDeck.Identity;

public static class IdentityModuleConfiguration
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services)
    {
        services.AddScoped<ICurrentUserProvider, CurrentUserProvider>();
        services.AddScoped<IApplicationAccessAuthorizer, ApplicationAccessAuthorizer>();

        return services;
    }
}
