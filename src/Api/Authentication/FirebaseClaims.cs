namespace Tranqui.Api.Authentication;

/// <summary>Claim names in Firebase-issued ID tokens.</summary>
public static class FirebaseClaims
{
    public const string UserId = "sub";
    public const string EmailVerified = "email_verified";
    public const string True = "true";
}
