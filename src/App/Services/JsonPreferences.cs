using System.Text.Json;

namespace Tranqui.App.Services;

/// <summary>A JSON value in the app's private preferences, read and written under a lock.</summary>
internal sealed class JsonPreferences<T>(string key, Func<T> empty)
{
    private readonly Lock gate = new();

    public T Read()
    {
        lock (gate)
        {
            var json = Preferences.Default.Get<string?>(key, null);
            return json is null ? empty() : JsonSerializer.Deserialize<T>(json) ?? empty();
        }
    }

    public void Write(T value)
    {
        lock (gate)
        {
            Preferences.Default.Set(key, JsonSerializer.Serialize(value));
        }
    }
}
