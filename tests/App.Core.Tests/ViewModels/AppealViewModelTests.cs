using System.Net;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Refit;
using Tranqui.App.Core.Api;
using Tranqui.App.Core.Appeals;
using Tranqui.App.Core.Authentication;
using Tranqui.App.Core.Resources;
using Tranqui.App.Core.ViewModels;
using Tranqui.Contracts.Appeals;
using Tranqui.Contracts.Errors;

namespace Tranqui.App.Core.Tests.ViewModels;

public sealed class AppealViewModelTests
{
    private const string E164 = "+573001234567";

    private readonly ITranquiApi api = Substitute.For<ITranquiApi>();
    private readonly IFirebaseAuthClient firebase = Substitute.For<IFirebaseAuthClient>();
    private readonly AppealViewModel viewModel;

    public AppealViewModelTests()
    {
        var device = Substitute.For<IDeviceIntegrity>();
        device.DeviceId.Returns("9774d56d682e549c");
        device.RequestTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns("integrity");
        api.RequestAppealVerificationAsync(Arg.Any<AppealVerificationRequest>(), Arg.Any<CancellationToken>())
            .Returns(new AppealVerificationResponse(E164));
        firebase.SendVerificationCodeAsync(E164, Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns("session");
        firebase.SignInWithPhoneNumberAsync("session", Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns("proof");
        viewModel = new AppealViewModel(new AppealService(api, firebase, device));
    }

    [Fact]
    public async Task SendCode_InvalidNumber_ShowsErrorAndCallsNothing()
    {
        viewModel.PhoneNumber = "12";

        await viewModel.SendCodeCommand.ExecuteAsync(null);

        viewModel.ErrorMessage.Should().Be(Texts.InvalidPhoneNumber);
        viewModel.IsDetailsStep.Should().BeTrue();
        await api.DidNotReceive().RequestAppealVerificationAsync(Arg.Any<AppealVerificationRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendCode_ValidNumber_MovesToTheCodeStep()
    {
        viewModel.PhoneNumber = "300 123 4567";

        await viewModel.SendCodeCommand.ExecuteAsync(null);

        viewModel.IsCodeStep.Should().BeTrue();
        viewModel.CodeSentMessage.Should().Contain(E164);
    }

    [Fact]
    public async Task SendCode_LimitReached_ShowsTheLimitMessage()
    {
        var problem = await ApiException.Create(
            new HttpRequestMessage(),
            HttpMethod.Post,
            new HttpResponseMessage(HttpStatusCode.TooManyRequests)
            {
                Content = new StringContent($$"""{"{{ApiErrorCodes.Field}}":"{{ApiErrorCodes.AppealLimitReached}}"}"""),
            },
            new RefitSettings());
        api.RequestAppealVerificationAsync(Arg.Any<AppealVerificationRequest>(), Arg.Any<CancellationToken>()).ThrowsAsync(problem);
        viewModel.PhoneNumber = "3001234567";

        await viewModel.SendCodeCommand.ExecuteAsync(null);

        viewModel.ErrorMessage.Should().Be(Texts.ErrorAppealLimitReached);
        viewModel.IsDetailsStep.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("12345")]
    [InlineData("12a456")]
    public async Task Submit_CodeNotSixDigits_ShowsError(string code)
    {
        await GivenCodeSentAsync();
        viewModel.Code = code;

        await viewModel.SubmitCommand.ExecuteAsync(null);

        viewModel.ErrorMessage.Should().Be(Texts.AppealCodeRequired);
        await api.DidNotReceive().SubmitAppealAsync(Arg.Any<SubmitAppealRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Submit_ReviewSpam_SendsTheEmailAndShowsPending()
    {
        api.SubmitAppealAsync(Arg.Any<SubmitAppealRequest>(), Arg.Any<CancellationToken>())
            .Returns(new AppealResponse(AppealStatusDto.Pending));
        await GivenCodeSentAsync();
        viewModel.IsReviewSpam = true;
        viewModel.ContactEmail = " owner@example.com ";
        viewModel.Code = "123456";

        await viewModel.SubmitCommand.ExecuteAsync(null);

        viewModel.IsDone.Should().BeTrue();
        viewModel.ResultMessage.Should().Be(Texts.AppealPending);
        await api.Received(1).SubmitAppealAsync(
            Arg.Is<SubmitAppealRequest>(request => request.Kind == AppealKindDto.ReviewSpam && request.ContactEmail == "owner@example.com"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Submit_HideNames_NeverSendsAnEmail()
    {
        api.SubmitAppealAsync(Arg.Any<SubmitAppealRequest>(), Arg.Any<CancellationToken>())
            .Returns(new AppealResponse(AppealStatusDto.Applied));
        await GivenCodeSentAsync();
        viewModel.ContactEmail = "owner@example.com";
        viewModel.Code = "123456";

        await viewModel.SubmitCommand.ExecuteAsync(null);

        viewModel.ResultMessage.Should().Be(Texts.AppealApplied);
        await api.Received(1).SubmitAppealAsync(
            Arg.Is<SubmitAppealRequest>(request => request.Kind == AppealKindDto.HideNames && request.ContactEmail == null),
            Arg.Any<CancellationToken>());
    }

    private async Task GivenCodeSentAsync()
    {
        viewModel.PhoneNumber = "3001234567";
        await viewModel.SendCodeCommand.ExecuteAsync(null);
    }
}
