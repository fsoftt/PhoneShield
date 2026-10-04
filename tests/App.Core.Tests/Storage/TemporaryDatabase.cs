using Microsoft.Data.Sqlite;
using Tranqui.App.Core.Storage;

namespace Tranqui.App.Core.Tests.Storage;

/// <summary>A real SQLite file per test, deleted afterwards.</summary>
public sealed class TemporaryDatabase : IDisposable
{
    private readonly string path = Path.Combine(Path.GetTempPath(), $"tranqui-{Guid.NewGuid():N}.db");

    public TemporaryDatabase()
    {
        Database = new LocalDatabase(path);
    }

    public LocalDatabase Database { get; }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        foreach (var file in new[] { path, path + "-wal", path + "-shm" }.Where(File.Exists))
        {
            File.Delete(file);
        }
    }
}
