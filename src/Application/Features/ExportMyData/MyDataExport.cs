namespace Tranqui.Application.Features.ExportMyData;

public sealed record MyDataExport(
    Guid AccountId,
    DateTimeOffset CreatedAt,
    IReadOnlyList<ConsentExport> Consents,
    int SpamReportCount,
    int ContactContributionCount);

public sealed record ConsentExport(string Type, string Version, DateTimeOffset AcceptedAt, DateTimeOffset? RevokedAt);
