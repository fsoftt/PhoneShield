using Tranqui.Domain.Reputation;

namespace Tranqui.Application.Features.LookupNumber;

public sealed record LookupResult(
    CallerStatus Status,
    string? DisplayName,
    IReadOnlyList<string> OtherNames,
    int SpamReportCount,
    int SavedByCount);
