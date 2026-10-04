using Tranqui.Api.Authentication;

namespace Tranqui.Api.BackOffice;

public static class BackOfficeExtensions
{
    /// <summary>The back office (a separate app, src/BackOffice) calls the /v1/admin endpoints with an admin's token.</summary>
    public const string AdminPolicy = "back-office";

    public static IServiceCollection AddBackOffice(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(BackOfficeOptions.SectionName);
        services.Configure<BackOfficeOptions>(section);
        var adminUids = (section.Get<BackOfficeOptions>() ?? new BackOfficeOptions()).AdminUids.ToHashSet(StringComparer.Ordinal);

        services.AddAuthorizationBuilder().AddPolicy(AdminPolicy, policy => policy
            .RequireAuthenticatedUser()
            .RequireClaim(FirebaseClaims.EmailVerified, FirebaseClaims.True)
            .RequireAssertion(context => context.User.FindFirst(FirebaseClaims.UserId)?.Value is { } uid && adminUids.Contains(uid)));

        if (section.Get<BackOfficeOptions>()?.AutomaticPurge ?? true)
        {
            services.AddHostedService<DailyMaintenanceService>();
        }

        return services;
    }
}
