using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using CommandDeck.Host.Configuration;
using Shouldly;

namespace CommandDeck.Host.Tests.Authentication;

public sealed class CookieAuthenticationFailureTests
{
    [Fact]
    [Trait("Category", "Application")]
    public async Task Api_request_without_cookie_session_returns_unauthorized_without_login_redirect()
    {
        using var app = await CreateApiHostAsync();
        using var client = app.GetTestClient();

        HttpResponseMessage response = await client.GetAsync(
            "/api/protected",
            CancellationToken.None);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        response.Headers.Location.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Api_request_with_invalid_cookie_session_returns_unauthorized_without_login_redirect()
    {
        using var app = await CreateApiHostAsync();
        using var client = app.GetTestClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/protected");
        request.Headers.Add("Cookie", "CommandDeck.Session=not-a-valid-cookie");

        HttpResponseMessage response = await client.SendAsync(
            request,
            CancellationToken.None);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
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

        var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapGet("/api/protected", () => Results.Ok())
            .RequireAuthorization();
        await app.StartAsync(CancellationToken.None);

        return app;
    }
}
