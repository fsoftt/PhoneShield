using System.Net;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Tranqui.App.Core.Authentication;

namespace Tranqui.App.Core.Tests.Authentication;

public sealed class FirebaseAuthClientTests
{
    private const string ApiKey = "test-api-key";

    private static readonly DateTimeOffset now = new(2026, 10, 3, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task SignInAsync_Success_ReturnsSessionWithExpiry()
    {
        var handler = new StubHttpMessageHandler(HttpStatusCode.OK,
            """{"idToken":"id","refreshToken":"refresh","expiresIn":"3600","email":"ana@example.com"}""");

        var session = await CreateClient(handler).SignInAsync("ana@example.com", "secret123", CancellationToken.None);

        session.Should().Be(new AuthSession("id", "refresh", now.AddHours(1), "ana@example.com"));
        handler.LastRequest!.RequestUri!.ToString().Should().Contain("accounts:signInWithPassword").And.Contain($"key={ApiKey}");
        handler.LastRequestBody.Should().Contain("\"returnSecureToken\":true");
    }

    [Theory]
    [InlineData("INVALID_LOGIN_CREDENTIALS", "Correo o contraseña incorrectos.")]
    [InlineData("WEAK_PASSWORD : Password should be at least 6 characters", "La contraseña debe tener al menos 8 caracteres.")]
    [InlineData("SOMETHING_NEW", "No pudimos completar la operación. Intenta de nuevo en un momento.")]
    public async Task SignInAsync_FirebaseError_ThrowsWithSpanishMessage(string firebaseMessage, string expected)
    {
        var handler = new StubHttpMessageHandler(HttpStatusCode.BadRequest,
            $$$"""{"error":{"code":400,"message":"{{{firebaseMessage}}}"}}""");

        var act = () => CreateClient(handler).SignInAsync("ana@example.com", "wrong", CancellationToken.None);

        (await act.Should().ThrowAsync<FirebaseAuthException>()).Which.Message.Should().Be(expected);
    }

    [Fact]
    public async Task RefreshAsync_UsesTheSecureTokenEndpoint()
    {
        var handler = new StubHttpMessageHandler(HttpStatusCode.OK,
            """{"id_token":"new-id","refresh_token":"new-refresh","expires_in":"3600","user_id":"uid"}""");

        var session = await CreateClient(handler).RefreshAsync("old-refresh", CancellationToken.None);

        session.IdToken.Should().Be("new-id");
        session.RefreshToken.Should().Be("new-refresh");
        handler.LastRequest!.RequestUri!.Host.Should().Be("securetoken.googleapis.com");
        handler.LastRequestBody.Should().Contain("grant_type=refresh_token").And.Contain("refresh_token=old-refresh");
    }

    [Theory]
    [InlineData("""{"users":[{"emailVerified":true}]}""", true)]
    [InlineData("""{"users":[{"emailVerified":false}]}""", false)]
    [InlineData("""{}""", false)]
    public async Task IsEmailVerifiedAsync_ReadsTheLookupResponse(string json, bool expected)
    {
        var handler = new StubHttpMessageHandler(HttpStatusCode.OK, json);

        (await CreateClient(handler).IsEmailVerifiedAsync("id", CancellationToken.None)).Should().Be(expected);
    }

    private static FirebaseAuthClient CreateClient(HttpMessageHandler handler) =>
        new(new HttpClient(handler), Options.Create(new FirebaseOptions { ApiKey = ApiKey }), new FakeTimeProvider(now));
}
