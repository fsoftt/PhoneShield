using Refit;
using Tranqui.App.Core.Api;
using Tranqui.Contracts.Accounts;
using Tranqui.Contracts.Contacts;
using Tranqui.Domain.Legal;
using Tranqui.Domain.Reputation;

namespace Tranqui.App.Core.Contacts;

/// <summary>
/// Opt-in contribution of the address book: records the consent, uploads it in batches (the server hashes numbers,
/// encrypts names and drops personal ones), re-syncs once a day, and withdraws everything on request.
/// </summary>
public sealed class ContactContributionService(
    ITranquiApi api,
    IDeviceContactSource contacts,
    IContributionState state,
    TimeProvider timeProvider)
{
    public static readonly TimeSpan SyncInterval = TimeSpan.FromDays(1);

    public bool IsContributing => state.IsContributing;

    public async Task<int> StartAsync(CancellationToken cancellationToken)
    {
        await api.AcceptContactUploadAsync(
            new AcceptContactUploadRequest(LegalDocuments.CurrentContactUploadVersion), cancellationToken);
        state.IsContributing = true;

        return await SyncAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await api.WithdrawContactsAsync(cancellationToken);
        state.IsContributing = false;
        state.LastSyncedAt = null;
    }

    /// <summary>Background re-sync; failures are ignored and retried on the next launch.</summary>
    public async Task SyncIfDueAsync(CancellationToken cancellationToken)
    {
        if (!state.IsContributing
            || (state.LastSyncedAt is { } last && timeProvider.GetUtcNow() - last < SyncInterval))
        {
            return;
        }

        try
        {
            await SyncAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is HttpRequestException or ApiException or OperationCanceledException)
        {
            // Offline or rate-limited: try again next time.
        }
    }

    private async Task<int> SyncAsync(CancellationToken cancellationToken)
    {
        var accepted = 0;
        foreach (var batch in contacts.ReadAll().Chunk(ContactUploadRules.MaxContactsPerBatch))
        {
            var request = new UploadContactsRequest(batch.Select(contact => new ContactDto(contact.PhoneNumber, contact.Name)).ToList());
            accepted += (await api.UploadContactsAsync(request, cancellationToken)).Accepted;
        }

        state.LastSyncedAt = timeProvider.GetUtcNow();
        return accepted;
    }
}
