namespace Tranqui.App.Core.Authentication;

/// <summary>Firebase Authentication REST API (email/password, and SMS for appeals). No native SDK needed.</summary>
public interface IFirebaseAuthClient
{
    Task<AuthSession> SignUpAsync(string email, string password, CancellationToken cancellationToken);

    Task<AuthSession> SignInAsync(string email, string password, CancellationToken cancellationToken);

    Task<AuthSession> RefreshAsync(string refreshToken, CancellationToken cancellationToken);

    Task SendEmailVerificationAsync(string idToken, CancellationToken cancellationToken);

    Task SendPasswordResetAsync(string email, CancellationToken cancellationToken);

    Task<bool> IsEmailVerifiedAsync(string idToken, CancellationToken cancellationToken);

    Task DeleteAccountAsync(string idToken, CancellationToken cancellationToken);

    /// <summary>Sends the SMS code; <paramref name="playIntegrityToken"/> proves the request comes from the genuine app.</summary>
    /// <returns>The session info that <see cref="SignInWithPhoneNumberAsync"/> needs.</returns>
    Task<string> SendVerificationCodeAsync(string e164, string playIntegrityToken, CancellationToken cancellationToken);

    /// <summary>Signs in a new, separate Firebase user with the SMS code and returns its ID token (the phone proof).</summary>
    Task<string> SignInWithPhoneNumberAsync(string sessionInfo, string code, CancellationToken cancellationToken);
}
