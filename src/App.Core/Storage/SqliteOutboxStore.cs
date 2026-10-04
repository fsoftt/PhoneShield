using Tranqui.App.Core.Sync;
using Tranqui.Contracts.Reports;

namespace Tranqui.App.Core.Storage;

/// <summary>Pending operations, kept in order. Saving replaces the whole queue in one transaction.</summary>
internal sealed class SqliteOutboxStore(LocalDatabase database) : IOutboxStore
{
    public IReadOnlyList<PendingOperation> Load()
    {
        using var connection = database.Open();
        using var query = connection.Command(
            "SELECT id, kind, e164, verdict, label, created_at, attempts FROM outbox ORDER BY position");
        using var reader = query.ExecuteReader();

        var operations = new List<PendingOperation>();
        while (reader.Read())
        {
            operations.Add(new PendingOperation(
                Guid.Parse(reader.GetString(0)),
                (PendingOperationKind)reader.GetInt32(1),
                reader.GetString(2),
                (ReportVerdictDto?)reader.NullableInt(3),
                reader.NullableString(4),
                reader.GetInt64(5).ToDateTimeOffset(),
                reader.GetInt32(6)));
        }

        return operations;
    }

    public void Save(IReadOnlyList<PendingOperation> operations)
    {
        ArgumentNullException.ThrowIfNull(operations);

        using var connection = database.Open();
        using var transaction = connection.BeginTransaction();
        using (var clear = connection.Command("DELETE FROM outbox"))
        {
            clear.Transaction = transaction;
            clear.ExecuteNonQuery();
        }

        foreach (var operation in operations)
        {
            using var insert = connection.Command(
                """
                INSERT INTO outbox (id, kind, e164, verdict, label, created_at, attempts)
                VALUES ($id, $kind, $e164, $verdict, $label, $created, $attempts)
                """,
                ("$id", operation.Id.ToString()),
                ("$kind", (int)operation.Kind),
                ("$e164", operation.E164),
                ("$verdict", (int?)operation.Verdict),
                ("$label", operation.Label),
                ("$created", operation.CreatedAt.ToStored()),
                ("$attempts", operation.Attempts));
            insert.Transaction = transaction;
            insert.ExecuteNonQuery();
        }

        transaction.Commit();
    }
}
