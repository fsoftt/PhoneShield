namespace Tranqui.App.Core.Authentication;

public interface IAuthService
{
    bool IsSignedIn { get; }

    string? Email { get; }

    /// <summary>Restores the previous session from secure storage. Returns false when the user must sign in again.</summary>
    Task<bool> RestoreAsync(CancellationToken cancellationToken);

    Task SignUpAsync(string email, string password, CancellationToken cancellationToken);

    Task SignInAsync(string email, string password, CancellationToken cancellationToken);

    Task SendPasswordResetAsync(string email, CancellationToken cancellationToken);

    Task ResendEmailVerificationAsync(CancellationToken cancellationToken);

    /// <summary>Checks with Firebase and, once verified, refreshes the token so it carries the verified claim.</summary>
    Task<bool> IsEmailVerifiedAsync(CancellationToken cancellationToken);

    /// <summary>A valid ID token, refreshed when close to expiry; null when signed out.</summary>
    Task<string?> GetIdTokenAsync(CancellationToken cancellationToken);

    Task DeleteIdentityAsync(CancellationToken cancellationToken);

    void SignOut();
}
