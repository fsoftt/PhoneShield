using System.Threading.RateLimiting;
using Tranqui.Api.Authentication;
using Tranqui.Contracts.Errors;

namespace Tranqui.Api.RateLimiting;

public static class RateLimitingExtensions
{
    public const string LookupPolicy = "lookup";
    public const string ReportPolicy = "report";
    public const string ContactUploadPolicy = "contact-upload";

    private const string TooManyRequestsTitle = "Too many requests. Try again later.";

    private static readonly TimeSpan hour = TimeSpan.FromHours(1);
    private static readonly TimeSpan day = TimeSpan.FromDays(1);

    /// <summary>
    /// Per-user limits (keyed by the validated Firebase uid), held in memory: the API runs as a single instance.
    /// Lookups are capped per hour and per day to stop anyone from enumerating numbers to harvest names;
    /// reports are capped per day to slow down coordinated false reporting.
    /// </summary>
    public static IServiceCollection AddTranquiRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(RateLimitingOptions.SectionName).Get<RateLimitingOptions>()
            ?? new RateLimitingOptions();

        services.AddRateLimiter(limiter =>
        {
            limiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            limiter.OnRejected = async (context, _) =>
                await context.HttpContext.RequestServices.GetRequiredService<IProblemDetailsService>().WriteAsync(new ProblemDetailsContext
                {
                    HttpContext = context.HttpContext,
                    ProblemDetails =
                    {
                        Title = TooManyRequestsTitle,
                        Status = StatusCodes.Status429TooManyRequests,
                        Extensions = { [ApiErrorCodes.Field] = ApiErrorCodes.RateLimited },
                    },
                });
            limiter.AddPolicy(LookupPolicy, httpContext => RateLimitPartition.Get(
                UserPartitionKey(httpContext),
                _ => RateLimiter.CreateChained(
                    FixedWindow(options.Lookup.PerHour, hour),
                    FixedWindow(options.Lookup.PerDay, day))));
            limiter.AddPolicy(ReportPolicy, httpContext => RateLimitPartition.Get(
                UserPartitionKey(httpContext),
                _ => FixedWindow(options.Reports.PerDay, day)));
            limiter.AddPolicy(ContactUploadPolicy, httpContext => RateLimitPartition.Get(
                UserPartitionKey(httpContext),
                _ => FixedWindow(options.ContactUploads.BatchesPerDay, day)));
        });

        return services;
    }

    private static string UserPartitionKey(HttpContext httpContext) =>
        httpContext.User.FindFirst(FirebaseClaims.UserId)?.Value ?? string.Empty;

    private static FixedWindowRateLimiter FixedWindow(int permits, TimeSpan window) =>
        new(new FixedWindowRateLimiterOptions { PermitLimit = permits, Window = window, QueueLimit = 0 });
}
