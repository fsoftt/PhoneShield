using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Tranqui.Application.Abstractions;

namespace Tranqui.Api.Tests;

/// <summary>
/// The API with a controllable clock (accounts must be a week old to appeal) and a stand-in for Google's Play
/// Integrity service that trusts exactly the tokens made by <see cref="TrustedToken"/>.
/// </summary>
public sealed class AppealApiFactory : TranquiApiFactory
{
    private const string TrustedPrefix = "trusted:";

    public FakeTimeProvider Clock { get; } = new(DateTimeOffset.UtcNow);

    public static string TrustedToken(string nonce) => TrustedPrefix + nonce;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<TimeProvider>(Clock);
            services.AddSingleton<IDeviceIntegrityVerifier, FakeIntegrityVerifier>();
        });
    }

    private sealed class FakeIntegrityVerifier : IDeviceIntegrityVerifier
    {
        public Task<bool> IsTrustedAsync(string integrityToken, string expectedNonce, CancellationToken cancellationToken) =>
            Task.FromResult(integrityToken == TrustedToken(expectedNonce));
    }
}
