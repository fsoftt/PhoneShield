using Tranqui.App.Core.Sync;
using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.App.Core.Calls;

/// <summary>
/// Blocking always works on the phone alone. When the user shares their blocks (Settings), each block or unblock is
/// also queued for the server, where it counts as a weak spam signal.
/// </summary>
public sealed class BlockingService(IBlockList blockList, IScreeningSettingsStore settings, Outbox outbox)
{
    public async Task BlockAsync(PhoneNumber number)
    {
        ArgumentNullException.ThrowIfNull(number);

        await blockList.AddAsync(number);
        if (settings.Load().ShareBlocks)
        {
            await outbox.EnqueueAsync(PendingOperationKind.Block, number.E164);
        }
    }

    public async Task UnblockAsync(PhoneNumber number)
    {
        ArgumentNullException.ThrowIfNull(number);

        await blockList.RemoveAsync(number);
        if (settings.Load().ShareBlocks)
        {
            await outbox.EnqueueAsync(PendingOperationKind.Unblock, number.E164);
        }
    }

    /// <summary>Turning sharing on sends every current block; turning it off takes them all back from the server.</summary>
    public async Task SetSharingAsync(bool share)
    {
        foreach (var number in await blockList.ListAsync())
        {
            await outbox.EnqueueAsync(share ? PendingOperationKind.Block : PendingOperationKind.Unblock, number.E164);
        }
    }
}
