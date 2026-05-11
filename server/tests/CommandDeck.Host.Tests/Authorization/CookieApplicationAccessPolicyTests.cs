using Mediator;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using CommandDeck.Host.Authorization;
using CommandDeck.Host.Configuration;
using CommandDeck.Host.Tests.Support;
using CommandDeck.Identity;
using CommandDeck.Identity.Access;
using CommandDeck.Identity.Contracts.CurrentUser;
using CommandDeck.Identity.CurrentUser;
using CommandDeck.Identity.Users;
using Shouldly;

namespace CommandDeck.Host.Tests.Authorization;

public sealed class CookieApplicationAccessPolicyTests
{
    [Fact]
    [Trait("Category", "Application")]
    public async Task Api_request_with_cookie_session_without_application_access_returns_forbidden_without_redirect()
    {
        using var app = await CreateApiHostAsync();
        using var client = app.GetTestClient();

        HttpResponseMessage signInResponse = await client.GetAsync(
            "/test/sign-in-without-access",
            CancellationToken.None);
        string sessionCookie = signInResponse.Headers.GetValues("Set-Cookie")
            .Single(x => x.StartsWith("CommandDeck.Session=", StringComparison.Ordinal));

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/protected");
        request.Headers.Add("Cookie", sessionCookie);

        HttpResponseMessage response = await client.SendAsync(
            request,
            CancellationToken.None);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        response.Headers.Location.ShouldBeNull();
    }

    private static async Task<WebApplication> CreateApiHostAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Configuration["Authentication:Oidc:Authority"] =
            "http://localhost:8080/realms/commanddeck";
        builder.AddHostAuthentication();
        builder.Services.RemoveAll<IDistributedCache>();
        builder.Services.AddDistributedMemoryCache();
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
        app.MapGet(
            "/test/sign-in-without-access",
            async context =>
            {
                var identity = new ClaimsIdentity(
                    [
                        new Claim("provider", "test"),
                        new Claim(ClaimTypes.NameIdentifier, "subject-without-access"),
                    ],
                    HostAuthenticationConfiguration.CookieScheme);
                await context.SignInAsync(
                    HostAuthenticationConfiguration.CookieScheme,
                    new ClaimsPrincipal(identity));
            });
        app.MapGet("/api/protected", () => Results.Ok())
            .RequireAuthorization(ApplicationAccessPolicyConfiguration.PolicyName);
        await app.StartAsync(CancellationToken.None);

        return app;
    }
}
