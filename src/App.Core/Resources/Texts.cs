namespace Tranqui.App.Core.Resources;

/// <summary>User-facing text (es-CO). Moves to .resx files when a second language is added.</summary>
public static class Texts
{
    public const string EmailRequired = "Escribe tu correo.";
    public const string PasswordRequired = "Escribe tu contraseña.";
    public const string PasswordTooShort = "La contraseña debe tener al menos 8 caracteres.";
    public const string PasswordsDoNotMatch = "Las contraseñas no coinciden.";
    public const string TermsRequired = "Debes aceptar los términos y la política de tratamiento de datos.";
    public const string PasswordResetSent = "Si el correo tiene una cuenta, te enviamos un enlace para cambiar la contraseña.";
    public const string VerificationSent = "Te enviamos un nuevo correo de verificación.";
    public const string EmailNotVerifiedYet = "Aún no vemos tu correo verificado. Abre el enlace que te enviamos y vuelve a intentarlo.";
    public const string ConnectionError = "No pudimos conectarnos. Revisa tu conexión e intenta de nuevo.";
}
