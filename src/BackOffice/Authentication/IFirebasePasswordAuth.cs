namespace Tranqui.BackOffice.Authentication;

/// <summary>Firebase Authentication's REST API, called from the server only.</summary>
public interface IFirebasePasswordAuth
{
    /// <returns>The session, or null when the email or password is wrong.</returns>
    Task<FirebaseSession?> SignInAsync(string email, string password, CancellationToken cancellationToken);

    /// <returns>The renewed session, or null when the refresh token is no longer valid.</returns>
    Task<FirebaseSession?> RefreshAsync(FirebaseSession session, CancellationToken cancellationToken);
}
