using Google.Apis.PlayIntegrity.v1.Data;
using Tranqui.Domain.Appeals;

namespace Tranqui.Infrastructure.Integrity;

/// <summary>What a decoded Play Integrity verdict must say for the server to trust the request.</summary>
internal static class PlayIntegrityVerdict
{
    public const string PlayRecognized = "PLAY_RECOGNIZED";
    public const string MeetsDeviceIntegrity = "MEETS_DEVICE_INTEGRITY";

    public static bool IsTrusted(TokenPayloadExternal? payload, string packageName, string expectedNonce, DateTimeOffset now)
    {
        var request = payload?.RequestDetails;
        if (request is null || request.RequestPackageName != packageName || request.Nonce != expectedNonce)
        {
            return false;
        }

        var issuedAt = DateTimeOffset.FromUnixTimeMilliseconds(request.TimestampMillis ?? 0);
        var age = now - issuedAt;
        if (age < TimeSpan.Zero - AppealRules.IntegrityVerdictMaxAge || age > AppealRules.IntegrityVerdictMaxAge)
        {
            return false;
        }

        return payload!.AppIntegrity?.AppRecognitionVerdict == PlayRecognized
            && payload.DeviceIntegrity?.DeviceRecognitionVerdict?.Contains(MeetsDeviceIntegrity) == true;
    }
}
