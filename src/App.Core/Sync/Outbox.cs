using System.Net;
using Refit;
using Tranqui.App.Core.Api;
using Tranqui.Contracts.Blocks;
using Tranqui.Contracts.Reports;

namespace Tranqui.App.Core.Sync;

/// <summary>
/// Offline-first writes: every report, withdrawal, block and unblock is stored on the phone first and sent when there
/// is a connection, in order. Lookups cannot work this way (they need the server), so they rely on the cache instead.
/// </summary>
public sealed class Outbox(IOutboxStore store, ITranquiApi api, IBackgroundSync backgroundSync, TimeProvider timeProvider)
    : IDisposable
{
    /// <summary>A request the server keeps failing with an unexpected error is dropped after this many tries.</summary>
    public const int MaxAttempts = 10;

    private readonly SemaphoreSlim flushing = new(1, 1);

    public IReadOnlyList<PendingOperation> Pending => store.Load();

    public async Task EnqueueAsync(PendingOperationKind kind, string e164, ReportVerdictDto? verdict = null, string? label = null)
    {
        var operation = new PendingOperation(Guid.NewGuid(), kind, e164, verdict, label, timeProvider.GetUtcNow());
        await flushing.WaitAsync();
        try
        {
            store.Save([.. store.Load().Where(existing => !operation.Supersedes(existing)), operation]);
        }
        finally
        {
            flushing.Release();
        }

        backgroundSync.ScheduleFlush();
        _ = FlushAsync(CancellationToken.None);
    }

    /// <summary>Sends what it can. Returns true when nothing is left (or only operations waiting for a retry).</summary>
    public async Task<bool> FlushAsync(CancellationToken cancellationToken)
    {
        await flushing.WaitAsync(cancellationToken);
        try
        {
            var remaining = store.Load().ToList();
            while (remaining.Count > 0)
            {
                var operation = remaining[0];
                var outcome = await SendAsync(operation, cancellationToken);
                if (outcome == Outcome.TryLater)
                {
                    store.Save(remaining);
                    return false;
                }

                remaining.RemoveAt(0);
                if (outcome == Outcome.Retry && operation.Attempts + 1 < MaxAttempts)
                {
                    remaining.Add(operation with { Attempts = operation.Attempts + 1 });
                    store.Save(remaining);
                    return false;
                }

                store.Save(remaining);
            }

            return true;
        }
        finally
        {
            flushing.Release();
        }
    }

    public void Dispose() => flushing.Dispose();

    private async Task<Outcome> SendAsync(PendingOperation operation, CancellationToken cancellationToken)
    {
        try
        {
            await (operation.Kind switch
            {
                PendingOperationKind.Report => api.ReportCallAsync(
                    new ReportCallRequest(operation.E164, operation.Verdict ?? ReportVerdictDto.Spam, operation.Label), cancellationToken),
                PendingOperationKind.WithdrawReport => api.WithdrawReportAsync(new WithdrawReportRequest(operation.E164), cancellationToken),
                PendingOperationKind.Block => api.BlockAsync(new BlockRequest(operation.E164), cancellationToken),
                PendingOperationKind.Unblock => api.UnblockAsync(new BlockRequest(operation.E164), cancellationToken),
                _ => Task.CompletedTask,
            });

            return Outcome.Sent;
        }
        catch (ApiException exception)
        {
            return exception.StatusCode switch
            {
                HttpStatusCode.TooManyRequests or HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => Outcome.TryLater,
                >= HttpStatusCode.InternalServerError => Outcome.Retry,
                // The server will never accept it (invalid number, account not registered): sending again cannot help.
                _ => Outcome.Rejected,
            };
        }
        catch (Exception exception) when (exception is HttpRequestException or OperationCanceledException)
        {
            return Outcome.TryLater;
        }
    }

    private enum Outcome
    {
        Sent,
        Rejected,
        Retry,
        TryLater,
    }
}
