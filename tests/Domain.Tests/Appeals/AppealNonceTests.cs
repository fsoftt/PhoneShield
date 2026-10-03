using Tranqui.Domain.Appeals;
using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.Domain.Tests.Appeals;

public sealed class AppealNonceTests
{
    private static readonly DeviceId device = DeviceId.TryParse("9774d56d682e549c")!;
    private static readonly PhoneNumber number = PhoneNumber.TryParse("3001234567")!;

    [Fact]
    public void Compute_IsStableAndUrlSafe()
    {
        var nonce = AppealNonce.Compute(AppealAction.Appeal, device, number);

        nonce.Should().Be(AppealNonce.Compute(AppealAction.Appeal, device, number));
        nonce.Should().MatchRegex("^[A-Za-z0-9_=-]+$");
    }

    [Fact]
    public void Compute_BindsTheActionDeviceAndNumber()
    {
        var nonce = AppealNonce.Compute(AppealAction.Appeal, device, number);

        nonce.Should().NotBe(AppealNonce.Compute(AppealAction.SmsVerification, device, number));
        nonce.Should().NotBe(AppealNonce.Compute(AppealAction.Appeal, DeviceId.TryParse("1234")!, number));
        nonce.Should().NotBe(AppealNonce.Compute(AppealAction.Appeal, device, PhoneNumber.TryParse("3007654321")!));
    }
}
