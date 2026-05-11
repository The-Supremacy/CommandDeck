using Microsoft.AspNetCore.Authentication;
using CommandDeck.Host.Configuration;

namespace CommandDeck.Host.Features.Auth;

public static class AuthEndpoint
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(
                "/auth/login",
                (string? returnUrl, HttpRequest request) =>
                {
                    return TypedResults.Challenge(
                        new AuthenticationProperties
                        {
                            RedirectUri = ResolveLoginRedirectUri(returnUrl, request)
                        },
                        [HostAuthenticationConfiguration.OpenIdConnectScheme]);
                })
            .ExcludeFromDescription();

        endpoints.MapPost(
                "/auth/logout",
                () => TypedResults.SignOut(
                    new AuthenticationProperties { RedirectUri = "/" },
                    [
                        HostAuthenticationConfiguration.CookieScheme,
                        HostAuthenticationConfiguration.OpenIdConnectScheme,
                    ]))
            .ExcludeFromDescription();

        return endpoints;
    }

    public static string ResolveLoginRedirectUri(string? returnUrl, HttpRequest request)
    {
        if (string.IsNullOrWhiteSpace(returnUrl))
        {
            return "/";
        }

        if (IsRootRelativeUrl(returnUrl))
        {
            return returnUrl;
        }

        return Uri.TryCreate(returnUrl, UriKind.Absolute, out Uri? absoluteReturnUrl)
            && IsSameOrigin(absoluteReturnUrl, request)
            ? absoluteReturnUrl.ToString()
            : "/";
    }

    private static bool IsRootRelativeUrl(string returnUrl)
    {
        return returnUrl[0] == '/'
            && !returnUrl.StartsWith("//", StringComparison.Ordinal)
            && !returnUrl.StartsWith("/\\", StringComparison.Ordinal);
    }

    private static bool IsSameOrigin(Uri uri, HttpRequest request)
    {
        int? uriPort = uri.IsDefaultPort ? DefaultPort(uri.Scheme) : uri.Port;
        int? requestPort = request.Host.Port ?? DefaultPort(request.Scheme);

        return StringComparer.OrdinalIgnoreCase.Equals(uri.Scheme, request.Scheme)
            && StringComparer.OrdinalIgnoreCase.Equals(uri.Host, request.Host.Host)
            && uriPort == requestPort;
    }

    private static int? DefaultPort(string scheme)
    {
        return scheme.ToLowerInvariant() switch
        {
            "http" => 80,
            "https" => 443,
            _ => null
        };
    }
}
