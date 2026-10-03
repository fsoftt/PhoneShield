using System.Text.Json;
using Tranqui.App.Core.History;
using Tranqui.Contracts.Reports;

namespace Tranqui.App.Services;

/// <summary>Call history in the app's private storage, trimmed on every write (30 days, 200 entries).</summary>
internal sealed class PreferencesCallHistory(TimeProvider timeProvider) : ICallHistory
{
    private const string Key = "tranqui.call_history";

    private readonly Lock gate = new();

    public Task AddAsync(CallRecord record)
    {
        Update(records => records.Add(record));
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<CallRecord>> ListAsync()
    {
        IReadOnlyList<CallRecord> records = CallHistoryRules.Trim(Load(), timeProvider.GetUtcNow());
        return Task.FromResult(records);
    }

    public Task SetVerdictAsync(Guid id, ReportVerdictDto verdict)
    {
        Update(records =>
        {
            var index = records.FindIndex(record => record.Id == id);
            if (index >= 0)
            {
                records[index] = records[index] with { MyVerdict = verdict };
            }
        });
        return Task.CompletedTask;
    }

    public Task ClearAsync()
    {
        lock (gate)
        {
            Preferences.Default.Remove(Key);
        }

        return Task.CompletedTask;
    }

    private List<CallRecord> Load()
    {
        lock (gate)
        {
            var json = Preferences.Default.Get<string?>(Key, null);
            return json is null ? [] : JsonSerializer.Deserialize<List<CallRecord>>(json) ?? [];
        }
    }

    private void Update(Action<List<CallRecord>> change)
    {
        lock (gate)
        {
            var records = Load();
            change(records);
            Preferences.Default.Set(Key, JsonSerializer.Serialize(CallHistoryRules.Trim(records, timeProvider.GetUtcNow())));
        }
    }
}
