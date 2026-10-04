using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tranqui.Contracts.Appeals;
using Tranqui.Contracts.BackOffice;

namespace Tranqui.BackOffice.Api;

/// <summary>Typed client for the API's /v1/admin endpoints. The API decides who is an admin.</summary>
public sealed class AdminApiClient(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    public Task<BackOfficeOverviewResponse> GetOverviewAsync(CancellationToken cancellationToken) =>
        GetAsync<BackOfficeOverviewResponse>("v1/admin/overview", accessToken: null, cancellationToken);

    /// <summary>Used at sign-in, before the session cookie exists, to check the account is an admin.</summary>
    public Task<BackOfficeOverviewResponse> GetOverviewAsync(string accessToken, CancellationToken cancellationToken) =>
        GetAsync<BackOfficeOverviewResponse>("v1/admin/overview", accessToken, cancellationToken);

    public Task<List<AppealReviewResponse>> ListAppealsAsync(AppealStatusDto status, CancellationToken cancellationToken) =>
        GetAsync<List<AppealReviewResponse>>($"v1/admin/appeals?status={status}", accessToken: null, cancellationToken);

    public async Task ResolveAsync(Guid appealId, AppealDecisionDto decision, CancellationToken cancellationToken)
    {
        using var response = await httpClient.PostAsJsonAsync(
            new Uri($"v1/admin/appeals/{appealId}/resolution", UriKind.Relative),
            new ResolveAppealRequest(decision),
            jsonOptions,
            cancellationToken);
        EnsureSuccess(response);
    }

    public async Task<PurgeResponse> PurgeAsync(CancellationToken cancellationToken)
    {
        using var response = await httpClient.PostAsync(new Uri("v1/admin/purges", UriKind.Relative), content: null, cancellationToken);
        EnsureSuccess(response);

        return (await response.Content.ReadFromJsonAsync<PurgeResponse>(jsonOptions, cancellationToken))!;
    }

    private async Task<T> GetAsync<T>(string path, string? accessToken, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(path, UriKind.Relative));
        if (accessToken is not null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        using var response = await httpClient.SendAsync(request, cancellationToken);
        EnsureSuccess(response);

        return (await response.Content.ReadFromJsonAsync<T>(jsonOptions, cancellationToken))!;
    }

    private static void EnsureSuccess(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            throw new AdminApiException(response.StatusCode);
        }
    }
}
