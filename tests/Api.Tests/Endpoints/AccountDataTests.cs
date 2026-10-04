using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tranqui.Api.Endpoints;
using Tranqui.Contracts.Blocks;
using Tranqui.Contracts.Accounts;
using Tranqui.Contracts.Contacts;
using Tranqui.Contracts.Lookups;
using Tranqui.Contracts.Reports;
using Tranqui.Domain.Legal;
using Tranqui.Infrastructure.Persistence;

namespace Tranqui.Api.Tests.Endpoints;

public sealed class AccountDataTests(TranquiApiFactory factory) : IClassFixture<TranquiApiFactory>
{
    [Fact]
    public async Task Export_ListsConsentsAndCountsContributions()
    {
        using var client = await CreateActiveUserAsync("3021112233", "3022223344");

        var export = await client.GetFromJsonAsync<MyDataResponse>(ExportMyData.Route, TestContext.Current.CancellationToken);

        export!.Consents.Select(consent => consent.Type).Should().Equal("TermsAndPrivacyPolicy", "ContactUpload");
        export.SpamReportCount.Should().Be(1);
        export.ContactContributionCount.Should().Be(1);
    }

    [Fact]
    public async Task Delete_RemovesTheAccountAndEverythingItContributed()
    {
        const string reportedNumber = "3023334455";
        const string contactNumber = "3024445566";
        using var client = await CreateActiveUserAsync(reportedNumber, contactNumber);

        var response = await client.DeleteAsync(RegisterAccount.Route, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await client.GetAsync(ExportMyData.Route, TestContext.Current.CancellationToken)).StatusCode
            .Should().Be(HttpStatusCode.Conflict);
        (await LookupAsync(reportedNumber)).SpamReportCount.Should().Be(0);
        (await LookupAsync(contactNumber)).SavedByCount.Should().Be(0);
    }

    [Theory]
    [InlineData(null, 1)]
    [InlineData(false, 1)]
    [InlineData(true, 0)]
    public async Task Delete_KeepsSharedBlocksUnlessAskedToRemoveThem(bool? removeSharedBlocks, int blocksLeft)
    {
        using var client = await factory.CreateRegisteredClientAsync();
        (await client.PostAsJsonAsync(BlockNumber.Route, new BlockRequest("3025556677"), TestContext.Current.CancellationToken))
            .EnsureSuccessStatusCode();
        var before = await CountBlocksAsync();
        var route = removeSharedBlocks is null ? RegisterAccount.Route : $"{RegisterAccount.Route}?removeSharedBlocks={removeSharedBlocks.Value.ToString().ToLowerInvariant()}";

        (await client.DeleteAsync(route, TestContext.Current.CancellationToken)).StatusCode.Should().Be(HttpStatusCode.NoContent);

        (await CountBlocksAsync()).Should().Be(before - 1 + blocksLeft);
    }

    [Fact]
    public async Task Delete_WithoutAccount_IsIdempotent()
    {
        using var client = factory.CreateClientFor(TestTokens.Create(Guid.NewGuid().ToString("N")));

        var response = await client.DeleteAsync(RegisterAccount.Route, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private async Task<int> CountBlocksAsync()
    {
        using var scope = factory.Services.CreateScope();

        return await scope.ServiceProvider.GetRequiredService<TranquiDbContext>().BlockSignals.CountAsync(TestContext.Current.CancellationToken);
    }

    private async Task<HttpClient> CreateActiveUserAsync(string reportedNumber, string contactNumber)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = await factory.CreateRegisteredClientAsync();
        (await client.PostAsJsonAsync(
            AcceptContactUpload.Route,
            new AcceptContactUploadRequest(LegalDocuments.CurrentContactUploadVersion),
            cancellationToken)).EnsureSuccessStatusCode();
        (await client.PostAsJsonAsync(
            UploadContacts.Route,
            new UploadContactsRequest([new ContactDto(contactNumber, "Taller Pedro")]),
            cancellationToken)).EnsureSuccessStatusCode();
        (await client.PostAsJsonAsync(
            ReportCall.Route,
            new ReportCallRequest(reportedNumber, ReportVerdictDto.Spam, null),
            ApiClientExtensions.JsonOptions,
            cancellationToken)).EnsureSuccessStatusCode();

        return client;
    }

    private async Task<LookupResponse> LookupAsync(string number)
    {
        using var client = factory.CreateClientFor(TestTokens.Create(Guid.NewGuid().ToString("N")));
        var response = await client.PostAsJsonAsync(LookupNumber.Route, new LookupRequest(number), TestContext.Current.CancellationToken);

        return (await response.Content.ReadFromJsonAsync<LookupResponse>(ApiClientExtensions.JsonOptions, TestContext.Current.CancellationToken))!;
    }
}
