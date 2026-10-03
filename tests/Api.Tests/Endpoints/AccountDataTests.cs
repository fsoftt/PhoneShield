using System.Net;
using System.Net.Http.Json;
using Tranqui.Api.Endpoints;
using Tranqui.Contracts.Accounts;
using Tranqui.Contracts.Contacts;
using Tranqui.Contracts.Lookups;
using Tranqui.Contracts.Reports;
using Tranqui.Domain.Legal;

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

    [Fact]
    public async Task Delete_WithoutAccount_IsIdempotent()
    {
        using var client = factory.CreateClientFor(TestTokens.Create(Guid.NewGuid().ToString("N")));

        var response = await client.DeleteAsync(RegisterAccount.Route, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
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
