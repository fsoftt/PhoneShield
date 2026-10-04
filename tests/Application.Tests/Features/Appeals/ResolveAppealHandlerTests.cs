using System.Security.Cryptography;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Tranqui.Application.Errors;
using Tranqui.Application.Features.ResolveAppeal;
using Tranqui.Domain.Abstractions;
using Tranqui.Domain.Appeals;
using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.Application.Tests.Features.Appeals;

public sealed class ResolveAppealHandlerTests
{
    private static readonly DateTimeOffset now = new(2026, 10, 3, 12, 0, 0, TimeSpan.Zero);
    private static readonly PhoneHash hash = new(RandomNumberGenerator.GetBytes(PhoneHash.SizeInBytes), 1);

    private readonly IAppealRepository appeals = Substitute.For<IAppealRepository>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ResolveAppealHandler handler;

    public ResolveAppealHandlerTests()
    {
        handler = new ResolveAppealHandler(appeals, unitOfWork, new FakeTimeProvider(now));
    }

    [Fact]
    public async Task Approve_ClearsTheNumber()
    {
        var appeal = GivenAppeal(AppealKind.ReviewSpam);

        var status = await handler.Handle(new ResolveAppealCommand(appeal.Id, AppealDecision.Approve), CancellationToken.None);

        status.Should().Be(AppealStatus.Approved);
        appeals.Received(1).Clear(Arg.Is<ClearedNumber>(cleared => cleared.ClearedAt == now));
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Approve_AlreadyClearedNumber_MovesTheClearingForward()
    {
        var appeal = GivenAppeal(AppealKind.ReviewSpam);
        var earlier = ClearedNumber.Create(hash, now.AddDays(-200));
        appeals.GetClearedAsync(Arg.Any<PhoneHash>(), Arg.Any<CancellationToken>()).Returns(earlier);

        await handler.Handle(new ResolveAppealCommand(appeal.Id, AppealDecision.Approve), CancellationToken.None);

        earlier.ClearedAt.Should().Be(now);
        appeals.DidNotReceive().Clear(Arg.Any<ClearedNumber>());
    }

    [Fact]
    public async Task Reject_ClearsNothing()
    {
        var appeal = GivenAppeal(AppealKind.ReviewSpam);

        var status = await handler.Handle(new ResolveAppealCommand(appeal.Id, AppealDecision.Reject), CancellationToken.None);

        status.Should().Be(AppealStatus.Rejected);
        appeals.DidNotReceive().Clear(Arg.Any<ClearedNumber>());
    }

    [Fact]
    public async Task Missing_Throws()
    {
        var act = () => handler.Handle(new ResolveAppealCommand(Guid.NewGuid(), AppealDecision.Reject), CancellationToken.None);

        await act.Should().ThrowAsync<AppealNotFoundException>();
    }

    [Fact]
    public async Task NotPending_Throws()
    {
        var appeal = GivenAppeal(AppealKind.HideNames);

        var act = () => handler.Handle(new ResolveAppealCommand(appeal.Id, AppealDecision.Approve), CancellationToken.None);

        await act.Should().ThrowAsync<AppealAlreadyResolvedException>();
    }

    private Appeal GivenAppeal(AppealKind kind)
    {
        var appeal = Appeal.File(hash, kind, "reason", "owner@example.com", now.AddDays(-2));
        appeals.GetAppealAsync(appeal.Id, Arg.Any<CancellationToken>()).Returns(appeal);

        return appeal;
    }
}
