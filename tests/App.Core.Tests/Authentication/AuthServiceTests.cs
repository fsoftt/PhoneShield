using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Tranqui.App.Core.Authentication;

namespace Tranqui.App.Core.Tests.Authentication;

public sealed class AuthServiceTests : IDisposable
{
    private static readonly DateTimeOffset now = new(2026, 10, 3, 12, 0, 0, TimeSpan.Zero);

    private readonly IFirebaseAuthClient firebase = Substitute.For<IFirebaseAuthClient>();
    private readonly ISecureStore secureStore = Substitute.For<ISecureStore>();
    private readonly AuthService authService;

    public AuthServiceTests()
    {
        authService = new AuthService(firebase, secureStore, new FakeTimeProvider(now));
    }

    [Fact]
    public async Task GetIdTokenAsync_SignedOut_ReturnsNull()
    {
        (await authService.GetIdTokenAsync(CancellationToken.None)).Should().BeNull();
    }

    [Fact]
    public async Task GetIdTokenAsync_FreshToken_DoesNotRefresh()
    {
        await SignInWith(expiresAt: now.AddHours(1));

        (await authService.GetIdTokenAsync(CancellationToken.None)).Should().Be("id");
        await firebase.DidNotReceive().RefreshAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetIdTokenAsync_TokenAboutToExpire_RefreshesAndStoresTheNewRefreshToken()
    {
        await SignInWith(expiresAt: now.Add(AuthService.RefreshMargin));
        firebase.RefreshAsync("refresh", Arg.Any<CancellationToken>())
            .Returns(new AuthSession("new-id", "new-refresh", now.AddHours(1), null));

        (await authService.GetIdTokenAsync(CancellationToken.None)).Should().Be("new-id");
        await secureStore.Received().SetAsync(AuthService.RefreshTokenKey, "new-refresh");
        authService.Email.Should().Be("ana@example.com");
    }

    [Fact]
    public async Task RestoreAsync_WithoutStoredToken_ReturnsFalse()
    {
        (await authService.RestoreAsync(CancellationToken.None)).Should().BeFalse();
        authService.IsSignedIn.Should().BeFalse();
    }

    [Fact]
    public async Task RestoreAsync_ExpiredRefreshToken_SignsOut()
    {
        secureStore.GetAsync(AuthService.RefreshTokenKey).Returns("stale");
        firebase.RefreshAsync("stale", Arg.Any<CancellationToken>())
            .ThrowsAsync(new FirebaseAuthException("INVALID_REFRESH_TOKEN", "expired"));

        (await authService.RestoreAsync(CancellationToken.None)).Should().BeFalse();
        secureStore.Received().Remove(AuthService.RefreshTokenKey);
    }

    [Fact]
    public async Task SignUpAsync_SendsTheVerificationEmail()
    {
        firebase.SignUpAsync("ana@example.com", "secret123", Arg.Any<CancellationToken>())
            .Returns(new AuthSession("id", "refresh", now.AddHours(1), "ana@example.com"));

        await authService.SignUpAsync("ana@example.com", "secret123", CancellationToken.None);

        await firebase.Received(1).SendEmailVerificationAsync("id", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SignOut_ForgetsTheSessionAndStoredTokens()
    {
        await SignInWith(expiresAt: now.AddHours(1));

        authService.SignOut();

        authService.IsSignedIn.Should().BeFalse();
        secureStore.Received().Remove(AuthService.RefreshTokenKey);
        secureStore.Received().Remove(AuthService.EmailKey);
    }

    public void Dispose() => authService.Dispose();

    private async Task SignInWith(DateTimeOffset expiresAt)
    {
        firebase.SignInAsync("ana@example.com", "secret123", Arg.Any<CancellationToken>())
            .Returns(new AuthSession("id", "refresh", expiresAt, "ana@example.com"));
        await authService.SignInAsync("ana@example.com", "secret123", CancellationToken.None);
    }
}
