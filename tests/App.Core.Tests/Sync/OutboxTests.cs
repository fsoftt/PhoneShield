using System.Net;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Refit;
using Tranqui.App.Core.Api;
using Tranqui.App.Core.Sync;
using Tranqui.Contracts.Blocks;
using Tranqui.Contracts.Reports;

namespace Tranqui.App.Core.Tests.Sync;

public sealed class OutboxTests : IDisposable
{
    private const string Number = "+573001234567";

    private readonly ITranquiApi api = Substitute.For<ITranquiApi>();
    private readonly IBackgroundSync backgroundSync = Substitute.For<IBackgroundSync>();
    private readonly InMemoryOutboxStore store = new();
    private readonly Outbox outbox;

    public OutboxTests()
    {
        outbox = new Outbox(store, api, backgroundSync, new FakeTimeProvider(DateTimeOffset.UtcNow));
    }

    public void Dispose() => outbox.Dispose();

    [Fact]
    public async Task Enqueue_Online_SendsAndEmptiesTheQueue()
    {
        await outbox.EnqueueAsync(PendingOperationKind.Report, Number, ReportVerdictDto.Spam, "Spam Claro");
        await outbox.FlushAsync(CancellationToken.None);

        await api.Received(1).ReportCallAsync(new ReportCallRequest(Number, ReportVerdictDto.Spam, "Spam Claro"), Arg.Any<CancellationToken>());
        store.Load().Should().BeEmpty();
        backgroundSync.Received().ScheduleFlush();
    }

    [Fact]
    public async Task Flush_Offline_KeepsEverythingForLater()
    {
        api.BlockAsync(Arg.Any<BlockRequest>(), Arg.Any<CancellationToken>()).ThrowsAsync(new HttpRequestException());
        await outbox.EnqueueAsync(PendingOperationKind.Block, Number);

        var done = await outbox.FlushAsync(CancellationToken.None);

        done.Should().BeFalse();
        store.Load().Should().ContainSingle(operation => operation.Kind == PendingOperationKind.Block);
    }

    [Fact]
    public async Task Enqueue_LatestChangeForANumberReplacesTheEarlierOne()
    {
        api.ReportCallAsync(Arg.Any<ReportCallRequest>(), Arg.Any<CancellationToken>()).ThrowsAsync(new HttpRequestException());
        api.WithdrawReportAsync(Arg.Any<WithdrawReportRequest>(), Arg.Any<CancellationToken>()).ThrowsAsync(new HttpRequestException());
        await outbox.EnqueueAsync(PendingOperationKind.Report, Number, ReportVerdictDto.Spam);

        await outbox.EnqueueAsync(PendingOperationKind.WithdrawReport, Number);

        store.Load().Should().ContainSingle().Which.Kind.Should().Be(PendingOperationKind.WithdrawReport);
    }

    [Fact]
    public async Task Enqueue_BlockAndReportForTheSameNumber_AreBothKept()
    {
        api.ReportCallAsync(Arg.Any<ReportCallRequest>(), Arg.Any<CancellationToken>()).ThrowsAsync(new HttpRequestException());
        await outbox.EnqueueAsync(PendingOperationKind.Report, Number, ReportVerdictDto.Spam);

        await outbox.EnqueueAsync(PendingOperationKind.Block, Number);

        store.Load().Should().HaveCount(2);
    }

    [Fact]
    public async Task Flush_RejectedByTheServer_IsDropped()
    {
        api.ReportCallAsync(Arg.Any<ReportCallRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(await ApiExceptionAsync(HttpStatusCode.BadRequest));
        await outbox.EnqueueAsync(PendingOperationKind.Report, Number, ReportVerdictDto.Spam);

        await outbox.FlushAsync(CancellationToken.None);

        store.Load().Should().BeEmpty();
    }

    [Fact]
    public async Task Flush_RateLimited_KeepsItForLater()
    {
        api.ReportCallAsync(Arg.Any<ReportCallRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(await ApiExceptionAsync(HttpStatusCode.TooManyRequests));
        await outbox.EnqueueAsync(PendingOperationKind.Report, Number, ReportVerdictDto.Spam);

        await outbox.FlushAsync(CancellationToken.None);

        store.Load().Should().ContainSingle();
    }

    [Fact]
    public async Task Flush_ServerErrors_GiveUpAfterTheMaximumAttempts()
    {
        api.ReportCallAsync(Arg.Any<ReportCallRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(await ApiExceptionAsync(HttpStatusCode.InternalServerError));
        await outbox.EnqueueAsync(PendingOperationKind.Report, Number, ReportVerdictDto.Spam);

        for (var attempt = 0; attempt < Outbox.MaxAttempts; attempt++)
        {
            await outbox.FlushAsync(CancellationToken.None);
        }

        store.Load().Should().BeEmpty();
    }

    private static Task<ApiException> ApiExceptionAsync(HttpStatusCode status) =>
        ApiException.Create(new HttpRequestMessage(), HttpMethod.Post, new HttpResponseMessage(status), new RefitSettings());
}
