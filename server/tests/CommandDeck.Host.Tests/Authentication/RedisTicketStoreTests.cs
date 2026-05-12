using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using CommandDeck.Host.Authentication;
using CommandDeck.Host.Configuration;
using Shouldly;

namespace CommandDeck.Host.Tests.Authentication;

public sealed class RedisTicketStoreTests
{
    [Fact]
    [Trait("Category", "Application")]
    public async Task TicketStore_WhenTicketIsStored_RoundTripsAndRemovesAuthenticationTicket()
    {
        using ServiceProvider services = new ServiceCollection()
            .AddDistributedMemoryCache()
            .BuildServiceProvider();
        var cache = services.GetRequiredService<IDistributedCache>();
        var store = new RedisTicketStore(cache);
        var ticket = new AuthenticationTicket(
            new ClaimsPrincipal(
                new ClaimsIdentity(
                    [new Claim(ClaimTypes.NameIdentifier, "subject-1")],
                    HostAuthenticationConfiguration.CookieScheme)),
            HostAuthenticationConfiguration.CookieScheme);

        string key = await store.StoreAsync(ticket);
        AuthenticationTicket? retrieved = await store.RetrieveAsync(key);

        retrieved.ShouldNotBeNull();
        retrieved.Principal.FindFirstValue(ClaimTypes.NameIdentifier)
            .ShouldBe("subject-1");

        await store.RemoveAsync(key);

        AuthenticationTicket? removed = await store.RetrieveAsync(key);
        removed.ShouldBeNull();
    }
}
