using Tranqui.Domain.Retention;

namespace Tranqui.Application.Features.PurgeExpiredData;

/// <summary>Rows strictly older than these instants are deleted.</summary>
public sealed record PurgeCutoffs(DateTimeOffset AppealQuotaUsages, DateTimeOffset ResolvedAppeals, DateTimeOffset Votes)
{
    public static PurgeCutoffs At(DateTimeOffset now) => new(
        now - RetentionRules.AppealQuotaUsages,
        now - RetentionRules.ResolvedAppeals,
        now - RetentionRules.Votes);
}
