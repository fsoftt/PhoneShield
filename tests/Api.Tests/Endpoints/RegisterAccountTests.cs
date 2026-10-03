using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Tranqui.Api.Endpoints;
using Tranqui.Contracts.Accounts;
using Tranqui.Domain.Legal;

namespace Tranqui.Api.Tests.Endpoints;

public sealed class RegisterAccountTests(TranquiApiFactory factory) : IClassFixture<TranquiApiFactory>
{
    private static readonly RegisterAccountRequest validRequest = new(LegalDocuments.CurrentTermsVersion);

    [Fact]
    public async Task Post_WithoutToken_ReturnsUnauthorized()
    {
        var response = await PostAsync(token: null, validRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Post_WithExpiredToken_ReturnsUnauthorized()
    {
        var token = TestTokens.Create(NewUid(), expires: DateTime.UtcNow.AddMinutes(-10));

        var response = await PostAsync(token, validRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Post_WithTokenForAnotherProject_ReturnsUnauthorized()
    {
        var token = TestTokens.Create(NewUid(), audience: "another-project");

        var response = await PostAsync(token, validRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Post_WithUnverifiedEmail_ReturnsForbidden()
    {
        var token = TestTokens.Create(NewUid(), emailVerified: false);

        var response = await PostAsync(token, validRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Post_WithOutdatedTerms_ReturnsValidationProblem()
    {
        var response = await PostAsync(TestTokens.Create(NewUid()), new RegisterAccountRequest("2000-01-01"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestContext.Current.CancellationToken);
        problem!.Errors.Should().ContainKey("AcceptedTermsVersion");
    }

    [Fact]
    public async Task Post_ValidRequest_CreatesTheAccount()
    {
        var response = await PostAsync(TestTokens.Create(NewUid()), validRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var account = await response.Content.ReadFromJsonAsync<AccountResponse>(TestContext.Current.CancellationToken);
        account!.Id.Should().NotBe(Guid.Empty);
        account.AcceptedTermsVersion.Should().Be(LegalDocuments.CurrentTermsVersion);
    }

    [Fact]
    public async Task Post_Twice_ReturnsTheSameAccount()
    {
        var token = TestTokens.Create(NewUid());

        var first = await ReadAccountAsync(await PostAsync(token, validRequest));
        var second = await ReadAccountAsync(await PostAsync(token, validRequest));

        second.Id.Should().Be(first.Id);
    }

    private static string NewUid() => Guid.NewGuid().ToString("N");

    private static async Task<AccountResponse> ReadAccountAsync(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<AccountResponse>(TestContext.Current.CancellationToken))!;

    private async Task<HttpResponseMessage> PostAsync(string? token, RegisterAccountRequest request)
    {
        using var client = factory.CreateClient();
        if (token is not null)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await client.PostAsJsonAsync(RegisterAccount.Route, request, TestContext.Current.CancellationToken);
    }
}
