using System.Text;
using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.PlayIntegrity.v1;
using Google.Apis.PlayIntegrity.v1.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tranqui.Application.Abstractions;

namespace Tranqui.Infrastructure.Integrity;

/// <summary>Decodes integrity tokens with Google's Play Integrity API and applies <see cref="PlayIntegrityVerdict"/>.</summary>
internal sealed partial class PlayIntegrityVerifier(
    IOptions<PlayIntegrityOptions> options,
    TimeProvider timeProvider,
    ILogger<PlayIntegrityVerifier> logger) : IDeviceIntegrityVerifier, IDisposable
{
    private readonly Lazy<PlayIntegrityService?> service = new(() => CreateService(options.Value));

    public async Task<bool> IsTrustedAsync(string integrityToken, string expectedNonce, CancellationToken cancellationToken)
    {
        if (service.Value is not { } playIntegrity)
        {
            LogNotConfigured();
            return false;
        }

        try
        {
            var response = await playIntegrity.V1
                .DecodeIntegrityToken(new DecodeIntegrityTokenRequest { IntegrityToken = integrityToken }, options.Value.PackageName)
                .ExecuteAsync(cancellationToken);

            var trusted = PlayIntegrityVerdict.IsTrusted(
                response.TokenPayloadExternal, options.Value.PackageName, expectedNonce, timeProvider.GetUtcNow());
            if (!trusted)
            {
                LogRejected(response.TokenPayloadExternal);
            }

            return trusted;
        }
        catch (GoogleApiException exception)
        {
            LogDecodeFailed(exception.HttpStatusCode.ToString());
            return false;
        }
    }

    public void Dispose()
    {
        if (service.IsValueCreated)
        {
            service.Value?.Dispose();
        }
    }

    private static PlayIntegrityService? CreateService(PlayIntegrityOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ServiceAccountKey))
        {
            return null;
        }

        var json = Encoding.UTF8.GetString(Convert.FromBase64String(options.ServiceAccountKey));
        var credential = CredentialFactory.FromJson<ServiceAccountCredential>(json)
            .ToGoogleCredential()
            .CreateScoped(PlayIntegrityService.Scope.Playintegrity);

        return new PlayIntegrityService(new BaseClientService.Initializer { HttpClientInitializer = credential });
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Play Integrity is not configured; rejecting the request")]
    private partial void LogNotConfigured();

    private void LogRejected(TokenPayloadExternal? payload)
    {
        if (!logger.IsEnabled(LogLevel.Information))
        {
            return;
        }

        var appVerdict = payload?.AppIntegrity?.AppRecognitionVerdict ?? "none";
        var deviceVerdict = string.Join(',', payload?.DeviceIntegrity?.DeviceRecognitionVerdict ?? []);
        LogRejected(appVerdict, deviceVerdict);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Play Integrity verdict rejected: app {AppVerdict}, device {DeviceVerdict}")]
    private partial void LogRejected(string appVerdict, string deviceVerdict);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Play Integrity token could not be decoded: {Status}")]
    private partial void LogDecodeFailed(string status);
}
