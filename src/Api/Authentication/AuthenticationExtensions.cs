using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Tranqui.Application.Abstractions;

namespace Tranqui.Api.Authentication;

public static class AuthenticationExtensions
{
    public const string PhoneVerifiedPolicy = "phone-verified";

    private const string FirebaseIssuerPrefix = "https://securetoken.google.com/";

    /// <summary>
    /// Validates Firebase ID tokens (signature via Google's public keys, issuer, audience, lifetime) and requires an
    /// authenticated user with a verified email on every endpoint unless it explicitly opts out.
    /// </summary>
    public static IServiceCollection AddFirebaseAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var projectId = configuration.GetSection(FirebaseAuthenticationOptions.SectionName)
            .Get<FirebaseAuthenticationOptions>()?.ProjectId;
        if (string.IsNullOrWhiteSpace(projectId))
        {
            throw new InvalidOperationException($"{FirebaseAuthenticationOptions.SectionName}:ProjectId is not configured.");
        }

        var issuer = FirebaseIssuerPrefix + projectId;

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = issuer;
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = issuer,
                    ValidAudience = projectId,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    NameClaimType = FirebaseClaims.UserId,
                };
            });

        // App endpoints need a verified email account. Appeal tokens come from a throwaway phone sign-in on the
        // website: they carry phone_number but no verified email, so they can only reach the appeal endpoint.
        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireClaim(FirebaseClaims.EmailVerified, FirebaseClaims.True)
                .Build())
            .AddPolicy(PhoneVerifiedPolicy, policy => policy
                .RequireAuthenticatedUser()
                .RequireClaim(FirebaseClaims.PhoneNumber));

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpContextCurrentUser>();
        services.AddScoped<IVerifiedPhone, HttpContextVerifiedPhone>();

        return services;
    }
}
