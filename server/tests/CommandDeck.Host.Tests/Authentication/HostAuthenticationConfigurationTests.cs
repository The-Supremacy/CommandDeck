using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using CommandDeck.Host.Authentication;
using CommandDeck.Host.Configuration;
using Shouldly;

namespace CommandDeck.Host.Tests.Authentication;

public sealed class HostAuthenticationConfigurationTests
{
    [Fact]
    [Trait("Category", "Application")]
    public void HostAuthentication_WhenConfigured_UsesCookieSessionAndOidcChallengeSchemes()
    {
        using ServiceProvider services = BuildServices();

        AuthenticationOptions authOptions = services.GetRequiredService<IOptions<AuthenticationOptions>>()
            .Value;
        CookieAuthenticationOptions cookieOptions =
            services.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
                .Get(HostAuthenticationConfiguration.CookieScheme);
        OpenIdConnectOptions oidcOptions =
            services.GetRequiredService<IOptionsMonitor<OpenIdConnectOptions>>()
                .Get(HostAuthenticationConfiguration.OpenIdConnectScheme);

        authOptions.DefaultAuthenticateScheme.ShouldBe(HostAuthenticationConfiguration.CookieScheme);
        authOptions.DefaultChallengeScheme.ShouldBe(HostAuthenticationConfiguration.OpenIdConnectScheme);
        authOptions.DefaultSignOutScheme.ShouldBe(HostAuthenticationConfiguration.OpenIdConnectScheme);
        cookieOptions.SessionStore.ShouldBeOfType<RedisTicketStore>();
        oidcOptions.Authority.ShouldBe("http://localhost:8080/realms/commanddeck");
        oidcOptions.ClientId.ShouldBe("commanddeck-host");
        oidcOptions.CallbackPath.ToString().ShouldBe("/auth/callback");
        oidcOptions.SignedOutCallbackPath.ToString().ShouldBe("/auth/signed-out");
        oidcOptions.UsePkce.ShouldBeTrue();
        oidcOptions.SaveTokens.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task OidcTokenValidation_WhenPrincipalLacksProvider_StampsProviderClaimFromAuthority()
    {
        using ServiceProvider services = BuildServices();
        OpenIdConnectOptions oidcOptions =
            services.GetRequiredService<IOptionsMonitor<OpenIdConnectOptions>>()
                .Get(HostAuthenticationConfiguration.OpenIdConnectScheme);
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "subject-1")],
            HostAuthenticationConfiguration.OpenIdConnectScheme);
        var principal = new ClaimsPrincipal(identity);
        var context = new TokenValidatedContext(
            new DefaultHttpContext { RequestServices = services },
            new AuthenticationScheme(
                HostAuthenticationConfiguration.OpenIdConnectScheme,
                HostAuthenticationConfiguration.OpenIdConnectScheme,
                typeof(IAuthenticationHandler)),
            oidcOptions,
            principal,
            new AuthenticationProperties());

        await oidcOptions.Events.OnTokenValidated(context);

        principal.FindFirst(AppSessionClaimTypes.Provider)?.Value.ShouldBe("http://localhost:8080/realms/commanddeck");
    }

    private static ServiceProvider BuildServices()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration["Authentication:Oidc:Authority"] =
            "http://localhost:8080/realms/commanddeck";
        builder.AddHostAuthentication();
        builder.Services.RemoveAll<IDistributedCache>();
        builder.Services.AddDistributedMemoryCache();

        return builder.Services.BuildServiceProvider();
    }
}
