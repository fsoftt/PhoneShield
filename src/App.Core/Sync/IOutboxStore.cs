namespace Tranqui.App.Core.Sync;

/// <summary>Durable storage for pending operations, in the app's private storage. Provided by the platform.</summary>
public interface IOutboxStore
{
    IReadOnlyList<PendingOperation> Load();

    void Save(IReadOnlyList<PendingOperation> operations);
}
