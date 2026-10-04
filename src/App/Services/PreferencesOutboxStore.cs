using Tranqui.App.Core.Sync;

namespace Tranqui.App.Services;

/// <summary>Pending operations in the app's private storage, so nothing is lost if the app or phone restarts.</summary>
internal sealed class PreferencesOutboxStore : IOutboxStore
{
    private readonly JsonPreferences<List<PendingOperation>> storage = new("tranqui.outbox", () => []);

    public IReadOnlyList<PendingOperation> Load() => storage.Read();

    public void Save(IReadOnlyList<PendingOperation> operations) => storage.Write([.. operations]);
}
