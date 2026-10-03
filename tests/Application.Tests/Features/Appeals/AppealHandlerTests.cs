using System.Security.Cryptography;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Tranqui.Application.Abstractions;
using Tranqui.Application.Errors;
using Tranqui.Application.Features.RequestAppealVerification;
using Tranqui.Application.Features.SubmitAppeal;
using Tranqui.Domain.Abstractions;
using Tranqui.Domain.Appeals;
using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.Application.Tests.Features.Appeals;

public sealed class AppealHandlerTests
{
    private static readonly DateTimeOffset now = new(2026, 10, 3, 12, 0, 0, TimeSpan.Zero);
    private static readonly PhoneHash hash = new(RandomNumberGenerator.GetBytes(PhoneHash.SizeInBytes), 1);

    private readonly IAppealRepository appeals = Substitute.For<IAppealRepository>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IPhoneNumberHasher hasher = Substitute.For<IPhoneNumberHasher>();
    private readonly FakeTimeProvider timeProvider = new(now);

    public AppealHandlerTests()
    {
        hasher.Hash(Arg.Any<PhoneNumber>()).Returns(hash);
    }

    [Fact]
    public async Task RequestVerification_FirstInTheWindow_RecordsItAndReturnsE164()
    {
        var handler = new RequestAppealVerificationHandler(hasher, appeals, unitOfWork, timeProvider);

        var e164 = await handler.Handle(new RequestAppealVerificationCommand("300 123 4567"), CancellationToken.None);

        e164.Should().Be("+573001234567");
        appeals.Received(1).AddVerification(Arg.Is<SmsVerificationRequest>(request => request.RequestedAt == now));
        await appeals.Received(1).CountVerificationsSinceAsync(hash, now - AppealRules.LimitWindow, Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RequestVerification_AlreadySentThisMonth_Throws()
    {
        appeals.CountVerificationsSinceAsync(hash, Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>()).Returns(1);
        var handler = new RequestAppealVerificationHandler(hasher, appeals, unitOfWork, timeProvider);

        var act = () => handler.Handle(new RequestAppealVerificationCommand("3001234567"), CancellationToken.None);

        await act.Should().ThrowAsync<AppealLimitReachedException>();
        appeals.DidNotReceive().AddVerification(Arg.Any<SmsVerificationRequest>());
    }

    [Fact]
    public async Task Submit_HideNames_HidesTheNumber()
    {
        var handler = SubmitHandler();

        var status = await handler.Handle(new SubmitAppealCommand(AppealKind.HideNames, null, null), CancellationToken.None);

        status.Should().Be(AppealStatus.Applied);
        appeals.Received(1).Hide(Arg.Any<HiddenNumber>());
    }

    [Fact]
    public async Task Submit_HideNamesForAHiddenNumber_DoesNotHideTwice()
    {
        appeals.IsHiddenAsync(hash, Arg.Any<CancellationToken>()).Returns(true);

        await SubmitHandler().Handle(new SubmitAppealCommand(AppealKind.HideNames, null, null), CancellationToken.None);

        appeals.DidNotReceive().Hide(Arg.Any<HiddenNumber>());
    }

    [Fact]
    public async Task Submit_ReviewSpam_IsPendingAndHidesNothing()
    {
        var status = await SubmitHandler().Handle(
            new SubmitAppealCommand(AppealKind.ReviewSpam, "Mi tienda", null), CancellationToken.None);

        status.Should().Be(AppealStatus.Pending);
        appeals.DidNotReceive().Hide(Arg.Any<HiddenNumber>());
    }

    [Fact]
    public async Task Submit_AlreadyAppealedThisMonth_Throws()
    {
        appeals.CountAppealsSinceAsync(hash, Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>()).Returns(1);

        var act = () => SubmitHandler().Handle(new SubmitAppealCommand(AppealKind.HideNames, null, null), CancellationToken.None);

        await act.Should().ThrowAsync<AppealLimitReachedException>();
        appeals.DidNotReceive().AddAppeal(Arg.Any<Appeal>());
    }

    private SubmitAppealHandler SubmitHandler()
    {
        var verifiedPhone = Substitute.For<IVerifiedPhone>();
        verifiedPhone.E164.Returns("+573001234567");

        return new SubmitAppealHandler(verifiedPhone, hasher, appeals, unitOfWork, timeProvider);
    }
}
