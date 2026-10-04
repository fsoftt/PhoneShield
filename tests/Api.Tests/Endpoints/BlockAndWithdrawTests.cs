using System.Net;
using System.Net.Http.Json;
using Tranqui.Api.Endpoints;
using Tranqui.Contracts.Accounts;
using Tranqui.Contracts.Blocks;
using Tranqui.Contracts.Lookups;
using Tranqui.Contracts.Reports;

namespace Tranqui.Api.Tests.Endpoints;

public sealed class BlockAndWithdrawTests(TranquiApiFactory factory) : IClassFixture<TranquiApiFactory>
{
    [Fact]
    public async Task Block_IsIdempotentAndShowsUpInMyData()
    {
        using var client = await factory.CreateRegisteredClientAsync();

        (await BlockAsync(client, "3004445501")).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await BlockAsync(client, "+57 300 444 5501")).StatusCode.Should().Be(HttpStatusCode.NoContent);

        (await MyDataAsync(client)).BlockCount.Should().Be(1);
    }

    [Fact]
    public async Task Unblock_RemovesTheSignal()
    {
        using var client = await factory.CreateRegisteredClientAsync();
        await BlockAsync(client, "3004445502");

        var response = await SendDeleteAsync(client, BlockNumber.Route, new BlockRequest("3004445502"));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await MyDataAsync(client)).BlockCount.Should().Be(0);
    }

    [Fact]
    public async Task Block_FromUnregisteredAccount_IsAConflict()
    {
        using var client = factory.CreateClientFor(TestTokens.Create(Guid.NewGuid().ToString("N")));

        (await BlockAsync(client, "3004445503")).StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task WithdrawReport_RemovesMyVote()
    {
        using var client = await factory.CreateRegisteredClientAsync();
        await client.PostAsJsonAsync(
            ReportCall.Route, new ReportCallRequest("3004445504", ReportVerdictDto.Spam, null), ApiClientExtensions.JsonOptions,
            TestContext.Current.CancellationToken);

        var response = await SendDeleteAsync(client, ReportCall.Route, new WithdrawReportRequest("3004445504"));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await LookupAsync("3004445504")).SpamReportCount.Should().Be(0);
    }

    [Fact]
    public async Task Report_OffensiveLabel_KeepsTheVoteButNeverShowsTheLabel()
    {
        const string number = "3004445505";
        for (var i = 0; i < ApiClientExtensions.SameDaySpamReportersNeeded; i++)
        {
            using var reporter = await factory.CreateRegisteredClientAsync();
            await reporter.PostAsJsonAsync(
                ReportCall.Route, new ReportCallRequest(number, ReportVerdictDto.Spam, "Spam HP"), ApiClientExtensions.JsonOptions,
                TestContext.Current.CancellationToken);
        }

        var lookup = await LookupAsync(number);

        lookup.Status.Should().Be(CallerStatusDto.Spam);
        lookup.DisplayName.Should().BeNull();
    }

    private static Task<HttpResponseMessage> BlockAsync(HttpClient client, string number) =>
        client.PostAsJsonAsync(BlockNumber.Route, new BlockRequest(number), TestContext.Current.CancellationToken);

    private static Task<HttpResponseMessage> SendDeleteAsync<T>(HttpClient client, string route, T body)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, route) { Content = JsonContent.Create(body) };

        return client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    private static async Task<MyDataResponse> MyDataAsync(HttpClient client) =>
        (await client.GetFromJsonAsync<MyDataResponse>(ExportMyData.Route, TestContext.Current.CancellationToken))!;

    private async Task<LookupResponse> LookupAsync(string number)
    {
        using var client = factory.CreateClientFor(TestTokens.Create(Guid.NewGuid().ToString("N")));
        var response = await client.PostAsJsonAsync(LookupNumber.Route, new LookupRequest(number), TestContext.Current.CancellationToken);

        return (await response.Content.ReadFromJsonAsync<LookupResponse>(ApiClientExtensions.JsonOptions, TestContext.Current.CancellationToken))!;
    }
}
