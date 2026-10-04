using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Tranqui.Api.Endpoints;
using Tranqui.Contracts.Errors;
using Tranqui.Contracts.Lookups;
using Tranqui.Domain.PhoneNumbers;
using Tranqui.Domain.Reputation;
using Tranqui.Infrastructure.Persistence;

namespace Tranqui.Api.Tests.Endpoints;

public sealed class LookupNumberTests(TranquiApiFactory factory) : IClassFixture<TranquiApiFactory>
{
    private static readonly JsonSerializerOptions jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    [Fact]
    public async Task Post_WithoutToken_ReturnsUnauthorized()
    {
        var response = await PostAsync(token: null, "3001112233");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Post_InvalidNumber_ReturnsValidationProblem()
    {
        var response = await PostAsync(NewToken(), "123");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.ProblemCodeAsync()).Should().Be(ApiErrorCodes.ValidationFailed);
    }

    [Fact]
    public async Task Post_NumberWithoutData_ReturnsUnknown()
    {
        var result = await LookupAsync("3001112233");

        result.Status.Should().Be(CallerStatusDto.Unknown);
        result.DisplayName.Should().BeNull();
    }

    [Fact]
    public async Task Post_NumberReportedAsSpam_ReturnsSpamWithItsLabel()
    {
        const string number = "+573002223344";
        await SeedAsync(number, (scope, phoneNumber, hash) =>
        {
            var protector = scope.GetRequiredService<INameProtector>();
            // Spread over two days, as real reports are: a single-day burst is dampened.
            return Enumerable.Range(0, 6).Select(index => (object)SpamReport.Create(
                hash,
                NewContributor(),
                ReportVerdict.Spam,
                1,
                index < 3 ? protector.Protect(phoneNumber, CallerName.TryCreate("Spam Claro")!) : null,
                DateTimeOffset.UtcNow.AddDays(-(index % 2))));
        });

        var result = await LookupAsync("300 222 3344");

        result.Status.Should().Be(CallerStatusDto.Spam);
        result.DisplayName.Should().Be("Spam Claro");
        result.SpamReportCount.Should().Be(6);
    }

    [Fact]
    public async Task Post_NumberSavedWithCommonNames_ReturnsIdentifiedWithRankedNames()
    {
        const string number = "+573003334455";
        await SeedAsync(number, (scope, phoneNumber, hash) =>
        {
            var protector = scope.GetRequiredService<INameProtector>();
            return Contributions("Pizzería Juan", 4).Concat(Contributions("Juan Domicilios", 3)).Concat(Contributions(null, 2))
                .Select(name => (object)ContactContribution.Create(
                    hash,
                    NewContributor(),
                    name is null ? null : protector.Protect(phoneNumber, CallerName.TryCreate(name)!),
                    DateTimeOffset.UtcNow));
        });

        var result = await LookupAsync(number);

        result.Status.Should().Be(CallerStatusDto.Identified);
        result.DisplayName.Should().Be("Pizzería Juan");
        result.OtherNames.Should().Equal("Juan Domicilios");
        result.SavedByCount.Should().Be(9);
    }

    [Fact]
    public async Task Post_OverTheHourlyLimit_ReturnsTooManyRequests()
    {
        var token = NewToken();
        for (var i = 0; i < TranquiApiFactory.LookupLimitPerHour; i++)
        {
            (await PostAsync(token, "3001112233")).StatusCode.Should().Be(HttpStatusCode.OK);
        }

        var response = await PostAsync(token, "3001112233");

        response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        (await response.ProblemCodeAsync()).Should().Be(ApiErrorCodes.RateLimited);
    }

    private static IEnumerable<string?> Contributions(string? name, int count) => Enumerable.Repeat(name, count);

    private static ContributorId NewContributor() => new(RandomNumberGenerator.GetBytes(ContributorId.SizeInBytes));

    private static string NewToken() => TestTokens.Create(Guid.NewGuid().ToString("N"));

    private async Task SeedAsync(
        string number,
        Func<IServiceProvider, PhoneNumber, PhoneHash, IEnumerable<object>> createEntities)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var phoneNumber = PhoneNumber.TryParse(number)!;
        var hash = scope.ServiceProvider.GetRequiredService<IPhoneNumberHasher>().Hash(phoneNumber);
        var dbContext = scope.ServiceProvider.GetRequiredService<TranquiDbContext>();

        dbContext.AddRange(createEntities(scope.ServiceProvider, phoneNumber, hash));
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private async Task<LookupResponse> LookupAsync(string number)
    {
        var response = await PostAsync(NewToken(), number);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        return (await response.Content.ReadFromJsonAsync<LookupResponse>(jsonOptions, TestContext.Current.CancellationToken))!;
    }

    private async Task<HttpResponseMessage> PostAsync(string? token, string number)
    {
        using var client = factory.CreateClient();
        if (token is not null)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await client.PostAsJsonAsync(LookupNumber.Route, new LookupRequest(number), TestContext.Current.CancellationToken);
    }
}
