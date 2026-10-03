using Microsoft.AspNetCore.HttpOverrides;

namespace Tranqui.Api.Hosting;

public static class ProxyExtensions
{
    /// <summary>
    /// In production the API is reachable only through Caddy (deploy/docker-compose.yml publishes no API port), and
    /// Caddy replaces any client-sent X-Forwarded-For. Trusting it gives per-IP rate limits the real client address.
    /// </summary>
    public static IServiceCollection AddReverseProxySupport(this IServiceCollection services) =>
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        });
}
