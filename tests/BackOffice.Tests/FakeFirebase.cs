using Tranqui.BackOffice.Authentication;

namespace Tranqui.BackOffice.Tests;

/// <summary>Stands in for Firebase: two known accounts, an admin and a regular user.</summary>
internal sealed class FakeFirebase : IFirebasePasswordAuth
{
    public const string AdminEmail = "admin@example.com";
    public const string UserEmail = "user@example.com";
    public const string Password = "correct-password";
    public const string AdminToken = "admin-token";
    public const string RenewedAdminToken = "admin-token-renewed";
    public const string UserToken = "user-token";

    /// <summary>How long new sessions last; tests shorten it to force a renewal.</summary>
    public TimeSpan SessionLifetime { get; set; } = TimeSpan.FromHours(1);

    public int Refreshes { get; private set; }

    public Task<FirebaseSession?> SignInAsync(string email, string password, CancellationToken cancellationToken)
    {
        FirebaseSession? session = (email, password) switch
        {
            (AdminEmail, Password) => new("admin-uid", email, AdminToken, "refresh", DateTimeOffset.UtcNow + SessionLifetime),
            (UserEmail, Password) => new("user-uid", email, UserToken, "refresh", DateTimeOffset.UtcNow + SessionLifetime),
            _ => null,
        };

        return Task.FromResult(session);
    }

    public Task<FirebaseSession?> RefreshAsync(FirebaseSession session, CancellationToken cancellationToken)
    {
        Refreshes++;

        return Task.FromResult<FirebaseSession?>(session with
        {
            IdToken = session.IdToken == AdminToken ? RenewedAdminToken : session.IdToken,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(1),
        });
    }
}
