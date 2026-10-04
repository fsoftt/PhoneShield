using System.Globalization;
using System.Net;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace Tranqui.BackOffice.Authentication;

internal sealed class FirebasePasswordAuth(HttpClient httpClient, IOptions<BackOfficeOptions> options, TimeProvider timeProvider)
    : IFirebasePasswordAuth
{
    private const string SignInUrl = "https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key=";
    private const string RefreshUrl = "https://securetoken.googleapis.com/v1/token?key=";

    private string apiKey => Uri.EscapeDataString(options.Value.FirebaseApiKey);

    public async Task<FirebaseSession?> SignInAsync(string email, string password, CancellationToken cancellationToken)
    {
        using var response = await httpClient.PostAsJsonAsync(
            new Uri(SignInUrl + apiKey), new { email, password, returnSecureToken = true }, cancellationToken);
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        var body = (await response.Content.ReadFromJsonAsync<SignInResponse>(cancellationToken))!;

        return new FirebaseSession(body.LocalId, body.Email, body.IdToken, body.RefreshToken, ExpiresAt(body.ExpiresIn));
    }

    public async Task<FirebaseSession?> RefreshAsync(FirebaseSession session, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session);

        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = session.RefreshToken,
        });
        using var response = await httpClient.PostAsync(new Uri(RefreshUrl + apiKey), content, cancellationToken);
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        var body = (await response.Content.ReadFromJsonAsync<RefreshResponse>(cancellationToken))!;

        return session with { IdToken = body.IdToken, RefreshToken = body.RefreshToken, ExpiresAt = ExpiresAt(body.ExpiresIn) };
    }

    private DateTimeOffset ExpiresAt(string expiresInSeconds) =>
        timeProvider.GetUtcNow().AddSeconds(double.Parse(expiresInSeconds, CultureInfo.InvariantCulture));

    private sealed record SignInResponse(string LocalId, string Email, string IdToken, string RefreshToken, string ExpiresIn);

    private sealed record RefreshResponse(
        [property: JsonPropertyName("id_token")] string IdToken,
        [property: JsonPropertyName("refresh_token")] string RefreshToken,
        [property: JsonPropertyName("expires_in")] string ExpiresIn);
}
