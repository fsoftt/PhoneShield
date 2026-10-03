using System.Globalization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Tranqui.Application.Abstractions;
using Tranqui.Domain.Appeals;

namespace Tranqui.Api.Authentication;

/// <summary>
/// Validates the ID token of an appeal's SMS sign-in with the same rules (issuer, audience, lifetime, Google's keys)
/// as the bearer token, and requires that the sign-in happened within <see cref="AppealRules.PhoneProofMaxAge"/>.
/// </summary>
internal sealed class FirebasePhoneProofValidator(IOptionsMonitor<JwtBearerOptions> jwtOptions, TimeProvider timeProvider)
    : IPhoneProofValidator
{
    private readonly JsonWebTokenHandler handler = new() { MapInboundClaims = false };

    public async Task<string?> VerifiedNumberAsync(string phoneProof, CancellationToken cancellationToken)
    {
        var options = jwtOptions.Get(JwtBearerDefaults.AuthenticationScheme);
        var parameters = options.TokenValidationParameters.Clone();
        var configuration = options.Configuration
            ?? (options.ConfigurationManager is { } manager ? await manager.GetConfigurationAsync(cancellationToken) : null);
        if (configuration is not null)
        {
            parameters.IssuerSigningKeys = configuration.SigningKeys;
        }

        var result = await handler.ValidateTokenAsync(phoneProof, parameters);
        if (!result.IsValid
            || !result.Claims.TryGetValue(FirebaseClaims.PhoneNumber, out var phoneNumber)
            || !result.Claims.TryGetValue(FirebaseClaims.AuthTime, out var authTime))
        {
            return null;
        }

        var signedInAt = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(authTime, CultureInfo.InvariantCulture));

        return timeProvider.GetUtcNow() - signedInAt <= AppealRules.PhoneProofMaxAge ? phoneNumber as string : null;
    }
}
