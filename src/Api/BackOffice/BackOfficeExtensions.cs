using Tranqui.Api.Authentication;

namespace Tranqui.Api.BackOffice;

public static class BackOfficeExtensions
{
    public const string AdminPolicy = "back-office";
    public const string PagePath = "/admin";

    /// <summary>The page only talks to this API and to Firebase Authentication's REST endpoints.</summary>
    private const string PageContentSecurityPolicy =
        "default-src 'self'; connect-src 'self' https://identitytoolkit.googleapis.com https://securetoken.googleapis.com; "
        + "frame-ancestors 'none'; base-uri 'none'; form-action 'none'";

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
            services.AddHostedService<DailyPurgeService>();
        }

        return services;
    }

    /// <summary>Serves the static page at /admin with a strict content security policy.</summary>
    public static WebApplication UseBackOfficePage(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            if (context.Request.Path.StartsWithSegments(PagePath))
            {
                context.Response.Headers.ContentSecurityPolicy = PageContentSecurityPolicy;
            }

            await next(context);
        });
        app.UseDefaultFiles();
        app.UseStaticFiles();

        return app;
    }
}
