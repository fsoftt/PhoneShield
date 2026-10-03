using System.Net;

namespace Tranqui.Api.Tests;

public sealed class HealthEndpointTests(TranquiApiFactory factory) : IClassFixture<TranquiApiFactory>
{
    [Fact]
    public async Task Get_Health_ReturnsOk()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
