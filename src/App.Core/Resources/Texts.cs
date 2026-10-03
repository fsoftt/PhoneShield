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

    public const string PrivateNumberTitle = "Número privado";
    public const string PrivateNumberSubtitle = "Quien llama ocultó su número.";
    public const string KnownContactSubtitle = "Está en tus contactos.";
    public const string KnownContactButSpamSubtitle = "Está en tus contactos. La comunidad lo reportó como spam.";
    public const string SpamTitle = "Posible spam";
    public const string SpamSubtitleFormat = "Reportado por {0} personas.";
    public const string IdentifiedSubtitle = "Así lo conoce la comunidad.";
    public const string UnknownTitle = "Número desconocido";
    public const string UnknownSubtitle = "No tenemos datos de este número.";
    public const string OfflineSubtitle = "Sin conexión: no pudimos consultar este número.";

    public const string StepCallScreeningTitle = "Identificar y filtrar llamadas";
    public const string StepCallScreeningExplanation = "Permite que Tranqui revise quién llama antes de que suene el teléfono.";
    public const string StepOverlayTitle = "Mostrar el aviso sobre la llamada";
    public const string StepOverlayExplanation = "Muestra el aviso de color encima de la pantalla de llamada entrante.";
    public const string StepContactsTitle = "Reconocer tus contactos";
    public const string StepContactsExplanation = "Lee tu agenda solo en este teléfono para mostrar en verde a quien ya conoces. No se envía a ningún lado.";
    public const string StepNotificationsTitle = "Avisarte de llamadas bloqueadas";
    public const string StepNotificationsExplanation = "Te contamos cuándo bloqueamos una llamada y te preguntamos si fue spam.";
    public const string ProtectionActive = "Protección activa";
    public const string ProtectionIncomplete = "Completa estos pasos para activar la protección";

    public const string DeleteAccountTitle = "¿Borrar tu cuenta?";
    public const string DeleteAccountMessage = "Borraremos tu cuenta, tus reportes y los contactos que aportaste. No se puede deshacer.";
    public const string DeleteAccountAccept = "Borrar";
    public const string Cancel = "Cancelar";
    public const string MyDataSummaryFormat = "Cuenta creada el {0:d}. Reportes enviados: {1}. Contactos aportados: {2}.";
    public const string NoBlockedNumbers = "No has bloqueado ningún número.";
}
