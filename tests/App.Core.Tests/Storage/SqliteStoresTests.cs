using Tranqui.App.Core.Reports;
using Tranqui.App.Core.Storage;
using Tranqui.App.Core.Sync;
using Tranqui.Contracts.Reports;
using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.App.Core.Tests.Storage;

public sealed class SqliteStoresTests : IDisposable
{
    private static readonly DateTimeOffset now = new(2026, 10, 3, 12, 0, 0, TimeSpan.Zero);
    private static readonly PhoneNumber first = PhoneNumber.TryParse("+573001112233")!;
    private static readonly PhoneNumber second = PhoneNumber.TryParse("+573004445566")!;

    private readonly TemporaryDatabase temporary = new();

    public void Dispose() => temporary.Dispose();

    [Fact]
    public async Task BlockList_AddIsIdempotentAndRemoveWorks()
    {
        var blockList = new SqliteBlockList(temporary.Database);

        await blockList.AddAsync(second);
        await blockList.AddAsync(first);
        await blockList.AddAsync(first);
        await blockList.RemoveAsync(second);

        (await blockList.ListAsync()).Should().Equal(first);
        (await blockList.ContainsAsync(first)).Should().BeTrue();
        (await blockList.ContainsAsync(second)).Should().BeFalse();
    }

    [Fact]
    public void MyReports_SaveReplacesTheReportForTheNumber()
    {
        var reports = new SqliteMyReports(temporary.Database);

        reports.Save(new MyReport(first.E164, ReportVerdictDto.Spam, "Spam Claro", now));
        reports.Save(new MyReport(first.E164, ReportVerdictDto.NotSpam, null, now.AddMinutes(1)));
        reports.Save(new MyReport(second.E164, ReportVerdictDto.Spam, null, now.AddMinutes(-1)));

        reports.List().Should().Equal(
            new MyReport(first.E164, ReportVerdictDto.NotSpam, null, now.AddMinutes(1)),
            new MyReport(second.E164, ReportVerdictDto.Spam, null, now.AddMinutes(-1)));
    }

    [Fact]
    public void MyReports_Remove()
    {
        var reports = new SqliteMyReports(temporary.Database);
        reports.Save(new MyReport(first.E164, ReportVerdictDto.Spam, null, now));

        reports.Remove(first.E164);

        reports.List().Should().BeEmpty();
    }

    [Fact]
    public void Outbox_SaveReplacesTheQueueAndKeepsTheOrder()
    {
        var store = new SqliteOutboxStore(temporary.Database);
        PendingOperation[] queue =
        [
            new(Guid.NewGuid(), PendingOperationKind.Report, first.E164, ReportVerdictDto.Spam, "Spam Claro", now),
            new(Guid.NewGuid(), PendingOperationKind.Block, second.E164, null, null, now.AddSeconds(1), Attempts: 3),
        ];

        store.Save([new(Guid.NewGuid(), PendingOperationKind.Unblock, first.E164, null, null, now)]);
        store.Save(queue);

        store.Load().Should().Equal(queue);
    }
}
