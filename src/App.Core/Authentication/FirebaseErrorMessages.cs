using Tranqui.App.Core.Resources;

namespace Tranqui.App.Core.Authentication;

/// <summary>Maps Firebase Auth error codes to messages users can act on, in the device language.</summary>
internal static class FirebaseErrorMessages
{
    private static readonly Dictionary<string, Func<string>> messages = new(StringComparer.Ordinal)
    {
        ["EMAIL_EXISTS"] = () => Texts.FirebaseEmailExists,
        ["INVALID_EMAIL"] = () => Texts.FirebaseInvalidEmail,
        ["INVALID_LOGIN_CREDENTIALS"] = () => Texts.FirebaseWrongCredentials,
        ["INVALID_PASSWORD"] = () => Texts.FirebaseWrongCredentials,
        ["EMAIL_NOT_FOUND"] = () => Texts.FirebaseWrongCredentials,
        ["USER_DISABLED"] = () => Texts.FirebaseUserDisabled,
        ["WEAK_PASSWORD"] = () => Texts.PasswordTooShort,
        ["TOO_MANY_ATTEMPTS_TRY_LATER"] = () => Texts.FirebaseTooManyAttempts,
        ["TOKEN_EXPIRED"] = () => Texts.FirebaseSessionExpired,
        ["INVALID_REFRESH_TOKEN"] = () => Texts.FirebaseSessionExpired,
        ["CREDENTIAL_TOO_OLD_LOGIN_AGAIN"] = () => Texts.FirebaseSignInAgain,
        ["INVALID_CODE"] = () => Texts.FirebaseInvalidCode,
        ["SESSION_EXPIRED"] = () => Texts.FirebaseCodeExpired,
        ["INVALID_PHONE_NUMBER"] = () => Texts.InvalidPhoneNumber,
        ["QUOTA_EXCEEDED"] = () => Texts.FirebaseTooManyAttempts,
        ["INVALID_APP_CREDENTIAL"] = () => Texts.ErrorDeviceNotTrusted,
        ["MISSING_CLIENT_IDENTIFIER"] = () => Texts.ErrorDeviceNotTrusted,
    };

    /// <summary>Firebase messages look like "WEAK_PASSWORD : Password should be...": the code is the first token.</summary>
    public static FirebaseAuthException ToException(string? firebaseMessage)
    {
        var code = (firebaseMessage ?? string.Empty).Split(' ', 2)[0];
        var message = messages.TryGetValue(code, out var localized) ? localized() : Texts.FirebaseUnknownError;

        return new FirebaseAuthException(code, message);
    }
}
