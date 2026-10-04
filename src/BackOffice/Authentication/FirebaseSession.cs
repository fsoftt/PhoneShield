namespace Tranqui.BackOffice.Authentication;

/// <summary>A signed-in Firebase user: the ID token sent to the API and the refresh token that renews it.</summary>
public sealed record FirebaseSession(string Uid, string Email, string IdToken, string RefreshToken, DateTimeOffset ExpiresAt);
