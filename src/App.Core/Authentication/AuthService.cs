namespace Tranqui.App.Core.Authentication;

internal sealed class AuthService(IFirebaseAuthClient firebase, ISecureStore secureStore, TimeProvider timeProvider)
    : IAuthService, IDisposable
{
    internal const string RefreshTokenKey = "tranqui.refresh_token";
    internal const string EmailKey = "tranqui.email";

    /// <summary>Tokens are refreshed this long before they expire, so a request never carries an expired one.</summary>
    internal static readonly TimeSpan RefreshMargin = TimeSpan.FromMinutes(5);

    private readonly SemaphoreSlim refreshLock = new(1, 1);
    private AuthSession? session;

    public bool IsSignedIn => session is not null;

    public string? Email => session?.Email;

    public async Task<bool> RestoreAsync(CancellationToken cancellationToken)
    {
        var refreshToken = await secureStore.GetAsync(RefreshTokenKey);
        if (string.IsNullOrEmpty(refreshToken))
        {
            return false;
        }

        try
        {
            var refreshed = await firebase.RefreshAsync(refreshToken, cancellationToken);
            await StoreAsync(refreshed with { Email = await secureStore.GetAsync(EmailKey) });
            return true;
        }
        catch (FirebaseAuthException)
        {
            SignOut();
            return false;
        }
    }

    public async Task SignUpAsync(string email, string password, CancellationToken cancellationToken)
    {
        await StoreAsync(await firebase.SignUpAsync(email, password, cancellationToken));
        await firebase.SendEmailVerificationAsync(session!.IdToken, cancellationToken);
    }

    public async Task SignInAsync(string email, string password, CancellationToken cancellationToken) =>
        await StoreAsync(await firebase.SignInAsync(email, password, cancellationToken));

    public Task SendPasswordResetAsync(string email, CancellationToken cancellationToken) =>
        firebase.SendPasswordResetAsync(email, cancellationToken);

    public async Task ResendEmailVerificationAsync(CancellationToken cancellationToken) =>
        await firebase.SendEmailVerificationAsync(await RequireIdTokenAsync(cancellationToken), cancellationToken);

    public async Task<bool> IsEmailVerifiedAsync(CancellationToken cancellationToken)
    {
        if (!await firebase.IsEmailVerifiedAsync(await RequireIdTokenAsync(cancellationToken), cancellationToken))
        {
            return false;
        }

        await RefreshAsync(cancellationToken);
        return true;
    }

    public async Task<string?> GetIdTokenAsync(CancellationToken cancellationToken)
    {
        if (session is null)
        {
            return null;
        }

        if (session.ExpiresAt - timeProvider.GetUtcNow() <= RefreshMargin)
        {
            await RefreshAsync(cancellationToken);
        }

        return session?.IdToken;
    }

    public async Task DeleteIdentityAsync(CancellationToken cancellationToken)
    {
        await firebase.DeleteAccountAsync(await RequireIdTokenAsync(cancellationToken), cancellationToken);
        SignOut();
    }

    public void SignOut()
    {
        session = null;
        secureStore.Remove(RefreshTokenKey);
        secureStore.Remove(EmailKey);
    }

    public void Dispose() => refreshLock.Dispose();

    private async Task<string> RequireIdTokenAsync(CancellationToken cancellationToken) =>
        await GetIdTokenAsync(cancellationToken) ?? throw new InvalidOperationException("No user is signed in.");

    private async Task RefreshAsync(CancellationToken cancellationToken)
    {
        await refreshLock.WaitAsync(cancellationToken);
        try
        {
            if (session is not null)
            {
                var refreshed = await firebase.RefreshAsync(session.RefreshToken, cancellationToken);
                await StoreAsync(refreshed with { Email = session.Email });
            }
        }
        finally
        {
            refreshLock.Release();
        }
    }

    private async Task StoreAsync(AuthSession newSession)
    {
        session = newSession;
        await secureStore.SetAsync(RefreshTokenKey, newSession.RefreshToken);
        if (newSession.Email is not null)
        {
            await secureStore.SetAsync(EmailKey, newSession.Email);
        }
    }
}
