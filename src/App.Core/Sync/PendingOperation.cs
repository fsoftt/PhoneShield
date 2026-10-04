using Tranqui.Contracts.Reports;

namespace Tranqui.App.Core.Sync;

/// <summary>A change made on the phone that the server has not received yet. The number stays on the device until sent.</summary>
public sealed record PendingOperation(
    Guid Id,
    PendingOperationKind Kind,
    string E164,
    ReportVerdictDto? Verdict,
    string? Label,
    DateTimeOffset CreatedAt,
    int Attempts = 0)
{
    /// <summary>Report/withdraw replace each other for the same number, and so do block/unblock: only the latest matters.</summary>
    public bool Supersedes(PendingOperation other)
    {
        ArgumentNullException.ThrowIfNull(other);

        return other.E164 == E164 && Family(other.Kind) == Family(Kind);
    }

    private static int Family(PendingOperationKind kind) =>
        kind is PendingOperationKind.Report or PendingOperationKind.WithdrawReport ? 0 : 1;
}
