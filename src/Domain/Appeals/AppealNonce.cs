using System.Security.Cryptography;
using System.Text;
using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.Domain.Appeals;

/// <summary>
/// The Play Integrity nonce for an appeal step. The app requests its integrity token with this nonce and the server
/// recomputes it, so a token only vouches for this action, device and number.
/// </summary>
public static class AppealNonce
{
    private const string Purpose = "tranqui-appeal";

    public static string Compute(AppealAction action, DeviceId deviceId, PhoneNumber phoneNumber)
    {
        ArgumentNullException.ThrowIfNull(deviceId);
        ArgumentNullException.ThrowIfNull(phoneNumber);

        var input = string.Join('\n', Purpose, (int)action, deviceId.Value, phoneNumber.E164);

        return ToUrlSafeBase64(SHA256.HashData(Encoding.UTF8.GetBytes(input)));
    }

    /// <summary>Play Integrity expects URL-safe Base64 without line wraps (Android's URL_SAFE | NO_WRAP).</summary>
    public static string ToUrlSafeBase64(byte[] bytes) =>
        Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_');
}
