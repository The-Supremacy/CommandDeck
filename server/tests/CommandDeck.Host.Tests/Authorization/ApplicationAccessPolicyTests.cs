using Mediator;
using System.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using CommandDeck.Host.Authorization;
using CommandDeck.Host.Tests.Authentication;
using CommandDeck.Identity;
using CommandDeck.Identity.Access;
using CommandDeck.Identity.Contracts.CurrentUser;
using CommandDeck.Identity.Users;
using CommandDeck.Host.Tests.Support;
using CommandDeck.Identity.CurrentUser;
using Shouldly;

namespace CommandDeck.Host.Tests.Authorization;

public sealed class ApplicationAccessPolicyTests
{
    [Fact]
    [Trait("Category", "Application")]
    public async Task Protected_api_returns_forbidden_for_authenticated_user_without_application_access()
    {
        using var host = await CreateHostAsync();
        using var client = host.GetTestClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/protected");
        request.Headers.Add(TestAuthenticationHandler.SubjectHeader, "subject-without-access");

        HttpResponseMessage response = await client.SendAsync(
            request,
            CancellationToken.None);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        response.Headers.Location.ShouldBeNull();
    }

    private static async Task<WebApplication> CreateHostAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthenticationHandler.Scheme;
                options.DefaultChallengeScheme = TestAuthenticationHandler.Scheme;
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                TestAuthenticationHandler.Scheme,
                _ => { });
        builder.Services.AddIdentityModule();
        builder.Services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.Assemblies = [typeof(ResolveCurrentUserCommand).Assembly];
        });
        builder.Services.RemoveAll<IPipelineBehavior<ResolveCurrentUserCommand, CurrentUserContext>>();
        builder.Services.RemoveAll<IPipelineBehavior<GrantInitialApplicationAccessCommand, bool>>();
        builder.Services.AddSingleton<HostTestIdentityContext>();
        builder.Services.AddSingleton<ILocalUserRepository>(services => services.GetRequiredService<HostTestIdentityContext>());
        builder.Services.AddSingleton<IApplicationAccessRepository>(services => services.GetRequiredService<HostTestIdentityContext>());
        builder.Services.AddApplicationAccessAuthorization();

        var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapGet("/api/protected", () => Results.Ok())
            .RequireAuthorization(ApplicationAccessPolicyConfiguration.PolicyName);
        await app.StartAsync(CancellationToken.None);

        return app;
    }
}
