using System.Net;
using System.Net.Http.Json;
using Tranqui.Api.Endpoints;
using Tranqui.Contracts.Accounts;
using Tranqui.Contracts.Contacts;
using Tranqui.Contracts.Lookups;
using Tranqui.Domain.Legal;
using Tranqui.Domain.Reputation;

namespace Tranqui.Api.Tests.Endpoints;

public sealed class ContactUploadTests(TranquiApiFactory factory) : IClassFixture<TranquiApiFactory>
{
    [Fact]
    public async Task Upload_WithoutConsent_ReturnsForbidden()
    {
        using var client = await factory.CreateRegisteredClientAsync();

        var response = await UploadAsync(client, new ContactDto("3011112233", "Pizzería Juan"));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Upload_TooManyContactsInOneBatch_ReturnsValidationProblem()
    {
        using var client = await CreateContributorAsync();
        var contacts = Enumerable.Range(0, ContactUploadRules.MaxContactsPerBatch + 1)
            .Select(index => new ContactDto($"301{index:D7}", null))
            .ToArray();

        var response = await UploadAsync(client, contacts);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Upload_SameNameByThreePeople_MakesTheNumberIdentified()
    {
        const string number = "3012223344";

        for (var i = 0; i < ReputationRules.MinimumDistinctContributorsPerName; i++)
        {
            using var client = await CreateContributorAsync();
            var response = await UploadAsync(client, new ContactDto(number, "Pizzería Juan"));
            (await response.Content.ReadFromJsonAsync<UploadContactsResponse>(TestContext.Current.CancellationToken))
                .Should().Be(new UploadContactsResponse(1, 0));
        }

        var lookup = await LookupAsync(number);

        lookup.Status.Should().Be(CallerStatusDto.Identified);
        lookup.DisplayName.Should().Be("Pizzería Juan");
    }

    [Fact]
    public async Task Upload_PersonalName_CountsAsSavedButIsNeverShown()
    {
        const string number = "3013334455";

        for (var i = 0; i < ReputationRules.MinimumDistinctContributorsPerName; i++)
        {
            using var client = await CreateContributorAsync();
            await UploadAsync(client, new ContactDto(number, "Mamá"));
        }

        var lookup = await LookupAsync(number);

        lookup.Status.Should().Be(CallerStatusDto.Unknown);
        lookup.DisplayName.Should().BeNull();
        lookup.SavedByCount.Should().Be(ReputationRules.MinimumDistinctContributorsPerName);
    }

    [Fact]
    public async Task Withdraw_RemovesEverythingTheUserContributed()
    {
        const string number = "3014445566";
        using var client = await CreateContributorAsync();
        await UploadAsync(client, new ContactDto(number, "Taller Pedro"), new ContactDto("3014445567", null));

        var response = await client.DeleteAsync(UploadContacts.Route, TestContext.Current.CancellationToken);

        (await response.Content.ReadFromJsonAsync<WithdrawContactsResponse>(TestContext.Current.CancellationToken))
            .Should().Be(new WithdrawContactsResponse(2));
        (await LookupAsync(number)).SavedByCount.Should().Be(0);
        (await UploadAsync(client, new ContactDto(number, "Taller Pedro"))).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private async Task<HttpClient> CreateContributorAsync()
    {
        var client = await factory.CreateRegisteredClientAsync();
        var response = await client.PostAsJsonAsync(
            AcceptContactUpload.Route,
            new AcceptContactUploadRequest(LegalDocuments.CurrentContactUploadVersion),
            TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        return client;
    }

    private static Task<HttpResponseMessage> UploadAsync(HttpClient client, params ContactDto[] contacts) =>
        client.PostAsJsonAsync(UploadContacts.Route, new UploadContactsRequest(contacts), TestContext.Current.CancellationToken);

    private async Task<LookupResponse> LookupAsync(string number)
    {
        using var client = factory.CreateClientFor(TestTokens.Create(Guid.NewGuid().ToString("N")));
        var response = await client.PostAsJsonAsync(LookupNumber.Route, new LookupRequest(number), TestContext.Current.CancellationToken);

        return (await response.Content.ReadFromJsonAsync<LookupResponse>(ApiClientExtensions.JsonOptions, TestContext.Current.CancellationToken))!;
    }
}
