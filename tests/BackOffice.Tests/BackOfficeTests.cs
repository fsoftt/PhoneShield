using System.Net;

namespace Tranqui.BackOffice.Tests;

public sealed class BackOfficeTests(BackOfficeFactory factory) : IClassFixture<BackOfficeFactory>
{
    [Fact]
    public async Task Anonymous_IsSentToSignIn()
    {
        using var client = factory.CreateBrowser();

        var response = await client.GetAsync(new Uri("/appeals", UriKind.Relative), TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.RedirectTarget().Should().Contain("/login");
    }

    [Fact]
    public async Task SignIn_WrongPassword_ExplainsAndStaysOut()
    {
        using var client = factory.CreateBrowser();

        var response = await client.PostFormAsync("/login", "/login",
            ("_handler", "login"), ("Input.Email", FakeFirebase.AdminEmail), ("Input.Password", "wrong"));

        (await response.ReadPageAsync()).Should().Contain("Correo o contraseña incorrectos.");
        var cookies = response.Headers.TryGetValues("Set-Cookie", out var values) ? values : [];
        cookies.Should().NotContain(cookie => cookie.StartsWith("__Host-tranqui-backoffice", StringComparison.Ordinal));
    }

    [Fact]
    public async Task SignIn_AccountThatIsNotAnAdmin_GetsNoSession()
    {
        using var client = factory.CreateBrowser();

        var response = await client.SignInAsync(FakeFirebase.UserEmail);

        (await response.ReadPageAsync()).Should().Contain("no tiene acceso");
        (await client.GetAsync(new Uri("/", UriKind.Relative), TestContext.Current.CancellationToken)).StatusCode.Should().Be(HttpStatusCode.Redirect);
    }

    [Fact]
    public async Task SignIn_Admin_SeesTheOverview()
    {
        using var client = factory.CreateBrowser();

        var response = await client.SignInAsync(FakeFirebase.AdminEmail);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.RedirectTarget().Should().Be("/");
        (await client.GetPageAsync("/")).Should().Contain("1234").And.Contain("7890");
    }

    [Fact]
    public async Task SignIn_ExternalReturnUrl_StaysOnTheSite()
    {
        using var client = factory.CreateBrowser();

        var response = await client.SignInAsync(FakeFirebase.AdminEmail, returnUrl: "//evil.example/phish");

        response.RedirectTarget().Should().Be("/");
    }

    [Fact]
    public async Task Appeals_RenderUserTextAsText()
    {
        using var client = await SignedInAdminAsync();

        var page = await client.GetPageAsync("/appeals");

        var raw = await (await client.GetAsync(new Uri("/appeals", UriKind.Relative), TestContext.Current.CancellationToken))
            .Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        raw.Should().Contain("&lt;b&gt;no html&lt;/b&gt;").And.NotContain("<b>no html</b>");
        page.Should().Contain("mailto:owner@example.com");
    }

    [Fact]
    public async Task Approve_CallsTheApiAndComesBackWithANotice()
    {
        using var client = await SignedInAdminAsync();

        var response = await client.PostFormAsync("/appeals", $"/appeals/{StubAdminApi.PendingAppealId}/resolution",
            ("decision", "Approve"), ("returnStatus", "Pending"));

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.RedirectTarget().Should().Be("/appeals?status=Pending&notice=resolved");
        factory.Api.Resolutions.Should().Contain((StubAdminApi.PendingAppealId, Contracts.BackOffice.AppealDecisionDto.Approve));
    }

    [Fact]
    public async Task Purge_ReportsWhatWasDeleted()
    {
        using var client = await SignedInAdminAsync();

        var response = await client.PostFormAsync("/", "/purges");

        response.RedirectTarget().Should().Be("/?notice=purged&quotaUses=4&appeals=5&reports=6&blocks=7");
        (await client.GetPageAsync(response.RedirectTarget())).Should().Contain("4 usos de cuota, 5 apelaciones, 6 reportes y 7 bloqueos");
    }

    [Fact]
    public async Task FormPost_WithoutAntiforgeryToken_IsRejected()
    {
        using var client = await SignedInAdminAsync();

        var response = await client.PostAsync(
            new Uri("/purges", UriKind.Relative), new FormUrlEncodedContent([]), TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ExpiringSession_IsRenewedBeforeCallingTheApi()
    {
        factory.Firebase.SessionLifetime = TimeSpan.FromMinutes(1);
        using var client = await SignedInAdminAsync();
        factory.Firebase.SessionLifetime = TimeSpan.FromHours(1);

        await client.GetPageAsync("/");

        factory.Firebase.Refreshes.Should().BeGreaterThan(0);
        factory.Api.ReceivedTokens.Should().Contain(FakeFirebase.RenewedAdminToken);
    }

    [Fact]
    public async Task Pages_SendStrictSecurityHeaders()
    {
        using var client = factory.CreateBrowser();

        var response = await client.GetAsync(new Uri("/login", UriKind.Relative), TestContext.Current.CancellationToken);

        response.Headers.GetValues("Content-Security-Policy").Single().Should().Contain("default-src 'self'").And.Contain("frame-ancestors 'none'");
        response.Headers.GetValues("X-Content-Type-Options").Single().Should().Be("nosniff");
    }

    [Fact]
    public async Task EnglishBrowser_GetsEnglish()
    {
        using var client = factory.CreateBrowser();
        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US");

        (await client.GetPageAsync("/login")).Should().Contain("Sign in").And.Contain("lang=\"en\"");
    }

    private async Task<HttpClient> SignedInAdminAsync()
    {
        var client = factory.CreateBrowser();
        (await client.SignInAsync(FakeFirebase.AdminEmail)).StatusCode.Should().Be(HttpStatusCode.Redirect);

        return client;
    }
}
