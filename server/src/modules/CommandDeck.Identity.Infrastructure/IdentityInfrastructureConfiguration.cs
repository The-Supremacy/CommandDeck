using Microsoft.Extensions.DependencyInjection;
using CommandDeck.Identity.Infrastructure.Persistence;
using CommandDeck.Identity.Users;
using CommandDeck.Identity.Access;

namespace CommandDeck.Identity.Infrastructure;

public static class IdentityInfrastructureConfiguration
{
    public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<ILocalUserRepository, LocalUserRepository>();
        services.AddScoped<IApplicationAccessRepository, ApplicationAccessRepository>();

        return services;
    }
}
