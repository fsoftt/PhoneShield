using System.Net;
using System.Net.Http.Json;
using Tranqui.Api.Endpoints;
using Tranqui.Contracts.Appeals;
using Tranqui.Contracts.BackOffice;
using Tranqui.Contracts.Errors;
using Tranqui.Contracts.Lookups;
using Tranqui.Contracts.Reports;
using Tranqui.Domain.Appeals;
using Tranqui.Domain.PhoneNumbers;
using Tranqui.Domain.Reputation;

namespace Tranqui.Api.Tests.Endpoints;

public sealed class BackOfficeTests(AppealApiFactory factory) : IClassFixture<AppealApiFactory>
{
    [Fact]
    public async Task Config_IsAnonymous()
    {
        using var client = factory.CreateClientFor(token: null);

        var config = await client.GetFromJsonAsync<BackOfficeConfigResponse>(GetBackOfficeConfig.Route, TestContext.Current.CancellationToken);

        config!.FirebaseApiKey.Should().Be("public-web-key");
    }

    [Fact]
    public async Task Page_IsServedWithAStrictContentSecurityPolicy()
    {
        using var client = factory.CreateClientFor(token: null);

        var response = await client.GetAsync(new Uri("/admin/", UriKind.Relative), TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/html");
        response.Headers.GetValues("Content-Security-Policy").Single().Should().StartWith("default-src 'self'");
    }

    [Theory]
    [InlineData(GetBackOfficeOverview.Route)]
    [InlineData(ListAppeals.Route)]
    public async Task AdminEndpoints_RegularUser_IsForbidden(string route)
    {
        using var client = factory.CreateClientFor(TestTokens.Create(Guid.NewGuid().ToString("N")));

        var response = await client.GetAsync(new Uri(route, UriKind.Relative), TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminEndpoints_AdminWithUnverifiedEmail_IsForbidden()
    {
        using var client = factory.CreateClientFor(TestTokens.Create(TranquiApiFactory.AdminUid, emailVerified: false));

        var response = await client.GetAsync(new Uri(GetBackOfficeOverview.Route, UriKind.Relative), TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ApprovingASpamReview_ClearsEarlierReportsAndErasesThePersonalData()
    {
        const string number = "3002223301";
        using var owner = await EstablishedOwnerAsync();
        await ReportSpamUntilFlaggedAsync(number);
        (await LookupAsync(number)).Status.Should().Be(CallerStatusDto.Spam);
        await SubmitSpamReviewAsync(owner, number, "Es la línea de mi tienda", "owner@example.com");
        using var admin = Admin();

        var pending = await ListAsync(admin, AppealStatusDto.Pending);
        var review = pending.Single(appeal => appeal.Reason == "Es la línea de mi tienda");
        review.ContactEmail.Should().Be("owner@example.com");
        review.NumberStatus.Should().Be(CallerStatusDto.Spam);

        var resolution = await admin.PostAsJsonAsync(
            ResolveAppeal.RouteFor(review.Id),
            new ResolveAppealRequest(AppealDecisionDto.Approve),
            ApiClientExtensions.JsonOptions,
            TestContext.Current.CancellationToken);

        resolution.StatusCode.Should().Be(HttpStatusCode.OK);
        (await LookupAsync(number)).Status.Should().NotBe(CallerStatusDto.Spam);
        var approved = (await ListAsync(admin, AppealStatusDto.Approved)).Single(appeal => appeal.Id == review.Id);
        approved.Reason.Should().BeNull();
        approved.ContactEmail.Should().BeNull();
    }

    [Fact]
    public async Task ResolvingTwice_IsAConflict()
    {
        await FileSpamReviewAsync("3002223302", "Twice", null);
        using var admin = Admin();
        var review = (await ListAsync(admin, AppealStatusDto.Pending)).Single(appeal => appeal.Reason == "Twice");
        await ResolveAsync(admin, review.Id, AppealDecisionDto.Reject);

        var second = await ResolveAsync(admin, review.Id, AppealDecisionDto.Approve);

        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await second.ProblemCodeAsync()).Should().Be(ApiErrorCodes.AppealAlreadyResolved);
    }

    [Fact]
    public async Task Resolving_UnknownAppeal_IsNotFound()
    {
        using var admin = Admin();

        var response = await ResolveAsync(admin, Guid.NewGuid(), AppealDecisionDto.Reject);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Overview_CountsPendingAppeals()
    {
        await FileSpamReviewAsync("3002223303", "Overview", null);
        using var admin = Admin();

        var overview = await admin.GetFromJsonAsync<BackOfficeOverviewResponse>(
            GetBackOfficeOverview.Route, TestContext.Current.CancellationToken);

        overview!.PendingAppeals.Should().BeGreaterThan(0);
        overview.Accounts.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Purge_DeletesQuotaUsesOlderThanAYear()
    {
        await FileSpamReviewAsync("3002223304", "Purge", null);
        factory.Clock.Advance(AppealRules.YearWindow + TimeSpan.FromDays(1));
        using var admin = Admin();

        var response = await admin.PostAsync(new Uri(PurgeExpiredData.Route, UriKind.Relative), null, TestContext.Current.CancellationToken);

        var result = await response.Content.ReadFromJsonAsync<PurgeResponse>(TestContext.Current.CancellationToken);
        result!.AppealQuotaUsages.Should().BeGreaterThanOrEqualTo(3);
    }

    private HttpClient Admin() => factory.CreateClientFor(TestTokens.Create(TranquiApiFactory.AdminUid));

    private static async Task<List<AppealReviewResponse>> ListAsync(HttpClient admin, AppealStatusDto status) =>
        (await admin.GetFromJsonAsync<List<AppealReviewResponse>>(
            $"{ListAppeals.Route}?status={status}", ApiClientExtensions.JsonOptions, TestContext.Current.CancellationToken))!;

    private static Task<HttpResponseMessage> ResolveAsync(HttpClient admin, Guid id, AppealDecisionDto decision) =>
        admin.PostAsJsonAsync(
            ResolveAppeal.RouteFor(id), new ResolveAppealRequest(decision), ApiClientExtensions.JsonOptions, TestContext.Current.CancellationToken);

    private async Task ReportSpamUntilFlaggedAsync(string number)
    {
        var reportersNeeded = (int)Math.Ceiling(ReputationRules.MinimumSpamWeight / ReputationRules.NewAccountVoteWeight);
        for (var i = 0; i < reportersNeeded; i++)
        {
            using var reporter = await factory.CreateRegisteredClientAsync();
            await reporter.PostAsJsonAsync(
                ReportCall.Route,
                new ReportCallRequest(number, ReportVerdictDto.Spam, null),
                ApiClientExtensions.JsonOptions,
                TestContext.Current.CancellationToken);
        }
    }

    private async Task FileSpamReviewAsync(string number, string reason, string? email)
    {
        using var owner = await EstablishedOwnerAsync();
        await SubmitSpamReviewAsync(owner, number, reason, email);
    }

    private async Task<HttpClient> EstablishedOwnerAsync()
    {
        var owner = await factory.CreateRegisteredClientAsync();
        factory.Clock.Advance(AppealRules.MinimumAccountAge + TimeSpan.FromDays(1));

        return owner;
    }

    private async Task SubmitSpamReviewAsync(HttpClient owner, string number, string reason, string? email)
    {
        var device = Guid.NewGuid().ToString("N")[..16];
        var phoneNumber = PhoneNumber.TryParse(number)!;
        var integrity = AppealApiFactory.TrustedToken(AppealNonce.Compute(AppealAction.Appeal, DeviceId.TryParse(device)!, phoneNumber));

        var response = await owner.PostAsJsonAsync(
            SubmitAppeal.Route,
            new SubmitAppealRequest(
                TestTokens.CreateForPhone(phoneNumber.E164, factory.Clock.GetUtcNow()), device, integrity, AppealKindDto.ReviewSpam, reason, email),
            ApiClientExtensions.JsonOptions,
            TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private async Task<LookupResponse> LookupAsync(string number)
    {
        using var client = factory.CreateClientFor(TestTokens.Create(Guid.NewGuid().ToString("N")));
        var response = await client.PostAsJsonAsync(LookupNumber.Route, new LookupRequest(number), TestContext.Current.CancellationToken);

        return (await response.Content.ReadFromJsonAsync<LookupResponse>(ApiClientExtensions.JsonOptions, TestContext.Current.CancellationToken))!;
    }
}
