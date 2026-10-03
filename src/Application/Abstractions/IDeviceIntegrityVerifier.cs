namespace Tranqui.Application.Abstractions;

/// <summary>
/// Checks a Play Integrity token: the genuine Tranqui app from Google Play, on a genuine device, recently, and for
/// exactly the expected nonce.
/// </summary>
public interface IDeviceIntegrityVerifier
{
    Task<bool> IsTrustedAsync(string integrityToken, string expectedNonce, CancellationToken cancellationToken);
}
