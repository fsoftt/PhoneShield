using Tranqui.Contracts.Appeals;
using Tranqui.Contracts.Lookups;

namespace Tranqui.Contracts.BackOffice;

public sealed record AppealReviewResponse(
    Guid Id,
    AppealKindDto Kind,
    AppealStatusDto Status,
    string? Reason,
    string? ContactEmail,
    DateTimeOffset CreatedAt,
    DateTimeOffset DueAt,
    DateTimeOffset? ResolvedAt,
    bool IsOverdue,
    CallerStatusDto NumberStatus,
    int SpamReportCount,
    int NotSpamReportCount,
    int SavedByCount);
