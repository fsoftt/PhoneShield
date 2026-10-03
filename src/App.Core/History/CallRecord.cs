using Tranqui.App.Core.Calls;
using Tranqui.Contracts.Reports;

namespace Tranqui.App.Core.History;

/// <summary>One screened call, kept only on the phone. <see cref="MyVerdict"/> is what the user reported, if anything.</summary>
public sealed record CallRecord(
    Guid Id,
    string? E164,
    string DisplayNumber,
    string Title,
    CallerCardState State,
    BlockReason? BlockReason,
    DateTimeOffset OccurredAt,
    ReportVerdictDto? MyVerdict = null);
