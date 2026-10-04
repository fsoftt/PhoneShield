using Tranqui.App.Core.Calls;
using Tranqui.App.Core.History;
using Tranqui.Contracts.Reports;

namespace Tranqui.App.Core.Storage;

/// <summary>Call history (device only), trimmed on every write to <see cref="CallHistoryRules"/>.</summary>
internal sealed class SqliteCallHistory(LocalDatabase database, TimeProvider timeProvider) : ICallHistory
{
    public Task AddAsync(CallRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        using var connection = database.Open();
        using var transaction = connection.BeginTransaction();
        using (var insert = connection.Command(
            """
            INSERT OR REPLACE INTO call_history (id, e164, display_number, title, state, block_reason, occurred_at, my_verdict)
            VALUES ($id, $e164, $display, $title, $state, $reason, $occurred, $verdict)
            """,
            ("$id", record.Id.ToString()),
            ("$e164", record.E164),
            ("$display", record.DisplayNumber),
            ("$title", record.Title),
            ("$state", (int)record.State),
            ("$reason", (int?)record.BlockReason),
            ("$occurred", record.OccurredAt.ToStored()),
            ("$verdict", (int?)record.MyVerdict)))
        {
            insert.Transaction = transaction;
            insert.ExecuteNonQuery();
        }

        using (var trim = connection.Command(
            """
            DELETE FROM call_history WHERE occurred_at <= $cutoff;
            DELETE FROM call_history WHERE id NOT IN (SELECT id FROM call_history ORDER BY occurred_at DESC LIMIT $max);
            """,
            ("$cutoff", (timeProvider.GetUtcNow() - CallHistoryRules.Retention).ToStored()),
            ("$max", CallHistoryRules.MaxEntries)))
        {
            trim.Transaction = transaction;
            trim.ExecuteNonQuery();
        }

        transaction.Commit();

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<CallRecord>> ListAsync()
    {
        using var connection = database.Open();
        using var query = connection.Command(
            """
            SELECT id, e164, display_number, title, state, block_reason, occurred_at, my_verdict
            FROM call_history WHERE occurred_at > $cutoff ORDER BY occurred_at DESC LIMIT $max
            """,
            ("$cutoff", (timeProvider.GetUtcNow() - CallHistoryRules.Retention).ToStored()),
            ("$max", CallHistoryRules.MaxEntries));
        using var reader = query.ExecuteReader();

        var records = new List<CallRecord>();
        while (reader.Read())
        {
            records.Add(new CallRecord(
                Guid.Parse(reader.GetString(0)),
                reader.NullableString(1),
                reader.GetString(2),
                reader.GetString(3),
                (CallerCardState)reader.GetInt32(4),
                (BlockReason?)reader.NullableInt(5),
                reader.GetInt64(6).ToDateTimeOffset(),
                (ReportVerdictDto?)reader.NullableInt(7)));
        }

        return Task.FromResult<IReadOnlyList<CallRecord>>(records);
    }

    public Task SetVerdictAsync(Guid id, ReportVerdictDto verdict)
    {
        using var connection = database.Open();
        using var update = connection.Command(
            "UPDATE call_history SET my_verdict = $verdict WHERE id = $id", ("$verdict", (int)verdict), ("$id", id.ToString()));
        update.ExecuteNonQuery();

        return Task.CompletedTask;
    }

    public Task ClearAsync()
    {
        using var connection = database.Open();
        using var delete = connection.Command("DELETE FROM call_history");
        delete.ExecuteNonQuery();

        return Task.CompletedTask;
    }
}
