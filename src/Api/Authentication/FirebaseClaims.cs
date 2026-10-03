namespace Tranqui.Api.Authentication;

/// <summary>Claim names in Firebase-issued ID tokens.</summary>
public static class FirebaseClaims
{
    public const string UserId = "sub";
    public const string EmailVerified = "email_verified";

    /// <summary>Present only when Firebase verified a phone number by SMS.</summary>
    public const string PhoneNumber = "phone_number";

    /// <summary>When the user signed in, in Unix seconds (unlike "iat", a token refresh does not move it).</summary>
    public const string AuthTime = "auth_time";
    public const string True = "true";
}
