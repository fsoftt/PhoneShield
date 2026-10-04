using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Tranqui.BackOffice.Api;
using Tranqui.BackOffice.Authentication;

namespace Tranqui.BackOffice.Tests;

/// <summary>The back office with fake Firebase and API, served over HTTPS so the secure cookies flow.</summary>
public sealed class BackOfficeFactory : WebApplicationFactory<Program>
{
    internal FakeFirebase Firebase { get; } = new();

    internal StubAdminApi Api { get; } = new();

    public HttpClient CreateBrowser() => CreateClient(new WebApplicationFactoryClientOptions
    {
        BaseAddress = new Uri("https://localhost"),
        AllowAutoRedirect = false,
    });

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("BackOffice:ApiBaseUrl", "http://api.test/");
        builder.UseSetting("BackOffice:FirebaseApiKey", "public-web-key");
        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<IFirebasePasswordAuth>(Firebase);
            services.AddHttpClient<AdminApiClient>().ConfigurePrimaryHttpMessageHandler(() => Api);
        });
    }
}
