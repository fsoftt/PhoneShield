using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tranqui.Contracts.Appeals;
using Tranqui.Contracts.BackOffice;
using Tranqui.Contracts.Lookups;

namespace Tranqui.BackOffice.Tests;

/// <summary>Stands in for the API's /v1/admin endpoints: admin tokens get data, anything else a 403.</summary>
internal sealed class StubAdminApi : HttpMessageHandler
{
    public static readonly Guid PendingAppealId = Guid.Parse("0199a0b0-0000-7000-8000-000000000001");

    private static readonly JsonSerializerOptions json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    public List<string> ReceivedTokens { get; } = [];

    public List<(Guid Id, AppealDecisionDto Decision)> Resolutions { get; } = [];

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = request.Headers.Authorization?.Parameter ?? string.Empty;
        ReceivedTokens.Add(token);
        if (!token.StartsWith(FakeFirebase.AdminToken, StringComparison.Ordinal))
        {
            return new HttpResponseMessage(HttpStatusCode.Forbidden);
        }

        var path = request.RequestUri!.AbsolutePath;
        if (path == "/v1/admin/overview")
        {
            return Json(new BackOfficeOverviewResponse(1234, 56, 7890, 1, 0, 2, 3));
        }

        if (path == "/v1/admin/appeals")
        {
            return Json(new List<AppealReviewResponse>
            {
                new(PendingAppealId, AppealKindDto.ReviewSpam, AppealStatusDto.Pending, "Es mi tienda <b>no html</b>", "owner@example.com",
                    DateTimeOffset.UtcNow.AddDays(-2), DateTimeOffset.UtcNow.AddDays(19), null, false, CallerStatusDto.Spam, 12, 1, 3),
            });
        }

        if (path.EndsWith("/resolution", StringComparison.Ordinal))
        {
            var body = JsonSerializer.Deserialize<ResolveAppealRequest>(await request.Content!.ReadAsStringAsync(cancellationToken), json)!;
            Resolutions.Add((Guid.Parse(path.Split('/')[4]), body.Decision));
            return Json(new AppealResponse(AppealStatusDto.Approved));
        }

        return path == "/v1/admin/purges" ? Json(new PurgeResponse(4, 5, 6)) : new HttpResponseMessage(HttpStatusCode.NotFound);
    }

    private static HttpResponseMessage Json<T>(T body) =>
        new(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(body, json), Encoding.UTF8, "application/json") };
}
