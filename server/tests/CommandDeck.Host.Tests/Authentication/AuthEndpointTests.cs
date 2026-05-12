using System.Net;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using CommandDeck.Host.Configuration;
using CommandDeck.Host.Features.Auth;
using Shouldly;

namespace CommandDeck.Host.Tests.Authentication;

public sealed class AuthEndpointTests
{
    [Fact]
    [Trait("Category", "Application")]
    public void ResolveLoginRedirectUri_WhenReturnUrlIsRootRelative_ReturnsReturnUrl()
    {
        var context = new DefaultHttpContext();
        context.Request.Scheme = "http";
        context.Request.Host = new HostString("localhost", 5173);

        string redirectUri = AuthEndpoint.ResolveLoginRedirectUri(
            "/after-login?tab=me#profile",
            context.Request);

        redirectUri.ShouldBe("/after-login?tab=me#profile");
    }

    [Fact]
    [Trait("Category", "Application")]
    public void ResolveLoginRedirectUri_WhenReturnUrlMatchesRequestOrigin_ReturnsAbsoluteReturnUrl()
    {
        var context = new DefaultHttpContext();
        context.Request.Scheme = "http";
        context.Request.Host = new HostString("localhost", 5173);

        string redirectUri = AuthEndpoint.ResolveLoginRedirectUri(
            "http://localhost:5173/after-login?tab=me#profile",
            context.Request);

        redirectUri.ShouldBe("http://localhost:5173/after-login?tab=me#profile");
    }

    [Fact]
    [Trait("Category", "Application")]
    public void ResolveLoginRedirectUri_WhenReturnUrlIsExternal_ReturnsRoot()
    {
        var context = new DefaultHttpContext();
        context.Request.Scheme = "http";
        context.Request.Host = new HostString("localhost", 5173);

        string redirectUri = AuthEndpoint.ResolveLoginRedirectUri(
            "https://example.test/after-login",
            context.Request);

        redirectUri.ShouldBe("/");
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task LoginRoute_WhenRequested_StartsOidcChallenge()
    {
        using var app = await CreateAuthHostAsync();
        using var client = app.GetTestClient();

        HttpResponseMessage response = await client.GetAsync(
            "/auth/login?returnUrl=/after-login",
            CancellationToken.None);

        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        response.Headers.Location.ShouldNotBeNull();
        response.Headers.Location.AbsoluteUri.ShouldStartWith("http://keycloak.test/authorize");
        response.Headers.Location.Query.ShouldContain("client_id=commanddeck-host");
        response.Headers.Location.Query.ShouldContain("redirect_uri=");
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task LoginRoute_WhenFrontendOriginIsForwarded_UsesFrontendOriginForOidcCallback()
    {
        using var app = await CreateAuthHostAsync();
        using var client = app.GetTestClient();

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            "/auth/login?returnUrl=http%3A%2F%2Flocalhost%3A5173%2Fafter-login%3Ftab%3Dme%23profile");
        request.Headers.Host = "localhost:5173";

        HttpResponseMessage response = await client.SendAsync(request, CancellationToken.None);

        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        response.Headers.Location.ShouldNotBeNull();
        var query = QueryHelpers.ParseQuery(response.Headers.Location.Query);
        query["redirect_uri"].ToString().ShouldBe("http://localhost:5173/auth/callback");
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task LoginRoute_WhenReturnUrlIsUntrustedAbsoluteUrl_IgnoresReturnUrlForOidcCallback()
    {
        using var app = await CreateAuthHostAsync();
        using var client = app.GetTestClient();

        HttpResponseMessage response = await client.GetAsync(
            "/auth/login?returnUrl=https%3A%2F%2Fexample.test%2Fafter-login",
            CancellationToken.None);

        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        response.Headers.Location.ShouldNotBeNull();
        var query = QueryHelpers.ParseQuery(response.Headers.Location.Query);
        query["redirect_uri"].ToString().ShouldNotContain("example.test");
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task LogoutRoute_WhenRequested_StartsCookieAndOidcSignout()
    {
        using var app = await CreateAuthHostAsync();
        using var client = app.GetTestClient();

        HttpResponseMessage response = await client.PostAsync(
            "/auth/logout",
            null,
            CancellationToken.None);

        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        response.Headers.Location.ShouldNotBeNull();
        response.Headers.Location.AbsoluteUri.ShouldStartWith("http://keycloak.test/logout");
    }

    private static async Task<WebApplication> CreateAuthHostAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Configuration["Authentication:Oidc:Authority"] =
            "http://localhost:8080/realms/commanddeck";
        builder.AddHostAuthentication();
        builder.Services.RemoveAll<IDistributedCache>();
        builder.Services.AddDistributedMemoryCache();
        builder.Services.PostConfigure<OpenIdConnectOptions>(
            HostAuthenticationConfiguration.OpenIdConnectScheme,
            options =>
            {
                var configuration = new OpenIdConnectConfiguration
                {
                    AuthorizationEndpoint = "http://keycloak.test/authorize",
                    EndSessionEndpoint = "http://keycloak.test/logout",
                };
                options.Configuration = configuration;
                options.ConfigurationManager =
                    new StaticConfigurationManager<OpenIdConnectConfiguration>(configuration);
            });

        var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapAuthEndpoints();
        await app.StartAsync(CancellationToken.None);

        return app;
    }
}
