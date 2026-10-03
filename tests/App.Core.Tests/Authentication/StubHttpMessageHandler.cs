using System.Net;
using System.Text;

namespace Tranqui.App.Core.Tests.Authentication;

/// <summary>Returns a canned JSON response and records the last request.</summary>
internal sealed class StubHttpMessageHandler(HttpStatusCode statusCode, string json) : HttpMessageHandler
{
    public HttpRequestMessage? LastRequest { get; private set; }

    public string? LastRequestBody { get; private set; }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequest = request;
        LastRequestBody = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);

        return new HttpResponseMessage(statusCode) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
    }
}
