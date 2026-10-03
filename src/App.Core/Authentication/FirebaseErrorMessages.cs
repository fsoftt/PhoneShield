namespace Tranqui.App.Core.Authentication;

/// <summary>Maps Firebase Auth error codes to messages users can act on (es-CO).</summary>
internal static class FirebaseErrorMessages
{
    private const string Fallback = "No pudimos completar la operación. Intenta de nuevo en un momento.";

    private static readonly Dictionary<string, string> messages = new(StringComparer.Ordinal)
    {
        ["EMAIL_EXISTS"] = "Ya existe una cuenta con ese correo. Inicia sesión.",
        ["INVALID_EMAIL"] = "El correo no es válido.",
        ["INVALID_LOGIN_CREDENTIALS"] = "Correo o contraseña incorrectos.",
        ["INVALID_PASSWORD"] = "Correo o contraseña incorrectos.",
        ["EMAIL_NOT_FOUND"] = "Correo o contraseña incorrectos.",
        ["USER_DISABLED"] = "Esta cuenta está deshabilitada.",
        ["WEAK_PASSWORD"] = "La contraseña debe tener al menos 8 caracteres.",
        ["TOO_MANY_ATTEMPTS_TRY_LATER"] = "Demasiados intentos. Espera unos minutos e intenta de nuevo.",
        ["TOKEN_EXPIRED"] = "Tu sesión expiró. Inicia sesión de nuevo.",
        ["INVALID_REFRESH_TOKEN"] = "Tu sesión expiró. Inicia sesión de nuevo.",
        ["CREDENTIAL_TOO_OLD_LOGIN_AGAIN"] = "Por seguridad, inicia sesión de nuevo para continuar.",
    };

    /// <summary>Firebase messages look like "WEAK_PASSWORD : Password should be...": the code is the first token.</summary>
    public static FirebaseAuthException ToException(string? firebaseMessage)
    {
        var code = (firebaseMessage ?? string.Empty).Split(' ', 2)[0];

        return new FirebaseAuthException(code, messages.GetValueOrDefault(code, Fallback));
    }
}
