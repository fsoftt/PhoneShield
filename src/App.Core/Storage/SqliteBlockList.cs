using Tranqui.App.Core.Calls;
using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.App.Core.Storage;

internal sealed class SqliteBlockList(LocalDatabase database) : IBlockList
{
    public Task<bool> ContainsAsync(PhoneNumber number)
    {
        ArgumentNullException.ThrowIfNull(number);

        using var connection = database.Open();
        using var query = connection.Command("SELECT 1 FROM blocked_numbers WHERE e164 = $e164", ("$e164", number.E164));

        return Task.FromResult(query.ExecuteScalar() is not null);
    }

    public Task AddAsync(PhoneNumber number) =>
        Execute("INSERT OR IGNORE INTO blocked_numbers (e164) VALUES ($e164)", number);

    public Task RemoveAsync(PhoneNumber number) =>
        Execute("DELETE FROM blocked_numbers WHERE e164 = $e164", number);

    public Task<IReadOnlyList<PhoneNumber>> ListAsync()
    {
        using var connection = database.Open();
        using var query = connection.Command("SELECT e164 FROM blocked_numbers ORDER BY e164");
        using var reader = query.ExecuteReader();

        var numbers = new List<PhoneNumber>();
        while (reader.Read())
        {
            if (PhoneNumber.TryParse(reader.GetString(0)) is { } number)
            {
                numbers.Add(number);
            }
        }

        return Task.FromResult<IReadOnlyList<PhoneNumber>>(numbers);
    }

    private Task Execute(string sql, PhoneNumber number)
    {
        ArgumentNullException.ThrowIfNull(number);

        using var connection = database.Open();
        using var command = connection.Command(sql, ("$e164", number.E164));
        command.ExecuteNonQuery();

        return Task.CompletedTask;
    }
}
