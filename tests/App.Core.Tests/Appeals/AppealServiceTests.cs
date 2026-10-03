using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Tranqui.App.Core.Api;
using Tranqui.App.Core.Appeals;
using Tranqui.App.Core.Authentication;
using Tranqui.Contracts.Appeals;
using Tranqui.Domain.Appeals;
using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.App.Core.Tests.Appeals;

public sealed class AppealServiceTests
{
    private const string Device = "9774d56d682e549c";
    private const string E164 = "+573001234567";

    private static readonly PhoneNumber number = PhoneNumber.TryParse(E164)!;

    private readonly ITranquiApi api = Substitute.For<ITranquiApi>();
    private readonly IFirebaseAuthClient firebase = Substitute.For<IFirebaseAuthClient>();
    private readonly IDeviceIntegrity device = Substitute.For<IDeviceIntegrity>();
    private readonly AppealService service;

    public AppealServiceTests()
    {
        device.DeviceId.Returns(Device);
        device.RequestTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(call => "token-for:" + call.Arg<string>());
        api.RequestAppealVerificationAsync(Arg.Any<AppealVerificationRequest>(), Arg.Any<CancellationToken>())
            .Returns(new AppealVerificationResponse(E164));
        firebase.SendVerificationCodeAsync(E164, Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns("session");
        firebase.SignInWithPhoneNumberAsync("session", "123456", Arg.Any<CancellationToken>()).Returns("phone-proof");
        api.SubmitAppealAsync(Arg.Any<SubmitAppealRequest>(), Arg.Any<CancellationToken>())
            .Returns(new AppealResponse(AppealStatusDto.Applied));
        service = new AppealService(api, firebase, device);
    }

    [Fact]
    public async Task RequestCodeAsync_AsksTheApiFirstWithAnIntegrityTokenForThisNumberAndDevice()
    {
        var request = await service.RequestCodeAsync(number, CancellationToken.None);

        request.Should().Be(new AppealCodeRequest(E164, "session"));
        var apiNonce = AppealNonce.Compute(AppealAction.SmsVerification, DeviceId.TryParse(Device)!, number);
        Received.InOrder(() =>
        {
            api.RequestAppealVerificationAsync(
                new AppealVerificationRequest(E164, Device, "token-for:" + apiNonce), Arg.Any<CancellationToken>());
            firebase.SendVerificationCodeAsync(E164, "token-for:" + FirebasePhoneNonce.For(E164), Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task RequestCodeAsync_ApiRefuses_SendsNoSms()
    {
        api.RequestAppealVerificationAsync(Arg.Any<AppealVerificationRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException());

        await FluentActions.Awaiting(() => service.RequestCodeAsync(number, CancellationToken.None))
            .Should().ThrowAsync<HttpRequestException>();
        await firebase.DidNotReceive().SendVerificationCodeAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SubmitAsync_SendsTheProofAndDeletesTheTemporaryFirebaseUser()
    {
        var status = await service.SubmitAsync(
            new AppealCodeRequest(E164, "session"), "123456", AppealKindDto.HideNames, null, null, CancellationToken.None);

        status.Should().Be(AppealStatusDto.Applied);
        var appealNonce = AppealNonce.Compute(AppealAction.Appeal, DeviceId.TryParse(Device)!, number);
        await api.Received(1).SubmitAppealAsync(
            new SubmitAppealRequest("phone-proof", Device, "token-for:" + appealNonce, AppealKindDto.HideNames, null, null),
            Arg.Any<CancellationToken>());
        await firebase.Received(1).DeleteAccountAsync("phone-proof", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SubmitAsync_ApiFails_StillDeletesTheTemporaryFirebaseUser()
    {
        api.SubmitAppealAsync(Arg.Any<SubmitAppealRequest>(), Arg.Any<CancellationToken>()).ThrowsAsync(new HttpRequestException());

        await FluentActions.Awaiting(() => service.SubmitAsync(
                new AppealCodeRequest(E164, "session"), "123456", AppealKindDto.ReviewSpam, null, null, CancellationToken.None))
            .Should().ThrowAsync<HttpRequestException>();
        await firebase.Received(1).DeleteAccountAsync("phone-proof", Arg.Any<CancellationToken>());
    }
}
