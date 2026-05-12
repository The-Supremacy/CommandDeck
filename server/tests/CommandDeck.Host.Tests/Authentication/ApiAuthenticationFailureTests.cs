using System.Net;
using CommandDeck.Host.Tests.Authentication;
using CommandDeck.Host.Tests.Support;
using Shouldly;

namespace CommandDeck.Host.Tests.Authentication;

public sealed class ApiAuthenticationFailureTests
{
    [Fact]
    [Trait("Category", "Application")]
    public async Task GetMe_WhenSessionIsMissing_ReturnsUnauthorizedWithoutRedirect()
    {
        await using var factory = new HostApplicationFactory();
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });

        HttpResponseMessage response = await client.GetAsync(
            "/api/me",
            CancellationToken.None);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        response.Headers.Location.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task GetMe_WhenSessionIsInvalid_ReturnsUnauthorizedWithoutRedirect()
    {
        await using var factory = new HostApplicationFactory();
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/me");
        request.Headers.Add(TestAuthenticationHandler.InvalidHeader, "true");

        HttpResponseMessage response = await client.SendAsync(
            request,
            CancellationToken.None);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        response.Headers.Location.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task GetMe_WhenSessionIsExpired_ReturnsUnauthorizedWithoutRedirect()
    {
        await using var factory = new HostApplicationFactory();
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/me");
        request.Headers.Add(TestAuthenticationHandler.InvalidHeader, "expired");

        HttpResponseMessage response = await client.SendAsync(
            request,
            CancellationToken.None);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        response.Headers.Location.ShouldBeNull();
    }
}
