namespace Tranqui.Api.Hosting;

public static class WebsiteCorsExtensions
{
    /// <summary>Lets the public website (e.g. https://fsoftt.github.io) call the appeal endpoints; nothing else.</summary>
    public const string WebsitePolicy = "website";

    private const string AllowedOriginsKey = "Cors:AllowedOrigins";

    public static IServiceCollection AddWebsiteCors(this IServiceCollection services, IConfiguration configuration)
    {
        var origins = configuration.GetSection(AllowedOriginsKey).Get<string[]>() ?? [];

        services.AddCors(cors => cors.AddPolicy(WebsitePolicy, policy => policy
            .WithOrigins(origins)
            .WithMethods(HttpMethods.Post)
            .WithHeaders("Authorization", "Content-Type")));

        return services;
    }
}
