using Android.Gms.Extensions;
using Android.Provider;
using Tranqui.App.Core.Appeals;
using Xamarin.Google.Android.Play.Core.Integrity;

namespace Tranqui.App.Appeals;

/// <summary>Play Integrity (classic requests) and ANDROID_ID for appeal limits.</summary>
internal sealed class PlayDeviceIntegrity : IDeviceIntegrity
{
    private static global::Android.Content.Context appContext => global::Android.App.Application.Context;

    public string DeviceId => Settings.Secure.GetString(appContext.ContentResolver, Settings.Secure.AndroidId) ?? string.Empty;

    public async Task<string> RequestTokenAsync(string nonce, CancellationToken cancellationToken)
    {
        var builder = IntegrityTokenRequest.InvokeBuilder().SetNonce(nonce);
        if (AppSettings.CloudProjectNumber > 0)
        {
            builder.SetCloudProjectNumber(AppSettings.CloudProjectNumber);
        }

        var response = await IntegrityManagerFactory.Create(appContext)
            .RequestIntegrityToken(builder.Build())
            .AsAsync<IntegrityTokenResponse>()
            .WaitAsync(cancellationToken);

        return response.Token ?? throw new InvalidOperationException("Play Integrity returned no token.");
    }
}
