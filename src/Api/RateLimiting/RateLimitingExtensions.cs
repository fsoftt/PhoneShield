using System.Threading.RateLimiting;
using Tranqui.Api.Authentication;

namespace Tranqui.Api.RateLimiting;

public static class RateLimitingExtensions
{
    public const string LookupPolicy = "lookup";

    private static readonly TimeSpan hour = TimeSpan.FromHours(1);
    private static readonly TimeSpan day = TimeSpan.FromDays(1);

    /// <summary>
    /// Per-user limits (keyed by the validated Firebase uid), held in memory: the API runs as a single instance.
    /// Lookups are capped per hour and per day to stop anyone from enumerating numbers to harvest names.
    /// </summary>
    public static IServiceCollection AddTranquiRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(RateLimitingOptions.SectionName).Get<RateLimitingOptions>()
            ?? new RateLimitingOptions();

        services.AddRateLimiter(limiter =>
        {
            limiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            limiter.AddPolicy(LookupPolicy, httpContext => RateLimitPartition.Get(
                httpContext.User.FindFirst(FirebaseClaims.UserId)?.Value ?? string.Empty,
                _ => RateLimiter.CreateChained(
                    FixedWindow(options.Lookup.PerHour, hour),
                    FixedWindow(options.Lookup.PerDay, day))));
        });

        return services;
    }

    private static FixedWindowRateLimiter FixedWindow(int permits, TimeSpan window) =>
        new(new FixedWindowRateLimiterOptions { PermitLimit = permits, Window = window, QueueLimit = 0 });
}
