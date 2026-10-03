using System.Text.Json;
using Tranqui.App.Core.Calls;
using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.App.Services;

/// <summary>Blocked numbers in the app's private preferences. They never leave the device.</summary>
internal sealed class PreferencesBlockList : IBlockList
{
    private const string Key = "tranqui.block_list";

    private readonly Lock gate = new();

    public Task<bool> ContainsAsync(PhoneNumber number) => Task.FromResult(Load().Contains(number.E164));

    public Task AddAsync(PhoneNumber number)
    {
        Update(numbers => numbers.Add(number.E164));
        return Task.CompletedTask;
    }

    public Task RemoveAsync(PhoneNumber number)
    {
        Update(numbers => numbers.Remove(number.E164));
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<PhoneNumber>> ListAsync()
    {
        IReadOnlyList<PhoneNumber> numbers = Load().Select(e164 => PhoneNumber.TryParse(e164)).OfType<PhoneNumber>().ToList();
        return Task.FromResult(numbers);
    }

    private HashSet<string> Load()
    {
        lock (gate)
        {
            var json = Preferences.Default.Get<string?>(Key, null);
            return json is null ? [] : JsonSerializer.Deserialize<HashSet<string>>(json) ?? [];
        }
    }

    private void Update(Action<HashSet<string>> change)
    {
        lock (gate)
        {
            var numbers = Load();
            change(numbers);
            Preferences.Default.Set(Key, JsonSerializer.Serialize(numbers));
        }
    }
}
