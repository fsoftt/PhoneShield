using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Tranqui.Api.Tests;

/// <summary>Issues tokens shaped like Firebase ID tokens, signed with a key generated for the test run.</summary>
public static class TestTokens
{
    public const string ProjectId = "tranqui-test";
    public const string Issuer = "https://securetoken.google.com/" + ProjectId;

    private static readonly TimeSpan lifetime = TimeSpan.FromHours(1);

    public static SymmetricSecurityKey SigningKey { get; } = new(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));

    public static string Create(
        string firebaseUid,
        bool emailVerified = true,
        string audience = ProjectId,
        DateTime? expires = null) =>
        Issue(audience, expires,
            new Claim("sub", firebaseUid),
            new Claim("email_verified", emailVerified ? "true" : "false", ClaimValueTypes.Boolean));

    /// <summary>A token from an SMS sign-in: Firebase verified the number; there is no email.</summary>
    public static string CreateForPhone(string e164, DateTimeOffset signedInAt) =>
        Issue(ProjectId, null,
            new Claim("sub", Guid.NewGuid().ToString("N")),
            new Claim("phone_number", e164),
            new Claim(
                "auth_time",
                signedInAt.ToUnixTimeSeconds().ToString(System.Globalization.CultureInfo.InvariantCulture),
                ClaimValueTypes.Integer64));

    private static string Issue(string audience, DateTime? expires, params Claim[] claims)
    {
        var expiresAt = expires ?? DateTime.UtcNow.Add(lifetime);

        return new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = Issuer,
            Audience = audience,
            Subject = new ClaimsIdentity(claims),
            NotBefore = expiresAt - lifetime,
            Expires = expiresAt,
            IssuedAt = expiresAt - lifetime,
            SigningCredentials = new SigningCredentials(SigningKey, SecurityAlgorithms.HmacSha256),
        });
    }
}
