using Tranqui.App.Core.Calls;
using Tranqui.App.Core.History;

namespace Tranqui.App.Core.Tests.History;

public sealed class CallHistoryRulesTests
{
    private static readonly DateTimeOffset now = new(2026, 10, 3, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Trim_DropsExpiredEntriesAndOrdersNewestFirst()
    {
        var old = Record(now - CallHistoryRules.Retention);
        var recent = Record(now.AddHours(-1));
        var newest = Record(now);

        CallHistoryRules.Trim([recent, old, newest], now).Should().Equal(newest, recent);
    }

    [Fact]
    public void Trim_KeepsAtMostTheMaximum()
    {
        var records = Enumerable.Range(0, CallHistoryRules.MaxEntries + 10).Select(minutes => Record(now.AddMinutes(-minutes)));

        CallHistoryRules.Trim(records, now).Should().HaveCount(CallHistoryRules.MaxEntries);
    }

    private static CallRecord Record(DateTimeOffset at) =>
        new(Guid.NewGuid(), "+573001234567", "+573001234567", "Título", CallerCardState.Unknown, null, at);
}
