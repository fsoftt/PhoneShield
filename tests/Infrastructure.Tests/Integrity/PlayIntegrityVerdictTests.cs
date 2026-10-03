using Google.Apis.PlayIntegrity.v1.Data;
using Tranqui.Domain.Appeals;
using Tranqui.Infrastructure.Integrity;

namespace Tranqui.Infrastructure.Tests.Integrity;

public sealed class PlayIntegrityVerdictTests
{
    private const string Package = "com.fsoftt.tranqui";
    private const string Nonce = "expected-nonce";

    private static readonly DateTimeOffset now = new(2026, 10, 3, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void IsTrusted_GenuineAppOnGenuineDevice_IsTrusted()
    {
        PlayIntegrityVerdict.IsTrusted(Payload(), Package, Nonce, now).Should().BeTrue();
    }

    [Fact]
    public void IsTrusted_Null_IsNotTrusted()
    {
        PlayIntegrityVerdict.IsTrusted(null, Package, Nonce, now).Should().BeFalse();
    }

    [Fact]
    public void IsTrusted_OtherNonce_IsNotTrusted()
    {
        PlayIntegrityVerdict.IsTrusted(Payload(), Package, "another-nonce", now).Should().BeFalse();
    }

    [Fact]
    public void IsTrusted_OtherPackage_IsNotTrusted()
    {
        PlayIntegrityVerdict.IsTrusted(Payload(), "com.example.other", Nonce, now).Should().BeFalse();
    }

    [Fact]
    public void IsTrusted_StaleVerdict_IsNotTrusted()
    {
        var payload = Payload(issuedAt: now - AppealRules.IntegrityVerdictMaxAge - TimeSpan.FromSeconds(1));

        PlayIntegrityVerdict.IsTrusted(payload, Package, Nonce, now).Should().BeFalse();
    }

    [Fact]
    public void IsTrusted_SideloadedOrModifiedApp_IsNotTrusted()
    {
        PlayIntegrityVerdict.IsTrusted(Payload(appVerdict: "UNRECOGNIZED_VERSION"), Package, Nonce, now).Should().BeFalse();
    }

    [Fact]
    public void IsTrusted_EmulatorOrRootedDevice_IsNotTrusted()
    {
        PlayIntegrityVerdict.IsTrusted(Payload(deviceVerdicts: []), Package, Nonce, now).Should().BeFalse();
    }

    private static TokenPayloadExternal Payload(
        string appVerdict = PlayIntegrityVerdict.PlayRecognized,
        string[]? deviceVerdicts = null,
        DateTimeOffset? issuedAt = null) => new()
    {
        RequestDetails = new RequestDetails
        {
            RequestPackageName = Package,
            Nonce = Nonce,
            TimestampMillis = (issuedAt ?? now.AddSeconds(-10)).ToUnixTimeMilliseconds(),
        },
        AppIntegrity = new AppIntegrity { AppRecognitionVerdict = appVerdict },
        DeviceIntegrity = new DeviceIntegrity
        {
            DeviceRecognitionVerdict = deviceVerdicts ?? [PlayIntegrityVerdict.MeetsDeviceIntegrity],
        },
    };
}
