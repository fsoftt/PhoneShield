namespace Tranqui.App.Core.Authentication;

/// <summary>Firebase Authentication REST API (email/password). No native SDK needed.</summary>
public interface IFirebaseAuthClient
{
    Task<AuthSession> SignUpAsync(string email, string password, CancellationToken cancellationToken);

    Task<AuthSession> SignInAsync(string email, string password, CancellationToken cancellationToken);

    Task<AuthSession> RefreshAsync(string refreshToken, CancellationToken cancellationToken);

    Task SendEmailVerificationAsync(string idToken, CancellationToken cancellationToken);

    Task SendPasswordResetAsync(string email, CancellationToken cancellationToken);

    Task<bool> IsEmailVerifiedAsync(string idToken, CancellationToken cancellationToken);

    Task DeleteAccountAsync(string idToken, CancellationToken cancellationToken);
}
