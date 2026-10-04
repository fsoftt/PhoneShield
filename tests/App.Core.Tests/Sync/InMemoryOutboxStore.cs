using Tranqui.App.Core.Sync;

namespace Tranqui.App.Core.Tests.Sync;

internal sealed class InMemoryOutboxStore : IOutboxStore
{
    private List<PendingOperation> operations = [];

    public IReadOnlyList<PendingOperation> Load() => [.. operations];

    public void Save(IReadOnlyList<PendingOperation> operations) => this.operations = [.. operations];
}
