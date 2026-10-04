using Tranqui.Domain.Appeals;
using Tranqui.Domain.Reputation;

namespace Tranqui.Application.Features.ListAppeals;

/// <summary>
/// What a reviewer sees. The number itself is never available (only its hash is stored), so the decision rests on the
/// owner's reason and the aggregated signals.
/// </summary>
public sealed record AppealReview(
    Guid Id,
    AppealKind Kind,
    AppealStatus Status,
    string? Reason,
    string? ContactEmail,
    DateTimeOffset CreatedAt,
    DateTimeOffset DueAt,
    DateTimeOffset? ResolvedAt,
    bool IsOverdue,
    CallerStatus NumberStatus,
    int SpamReportCount,
    int NotSpamReportCount,
    int SavedByCount);
