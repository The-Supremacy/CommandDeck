using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using CommandDeck.Host.Authentication;
using CommandDeck.Host.Configuration;
using Shouldly;

namespace CommandDeck.Host.Tests.Authentication;

public sealed class HostAuthenticationConfigurationTests
{
    [Fact]
    [Trait("Category", "Application")]
    public void Host_authentication_uses_cookie_session_and_oidc_challenge_schemes()
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
