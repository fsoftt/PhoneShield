namespace Tranqui.App.Core.Tests.Storage;

public sealed class LocalDatabaseTests : IDisposable
{
    private readonly TemporaryDatabase temporary = new();

    public void Dispose() => temporary.Dispose();

    [Fact]
    public void Open_CreatesTheSchemaOnce()
    {
        using (var first = temporary.Database.Open())
        {
            Version(first).Should().BeGreaterThan(0);
        }

        using var second = temporary.Database.Open();
        using var tables = second.CreateCommand();
        tables.CommandText = "SELECT count(*) FROM sqlite_master WHERE type = 'table' AND name IN ('call_history', 'blocked_numbers', 'my_reports', 'outbox')";

        Convert.ToInt32(tables.ExecuteScalar(), System.Globalization.CultureInfo.InvariantCulture).Should().Be(4);
    }

    private static int Version(Microsoft.Data.Sqlite.SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA user_version;";

        return Convert.ToInt32(command.ExecuteScalar(), System.Globalization.CultureInfo.InvariantCulture);
    }
}
