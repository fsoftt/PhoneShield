using System.Net;
using System.Net.Http.Json;
using Tranqui.Api.Endpoints;
using Tranqui.Contracts.Appeals;
using Tranqui.Contracts.Errors;
using Tranqui.Contracts.Lookups;
using Tranqui.Contracts.Reports;
using Tranqui.Domain.Appeals;
using Tranqui.Domain.PhoneNumbers;
using Tranqui.Domain.Reputation;

namespace Tranqui.Api.Tests.Endpoints;

public sealed class AppealTests(AppealApiFactory factory) : IClassFixture<AppealApiFactory>
{
    private static readonly TimeSpan pastMinimumAge = AppealRules.MinimumAccountAge + TimeSpan.FromDays(1);

    [Fact]
    public async Task RequestVerification_WithoutToken_ReturnsUnauthorized()
    {
        using var client = factory.CreateClientFor(token: null);

        var response = await RequestVerificationAsync(client, "3001112200", NewDevice());

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RequestVerification_EstablishedAccount_ReturnsTheNormalizedNumber()
    {
        using var client = await EstablishedAccountAsync();

        var response = await RequestVerificationAsync(client, "300 111 2201", NewDevice());

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AppealVerificationResponse>(TestContext.Current.CancellationToken);
        body!.PhoneNumber.Should().Be("+573001112201");
    }

    [Fact]
    public async Task RequestVerification_NewAccount_IsForbidden()
    {
        using var client = await factory.CreateRegisteredClientAsync();

        var response = await RequestVerificationAsync(client, "3001112202", NewDevice());

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await response.ProblemCodeAsync()).Should().Be(ApiErrorCodes.AppealAccountTooNew);
    }

    [Fact]
    public async Task RequestVerification_UntrustedDevice_IsForbidden()
    {
        using var client = await EstablishedAccountAsync();

        var response = await client.PostAsJsonAsync(
            RequestAppealVerification.Route,
            new AppealVerificationRequest("3001112203", NewDevice(), "forged-token"),
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await response.ProblemCodeAsync()).Should().Be(ApiErrorCodes.DeviceNotTrusted);
    }

    [Fact]
    public async Task RequestVerification_SameNumberFromAnotherAccountAndDevice_IsLimited()
    {
        using var first = await EstablishedAccountAsync();
        using var second = await EstablishedAccountAsync();
        await RequestVerificationAsync(first, "3001112204", NewDevice());

        var response = await RequestVerificationAsync(second, "3001112204", NewDevice());

        await ShouldBeLimitedAsync(response);
    }

    [Fact]
    public async Task RequestVerification_SameAccountWithAnotherSimAndDevice_IsLimited()
    {
        using var client = await EstablishedAccountAsync();
        await RequestVerificationAsync(client, "3001112205", NewDevice());

        var response = await RequestVerificationAsync(client, "3001112206", NewDevice());

        await ShouldBeLimitedAsync(response);
    }

    [Fact]
    public async Task RequestVerification_SameDeviceWithAnotherSimAndAccount_IsLimited()
    {
        var device = NewDevice();
        using var first = await EstablishedAccountAsync();
        using var second = await EstablishedAccountAsync();
        await RequestVerificationAsync(first, "3001112207", device);

        var response = await RequestVerificationAsync(second, "3001112208", device);

        await ShouldBeLimitedAsync(response);
    }

    [Fact]
    public async Task RequestVerification_SameAccountMonthAfterMonth_StopsAfterThreeInAYear()
    {
        using var client = await EstablishedAccountAsync();
        for (var month = 0; month < AppealRules.AccountOrDeviceUsesPerYear; month++)
        {
            var allowed = await RequestVerificationAsync(client, $"30011123{month:D2}", NewDevice());
            allowed.StatusCode.Should().Be(HttpStatusCode.OK);
            factory.Clock.Advance(AppealRules.MonthWindow + TimeSpan.FromDays(1));
        }

        var response = await RequestVerificationAsync(client, "3001112399", NewDevice());

        await ShouldBeLimitedAsync(response);
    }

    [Fact]
    public async Task Submit_WithoutPhoneProof_ReturnsPhoneNotVerified()
    {
        using var client = await EstablishedAccountAsync();
        var appEmailToken = TestTokens.Create(Guid.NewGuid().ToString("N"));

        var response = await SubmitAsync(client, "3001112210", AppealKindDto.ReviewSpam, appEmailToken, NewDevice());

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.ProblemCodeAsync()).Should().Be(ApiErrorCodes.PhoneNotVerified);
    }

    [Fact]
    public async Task Submit_StalePhoneProof_ReturnsPhoneNotVerified()
    {
        using var client = await EstablishedAccountAsync();
        var staleProof = TestTokens.CreateForPhone(
            "+573001112211", factory.Clock.GetUtcNow() - AppealRules.PhoneProofMaxAge - TimeSpan.FromMinutes(1));

        var response = await SubmitAsync(client, "3001112211", AppealKindDto.ReviewSpam, staleProof, NewDevice());

        (await response.ProblemCodeAsync()).Should().Be(ApiErrorCodes.PhoneNotVerified);
    }

    [Fact]
    public async Task Submit_ReviewSpam_IsPendingAndOnlyOncePerMonth()
    {
        using var client = await EstablishedAccountAsync();
        using var other = await EstablishedAccountAsync();

        var first = await SubmitAsync(client, "3001112212", AppealKindDto.ReviewSpam);
        var second = await SubmitAsync(other, "3001112212", AppealKindDto.ReviewSpam);

        first.StatusCode.Should().Be(HttpStatusCode.OK);
        (await first.Content.ReadFromJsonAsync<AppealResponse>(ApiClientExtensions.JsonOptions, TestContext.Current.CancellationToken))!
            .Status.Should().Be(AppealStatusDto.Pending);
        await ShouldBeLimitedAsync(second);
    }

    [Fact]
    public async Task Submit_HideNames_HidesTheNamesButKeepsTheSpamVerdict()
    {
        const string number = "3001112213";
        using var owner = await EstablishedAccountAsync();
        var reportersNeeded = (int)Math.Ceiling(ReputationRules.MinimumSpamWeight / ReputationRules.NewAccountVoteWeight);
        for (var i = 0; i < reportersNeeded; i++)
        {
            using var reporter = await factory.CreateRegisteredClientAsync();
            await reporter.PostAsJsonAsync(
                ReportCall.Route,
                new ReportCallRequest(number, ReportVerdictDto.Spam, "Spam Claro"),
                ApiClientExtensions.JsonOptions,
                TestContext.Current.CancellationToken);
        }

        var response = await SubmitAsync(owner, number, AppealKindDto.HideNames);

        (await response.Content.ReadFromJsonAsync<AppealResponse>(ApiClientExtensions.JsonOptions, TestContext.Current.CancellationToken))!
            .Status.Should().Be(AppealStatusDto.Applied);
        var lookup = await LookupAsync(number);
        lookup.Status.Should().Be(CallerStatusDto.Spam);
        lookup.DisplayName.Should().BeNull();
        lookup.OtherNames.Should().BeEmpty();
    }

    [Fact]
    public async Task AppEndpoints_WithPhoneProofAsBearer_AreForbidden()
    {
        using var client = factory.CreateClientFor(TestTokens.CreateForPhone("+573001112214", factory.Clock.GetUtcNow()));

        var response = await client.PostAsJsonAsync(LookupNumber.Route, new LookupRequest("3001112214"), TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private static string NewDevice() => Guid.NewGuid().ToString("N")[..16];

    private static string TrustedToken(AppealAction action, string device, string number) =>
        AppealApiFactory.TrustedToken(AppealNonce.Compute(action, DeviceId.TryParse(device)!, PhoneNumber.TryParse(number)!));

    private async Task<HttpClient> EstablishedAccountAsync()
    {
        var client = await factory.CreateRegisteredClientAsync();
        factory.Clock.Advance(pastMinimumAge);

        return client;
    }

    private static Task<HttpResponseMessage> RequestVerificationAsync(HttpClient client, string number, string device) =>
        client.PostAsJsonAsync(
            RequestAppealVerification.Route,
            new AppealVerificationRequest(number, device, TrustedToken(AppealAction.SmsVerification, device, number)),
            TestContext.Current.CancellationToken);

    private Task<HttpResponseMessage> SubmitAsync(HttpClient client, string number, AppealKindDto kind) =>
        SubmitAsync(
            client,
            number,
            kind,
            TestTokens.CreateForPhone(PhoneNumber.TryParse(number)!.E164, factory.Clock.GetUtcNow()),
            NewDevice());

    private static Task<HttpResponseMessage> SubmitAsync(
        HttpClient client, string number, AppealKindDto kind, string phoneProof, string device) =>
        client.PostAsJsonAsync(
            SubmitAppeal.Route,
            new SubmitAppealRequest(phoneProof, device, TrustedToken(AppealAction.Appeal, device, number), kind, null, null),
            ApiClientExtensions.JsonOptions,
            TestContext.Current.CancellationToken);

    private static async Task ShouldBeLimitedAsync(HttpResponseMessage response)
    {
        response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        (await response.ProblemCodeAsync()).Should().Be(ApiErrorCodes.AppealLimitReached);
    }

    private async Task<LookupResponse> LookupAsync(string number)
    {
        using var client = factory.CreateClientFor(TestTokens.Create(Guid.NewGuid().ToString("N")));
        var response = await client.PostAsJsonAsync(LookupNumber.Route, new LookupRequest(number), TestContext.Current.CancellationToken);

        return (await response.Content.ReadFromJsonAsync<LookupResponse>(ApiClientExtensions.JsonOptions, TestContext.Current.CancellationToken))!;
    }
}
