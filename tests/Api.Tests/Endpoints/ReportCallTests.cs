using System.Net;
using System.Net.Http.Json;
using Tranqui.Api.Endpoints;
using Tranqui.Contracts.Lookups;
using Tranqui.Contracts.Reports;
using Tranqui.Domain.Reputation;

namespace Tranqui.Api.Tests.Endpoints;

public sealed class ReportCallTests(TranquiApiFactory factory) : IClassFixture<TranquiApiFactory>
{
    [Fact]
    public async Task Post_WithoutToken_ReturnsUnauthorized()
    {
        using var client = factory.CreateClientFor(token: null);

        var response = await ReportAsync(client, new ReportCallRequest("3005556677", ReportVerdictDto.Spam, null));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Post_FromUnregisteredAccount_ReturnsConflict()
    {
        using var client = factory.CreateClientFor(TestTokens.Create(Guid.NewGuid().ToString("N")));

        var response = await ReportAsync(client, new ReportCallRequest("3005556677", ReportVerdictDto.Spam, null));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Post_NotSpamWithLabel_ReturnsValidationProblem()
    {
        using var client = await factory.CreateRegisteredClientAsync();

        var response = await ReportAsync(client, new ReportCallRequest("3005556677", ReportVerdictDto.NotSpam, "Pizzería"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_EnoughNewAccountsReportSpam_LookupShowsSpamWithTheLabel()
    {
        const string number = "3006667788";
        var reportersNeeded = (int)Math.Ceiling(ReputationRules.MinimumSpamWeight / ReputationRules.NewAccountVoteWeight);

        for (var i = 0; i < reportersNeeded; i++)
        {
            using var reporter = await factory.CreateRegisteredClientAsync();
            var response = await ReportAsync(reporter, new ReportCallRequest(number, ReportVerdictDto.Spam, "Spam Claro"));
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        var lookup = await LookupAsync(number);

        lookup.Status.Should().Be(CallerStatusDto.Spam);
        lookup.DisplayName.Should().Be("Spam Claro");
        lookup.SpamReportCount.Should().Be(reportersNeeded);
    }

    [Fact]
    public async Task Post_SameUserTwice_CountsOnce()
    {
        const string number = "3007778899";
        using var reporter = await factory.CreateRegisteredClientAsync();

        await ReportAsync(reporter, new ReportCallRequest(number, ReportVerdictDto.Spam, null));
        await ReportAsync(reporter, new ReportCallRequest(number, ReportVerdictDto.Spam, null));

        (await LookupAsync(number)).SpamReportCount.Should().Be(1);
    }

    private static Task<HttpResponseMessage> ReportAsync(HttpClient client, ReportCallRequest request) =>
        client.PostAsJsonAsync(ReportCall.Route, request, ApiClientExtensions.JsonOptions, TestContext.Current.CancellationToken);

    private async Task<LookupResponse> LookupAsync(string number)
    {
        using var client = factory.CreateClientFor(TestTokens.Create(Guid.NewGuid().ToString("N")));
        var response = await client.PostAsJsonAsync(LookupNumber.Route, new LookupRequest(number), TestContext.Current.CancellationToken);

        return (await response.Content.ReadFromJsonAsync<LookupResponse>(ApiClientExtensions.JsonOptions, TestContext.Current.CancellationToken))!;
    }
}
