using Microsoft.Data.Sqlite;

namespace Tranqui.App.Core.Storage;

/// <summary>
/// The app's SQLite database, in its private storage (Android sandbox, excluded from backups). Opens a pooled
/// connection per operation and upgrades the schema on first use with the ordered <see cref="migrations"/>, tracked in
/// <c>PRAGMA user_version</c>. Add a migration at the end; never edit a published one.
/// </summary>
public sealed class LocalDatabase(string path)
{
    private static readonly string[] migrations =
    [
        """
        CREATE TABLE call_history (
            id TEXT PRIMARY KEY,
            e164 TEXT NULL,
            display_number TEXT NOT NULL,
            title TEXT NOT NULL,
            state INTEGER NOT NULL,
            block_reason INTEGER NULL,
            occurred_at INTEGER NOT NULL,
            my_verdict INTEGER NULL
        );
        CREATE INDEX ix_call_history_occurred_at ON call_history (occurred_at);
        CREATE TABLE blocked_numbers (e164 TEXT PRIMARY KEY);
        CREATE TABLE my_reports (
            e164 TEXT PRIMARY KEY,
            verdict INTEGER NOT NULL,
            label TEXT NULL,
            reported_at INTEGER NOT NULL
        );
        CREATE TABLE outbox (
            position INTEGER PRIMARY KEY AUTOINCREMENT,
            id TEXT NOT NULL,
            kind INTEGER NOT NULL,
            e164 TEXT NOT NULL,
            verdict INTEGER NULL,
            label TEXT NULL,
            created_at INTEGER NOT NULL,
            attempts INTEGER NOT NULL
        );
        """,
    ];

    private readonly string connectionString = new SqliteConnectionStringBuilder
    {
        DataSource = path,
        Mode = SqliteOpenMode.ReadWriteCreate,
        Pooling = true,
    }.ToString();

    private readonly Lazy<bool> migrated = new(() => Migrate(path));

    public SqliteConnection Open()
    {
        _ = migrated.Value;
        var connection = new SqliteConnection(connectionString);
        connection.Open();

        return connection;
    }

    private static bool Migrate(string path)
    {
        using var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = path }.ToString());
        connection.Open();
        Execute(connection, "PRAGMA journal_mode = WAL;");

        var version = Convert.ToInt32(Scalar(connection, "PRAGMA user_version;"), System.Globalization.CultureInfo.InvariantCulture);
        for (var index = version; index < migrations.Length; index++)
        {
            using var transaction = connection.BeginTransaction();
            Execute(connection, migrations[index], transaction);
            Execute(connection, $"PRAGMA user_version = {index + 1};", transaction);
            transaction.Commit();
        }

        return true;
    }

    private static void Execute(SqliteConnection connection, string sql, SqliteTransaction? transaction = null)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private static object? Scalar(SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;

        return command.ExecuteScalar();
    }
}
