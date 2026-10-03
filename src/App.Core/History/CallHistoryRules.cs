namespace Tranqui.App.Core.History;

/// <summary>Data minimization on the device too: the history is short and expires.</summary>
public static class CallHistoryRules
{
    public const int MaxEntries = 200;

    public static readonly TimeSpan Retention = TimeSpan.FromDays(30);

    /// <summary>Newest first, without expired entries, capped at <see cref="MaxEntries"/>.</summary>
    public static List<CallRecord> Trim(IEnumerable<CallRecord> records, DateTimeOffset now) =>
        records.Where(record => now - record.OccurredAt < Retention)
            .OrderByDescending(record => record.OccurredAt)
            .Take(MaxEntries)
            .ToList();
}
