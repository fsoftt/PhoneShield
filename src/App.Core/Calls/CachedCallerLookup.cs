using System.Collections.Concurrent;
using Refit;
using Tranqui.App.Core.Api;
using Tranqui.Contracts.Lookups;
using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.App.Core.Calls;

/// <summary>
/// Android gives call screening about 5 seconds, so the lookup is capped at <see cref="Timeout"/>; recent answers are
/// cached on the device for <see cref="CacheLifetime"/> so repeated callers are identified instantly.
/// </summary>
internal sealed class CachedCallerLookup(ITranquiApi api, TimeProvider timeProvider) : ICallerLookup
{
    public static readonly TimeSpan Timeout = TimeSpan.FromSeconds(2);
    public static readonly TimeSpan CacheLifetime = TimeSpan.FromHours(24);

    private readonly ConcurrentDictionary<string, (LookupResponse Response, DateTimeOffset StoredAt)> cache = new();

    public async Task<LookupResponse?> LookupAsync(PhoneNumber number, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        if (cache.TryGetValue(number.E164, out var cached) && now - cached.StoredAt < CacheLifetime)
        {
            return cached.Response;
        }

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(Timeout);
        try
        {
            var response = await api.LookupAsync(new LookupRequest(number.E164), timeout.Token);
            cache[number.E164] = (response, now);
            return response;
        }
        catch (Exception exception) when (exception is HttpRequestException or ApiException or OperationCanceledException)
        {
            return null;
        }
    }
}
