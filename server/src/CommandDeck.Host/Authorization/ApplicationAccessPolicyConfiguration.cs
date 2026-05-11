using Microsoft.AspNetCore.Authorization;
using CommandDeck.Host.Features.CurrentUser;
using CommandDeck.Identity.Contracts.Authorization;
using CommandDeck.Identity.Contracts.CurrentUser;

namespace CommandDeck.Host.Authorization;

public static class ApplicationAccessPolicyConfiguration
{
    public const string PolicyName = "ApplicationAccess";

    public static IServiceCollection AddApplicationAccessAuthorization(
        this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy(
                PolicyName,
                policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.AddRequirements(new ApplicationAccessRequirement());
                });
        services.AddScoped<IAuthorizationHandler, ApplicationAccessAuthorizationHandler>();

        return services;
    }
}

public sealed class ApplicationAccessRequirement : IAuthorizationRequirement;

public sealed class ApplicationAccessAuthorizationHandler(
    IApplicationAccessAuthorizer authorizer)
    : AuthorizationHandler<ApplicationAccessRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ApplicationAccessRequirement requirement)
    {
        AuthenticatedIdentity? identity =
            AuthenticatedIdentityAdapter.FromClaimsPrincipal(context.User);
        bool hasAccess = await authorizer.HasApplicationAccessAsync(
            identity,
            CancellationToken.None);

        if (hasAccess)
        {
            context.Succeed(requirement);
        }
    }
}
