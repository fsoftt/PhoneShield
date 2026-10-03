using System.Net;
using System.Net.Http.Json;
using Tranqui.Api.Endpoints;
using Tranqui.Contracts.Appeals;
using Tranqui.Contracts.Errors;
using Tranqui.Contracts.Lookups;
using Tranqui.Contracts.Reports;
using Tranqui.Domain.Reputation;

namespace Tranqui.Api.Tests.Endpoints;

public sealed class AppealTests(TranquiApiFactory factory) : IClassFixture<TranquiApiFactory>
{
    [Fact]
    public async Task RequestVerification_Anonymous_ReturnsTheNormalizedNumber()
    {
        using var client = factory.CreateClientFor(token: null);

        var response = await RequestVerificationAsync(client, "300 111 2201");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AppealVerificationResponse>(TestContext.Current.CancellationToken);
        body!.PhoneNumber.Should().Be("+573001112201");
    }

    [Theory]
    [InlineData(TranquiApiFactory.WebsiteOrigin, true)]
    [InlineData("https://elsewhere.example", false)]
    public async Task Preflight_OnlyTheWebsiteOriginIsAllowed(string origin, bool allowed)
    {
        using var client = factory.CreateClientFor(token: null);
        using var request = new HttpRequestMessage(HttpMethod.Options, SubmitAppeal.Route);
        request.Headers.Add("Origin", origin);
        request.Headers.Add("Access-Control-Request-Method", "POST");
        request.Headers.Add("Access-Control-Request-Headers", "authorization,content-type");

        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        response.Headers.Contains("Access-Control-Allow-Origin").Should().Be(allowed);
    }

    [Fact]
    public async Task RequestVerification_SecondTimeInTheMonth_IsRejected()
    {
        using var client = factory.CreateClientFor(token: null);
        await RequestVerificationAsync(client, "3001112202");

        var response = await RequestVerificationAsync(client, "+57 300 111 2202");

        response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        (await response.ProblemCodeAsync()).Should().Be(ApiErrorCodes.AppealLimitReached);
    }

    [Fact]
    public async Task RequestVerification_InvalidNumber_ReturnsValidationProblem()
    {
        using var client = factory.CreateClientFor(token: null);

        var response = await RequestVerificationAsync(client, "12");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.ProblemCodeAsync()).Should().Be(ApiErrorCodes.ValidationFailed);
    }

    [Fact]
    public async Task Submit_WithAppAccountToken_IsForbidden()
    {
        using var client = factory.CreateClientFor(TestTokens.Create(Guid.NewGuid().ToString("N")));

        var response = await SubmitAsync(client, new SubmitAppealRequest(AppealKindDto.HideNames, null, null));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AppEndpoints_WithPhoneToken_AreForbidden()
    {
        using var client = factory.CreateClientFor(TestTokens.CreateForPhone("+573001112203"));

        var response = await client.PostAsJsonAsync(LookupNumber.Route, new LookupRequest("3001112203"), TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Submit_ReviewSpam_IsPendingAndOnlyOncePerMonth()
    {
        using var client = factory.CreateClientFor(TestTokens.CreateForPhone("+573001112204"));
        var request = new SubmitAppealRequest(AppealKindDto.ReviewSpam, "It is my shop's number.", "owner@example.com");

        var first = await SubmitAsync(client, request);
        var second = await SubmitAsync(client, request);

        first.StatusCode.Should().Be(HttpStatusCode.OK);
        (await first.Content.ReadFromJsonAsync<AppealResponse>(ApiClientExtensions.JsonOptions, TestContext.Current.CancellationToken))!
            .Status.Should().Be(AppealStatusDto.Pending);
        second.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        (await second.ProblemCodeAsync()).Should().Be(ApiErrorCodes.AppealLimitReached);
    }

    [Fact]
    public async Task Submit_InvalidEmail_ReturnsValidationProblem()
    {
        using var client = factory.CreateClientFor(TestTokens.CreateForPhone("+573001112205"));

        var response = await SubmitAsync(client, new SubmitAppealRequest(AppealKindDto.ReviewSpam, null, "not-an-email"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Submit_HideNames_HidesTheNamesButKeepsTheSpamVerdict()
    {
        const string number = "3001112206";
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

        using var owner = factory.CreateClientFor(TestTokens.CreateForPhone("+57" + number));
        var response = await SubmitAsync(owner, new SubmitAppealRequest(AppealKindDto.HideNames, null, null));

        (await response.Content.ReadFromJsonAsync<AppealResponse>(ApiClientExtensions.JsonOptions, TestContext.Current.CancellationToken))!
            .Status.Should().Be(AppealStatusDto.Applied);
        var lookup = await LookupAsync(number);
        lookup.Status.Should().Be(CallerStatusDto.Spam);
        lookup.DisplayName.Should().BeNull();
        lookup.OtherNames.Should().BeEmpty();
    }

    private static Task<HttpResponseMessage> RequestVerificationAsync(HttpClient client, string phoneNumber) =>
        client.PostAsJsonAsync(
            RequestAppealVerification.Route,
            new AppealVerificationRequest(phoneNumber),
            TestContext.Current.CancellationToken);

    private static Task<HttpResponseMessage> SubmitAsync(HttpClient client, SubmitAppealRequest request) =>
        client.PostAsJsonAsync(SubmitAppeal.Route, request, ApiClientExtensions.JsonOptions, TestContext.Current.CancellationToken);

    private async Task<LookupResponse> LookupAsync(string number)
    {
        using var client = factory.CreateClientFor(TestTokens.Create(Guid.NewGuid().ToString("N")));
        var response = await client.PostAsJsonAsync(LookupNumber.Route, new LookupRequest(number), TestContext.Current.CancellationToken);

        return (await response.Content.ReadFromJsonAsync<LookupResponse>(ApiClientExtensions.JsonOptions, TestContext.Current.CancellationToken))!;
    }
}
