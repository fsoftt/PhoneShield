using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tranqui.Api.Endpoints;
using Tranqui.Contracts.Accounts;
using Tranqui.Domain.Legal;

namespace Tranqui.Api.Tests;

public static class ApiClientExtensions
{
    public static JsonSerializerOptions JsonOptions { get; } = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    public static HttpClient CreateClientFor(this TranquiApiFactory factory, string? token)
    {
        var client = factory.CreateClient();
        if (token is not null)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return client;
    }

    public static async Task<HttpClient> CreateRegisteredClientAsync(this TranquiApiFactory factory)
    {
        var client = factory.CreateClientFor(TestTokens.Create(Guid.NewGuid().ToString("N")));
        var response = await client.PostAsJsonAsync(
            RegisterAccount.Route,
            new RegisterAccountRequest(LegalDocuments.CurrentTermsVersion),
            TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();

        return client;
    }
}
