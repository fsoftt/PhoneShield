namespace Tranqui.App.Core.Authentication;

public sealed record AuthSession(string IdToken, string RefreshToken, DateTimeOffset ExpiresAt, string? Email);
