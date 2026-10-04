using Microsoft.Data.Sqlite;

namespace Tranqui.App.Core.Storage;

/// <summary>Small helpers so every query is parameterized and every value converts the same way.</summary>
internal static class SqliteExtensions
{
    public static SqliteCommand Command(this SqliteConnection connection, string sql, params (string Name, object? Value)[] parameters)
    {
        var command = connection.CreateCommand();
        command.CommandText = sql;
        foreach (var (name, value) in parameters)
        {
            command.Parameters.AddWithValue(name, value ?? DBNull.Value);
        }

        return command;
    }

    public static long ToStored(this DateTimeOffset value) => value.ToUnixTimeMilliseconds();

    public static DateTimeOffset ToDateTimeOffset(this long storedMilliseconds) => DateTimeOffset.FromUnixTimeMilliseconds(storedMilliseconds);

    public static string? NullableString(this SqliteDataReader reader, int ordinal) => reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);

    public static int? NullableInt(this SqliteDataReader reader, int ordinal) => reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal);
}
