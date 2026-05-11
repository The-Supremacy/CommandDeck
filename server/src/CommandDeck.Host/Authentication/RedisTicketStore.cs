using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using CommandDeck.Host.Configuration;

namespace CommandDeck.Host.Authentication;

public sealed class RedisTicketStore(IDistributedCache cache) : ITicketStore
{
    private static readonly TicketSerializer Serializer = TicketSerializer.Default;
    private static readonly TimeSpan TicketLifetime = TimeSpan.FromHours(8);

    public async Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        string key = $"ticket:{Guid.NewGuid():N}";
        await RenewAsync(key, ticket);

        return key;
    }

    public async Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        byte[] bytes = Serializer.Serialize(ticket);
        await cache.SetAsync(
            key,
            bytes,
            new DistributedCacheEntryOptions
            {
                SlidingExpiration = TicketLifetime,
            });
    }

    public async Task<AuthenticationTicket?> RetrieveAsync(string key)
    {
        byte[]? bytes = await cache.GetAsync(key);

        return bytes is null ? null : Serializer.Deserialize(bytes);
    }

    public Task RemoveAsync(string key)
    {
        return cache.RemoveAsync(key);
    }
}

public sealed class RedisTicketStoreCookieOptions(RedisTicketStore ticketStore)
    : IPostConfigureOptions<CookieAuthenticationOptions>
{
    public void PostConfigure(string? name, CookieAuthenticationOptions options)
    {
        if (name == HostAuthenticationConfiguration.CookieScheme)
        {
            options.SessionStore = ticketStore;
        }
    }
}
