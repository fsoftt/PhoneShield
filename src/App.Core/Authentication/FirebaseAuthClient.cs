using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace Tranqui.App.Core.Authentication;

internal sealed class FirebaseAuthClient(HttpClient httpClient, IOptions<FirebaseOptions> options, TimeProvider timeProvider)
    : IFirebaseAuthClient
{
    private const string IdentityToolkitBase = "https://identitytoolkit.googleapis.com/v1/accounts:";
    private const string SecureTokenUrl = "https://securetoken.googleapis.com/v1/token";
    private const string VerifyEmailRequest = "VERIFY_EMAIL";
    private const string PasswordResetRequest = "PASSWORD_RESET";

    private readonly string apiKey = options.Value.ApiKey;

    public Task<AuthSession> SignUpAsync(string email, string password, CancellationToken cancellationToken) =>
        PasswordAuthAsync("signUp", email, password, cancellationToken);

    public Task<AuthSession> SignInAsync(string email, string password, CancellationToken cancellationToken) =>
        PasswordAuthAsync("signInWithPassword", email, password, cancellationToken);

    public async Task<AuthSession> RefreshAsync(string refreshToken, CancellationToken cancellationToken)
    {
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = refreshToken,
        });
        using var response = await httpClient.PostAsync(new Uri($"{SecureTokenUrl}?key={apiKey}"), content, cancellationToken);
        var body = await ReadAsync<RefreshResponse>(response, cancellationToken);

        return new AuthSession(body.IdToken, body.RefreshToken, ExpiresAt(body.ExpiresIn), Email: null);
    }

    public Task SendEmailVerificationAsync(string idToken, CancellationToken cancellationToken) =>
        PostAsync<object>("sendOobCode", new { requestType = VerifyEmailRequest, idToken }, cancellationToken);

    public Task SendPasswordResetAsync(string email, CancellationToken cancellationToken) =>
        PostAsync<object>("sendOobCode", new { requestType = PasswordResetRequest, email }, cancellationToken);

    public async Task<bool> IsEmailVerifiedAsync(string idToken, CancellationToken cancellationToken)
    {
        var body = await PostAsync<LookupResponse>("lookup", new { idToken }, cancellationToken);

        return body.Users is { Count: > 0 } users && users[0].EmailVerified;
    }

    public Task DeleteAccountAsync(string idToken, CancellationToken cancellationToken) =>
        PostAsync<object>("delete", new { idToken }, cancellationToken);

    public async Task<string> SendVerificationCodeAsync(string e164, string playIntegrityToken, CancellationToken cancellationToken)
    {
        var body = await PostAsync<SendVerificationCodeResponse>(
            "sendVerificationCode", new { phoneNumber = e164, playIntegrityToken }, cancellationToken);

        return body.SessionInfo;
    }

    public async Task<string> SignInWithPhoneNumberAsync(string sessionInfo, string code, CancellationToken cancellationToken)
    {
        var body = await PostAsync<PhoneSignInResponse>("signInWithPhoneNumber", new { sessionInfo, code }, cancellationToken);

        return body.IdToken;
    }

    private async Task<AuthSession> PasswordAuthAsync(string method, string email, string password, CancellationToken cancellationToken)
    {
        var body = await PostAsync<PasswordAuthResponse>(
            method, new { email, password, returnSecureToken = true }, cancellationToken);

        return new AuthSession(body.IdToken, body.RefreshToken, ExpiresAt(body.ExpiresIn), body.Email);
    }

    private async Task<T> PostAsync<T>(string method, object payload, CancellationToken cancellationToken)
    {
        using var response = await httpClient.PostAsJsonAsync(
            new Uri($"{IdentityToolkitBase}{method}?key={apiKey}"), payload, cancellationToken);

        return await ReadAsync<T>(response, cancellationToken);
    }

    private static async Task<T> ReadAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(cancellationToken);
            throw FirebaseErrorMessages.ToException(error?.Error?.Message);
        }

        return (await response.Content.ReadFromJsonAsync<T>(cancellationToken))!;
    }

    private DateTimeOffset ExpiresAt(string expiresInSeconds) =>
        timeProvider.GetUtcNow().AddSeconds(double.Parse(expiresInSeconds, CultureInfo.InvariantCulture));

    private sealed record PasswordAuthResponse(string IdToken, string RefreshToken, string ExpiresIn, string? Email);

    private sealed record RefreshResponse(
        [property: JsonPropertyName("id_token")] string IdToken,
        [property: JsonPropertyName("refresh_token")] string RefreshToken,
        [property: JsonPropertyName("expires_in")] string ExpiresIn);

    private sealed record SendVerificationCodeResponse(string SessionInfo);

    private sealed record PhoneSignInResponse(string IdToken);

    private sealed record LookupResponse(IReadOnlyList<LookupUser>? Users);

    private sealed record LookupUser(bool EmailVerified);

    private sealed record ErrorResponse(ErrorBody? Error);

    private sealed record ErrorBody(string? Message);
}
