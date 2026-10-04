using Microsoft.Extensions.Time.Testing;
using Tranqui.App.Core.Calls;
using Tranqui.App.Core.History;
using Tranqui.App.Core.Storage;
using Tranqui.Contracts.Reports;

namespace Tranqui.App.Core.Tests.Storage;

public sealed class SqliteCallHistoryTests : IDisposable
{
    private readonly TemporaryDatabase temporary = new();
    private readonly FakeTimeProvider time = new(new DateTimeOffset(2026, 10, 3, 12, 0, 0, TimeSpan.Zero));
    private readonly SqliteCallHistory history;

    public SqliteCallHistoryTests()
    {
        history = new SqliteCallHistory(temporary.Database, time);
    }

    public void Dispose() => temporary.Dispose();

    [Fact]
    public async Task AddAndList_RoundTripsEveryFieldNewestFirst()
    {
        var older = Record(time.GetUtcNow().AddHours(-2)) with { BlockReason = BlockReason.Prefix, MyVerdict = ReportVerdictDto.Spam };
        var newer = Record(time.GetUtcNow().AddHours(-1)) with { E164 = null, State = CallerCardState.PrivateNumber };
        await history.AddAsync(older);
        await history.AddAsync(newer);

        var records = await history.ListAsync();

        records.Should().Equal(newer, older);
    }

    [Fact]
    public async Task Add_DropsCallsOlderThanTheRetention()
    {
        await history.AddAsync(Record(time.GetUtcNow() - CallHistoryRules.Retention - TimeSpan.FromMinutes(1)));
        await history.AddAsync(Record(time.GetUtcNow()));

        (await history.ListAsync()).Should().ContainSingle();
    }

    [Fact]
    public async Task Add_KeepsOnlyTheNewestEntries()
    {
        for (var index = 0; index < CallHistoryRules.MaxEntries + 5; index++)
        {
            await history.AddAsync(Record(time.GetUtcNow().AddMinutes(-index)));
        }

        var records = await history.ListAsync();

        records.Should().HaveCount(CallHistoryRules.MaxEntries);
        records[0].OccurredAt.Should().Be(time.GetUtcNow());
    }

    [Fact]
    public async Task SetVerdict_UpdatesTheEntry()
    {
        var record = Record(time.GetUtcNow());
        await history.AddAsync(record);

        await history.SetVerdictAsync(record.Id, ReportVerdictDto.NotSpam);

        (await history.ListAsync()).Single().MyVerdict.Should().Be(ReportVerdictDto.NotSpam);
    }

    [Fact]
    public async Task Clear_EmptiesIt()
    {
        await history.AddAsync(Record(time.GetUtcNow()));

        await history.ClearAsync();

        (await history.ListAsync()).Should().BeEmpty();
    }

    private static CallRecord Record(DateTimeOffset occurredAt) =>
        new(Guid.NewGuid(), "+573001234567", "+573001234567", "Spam Claro", CallerCardState.Spam, null, occurredAt);
}
