namespace Tranqui.App.Core.Appeals;

/// <summary>The device's identity for appeal limits and its Play Integrity attestation. Provided by the platform.</summary>
public interface IDeviceIntegrity
{
    /// <summary>Android's ANDROID_ID: per device and app signing key; only a factory reset changes it.</summary>
    string DeviceId { get; }

    /// <summary>A Play Integrity token bound to <paramref name="nonce"/> (URL-safe Base64).</summary>
    Task<string> RequestTokenAsync(string nonce, CancellationToken cancellationToken);
}
