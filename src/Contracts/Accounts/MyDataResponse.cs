namespace Tranqui.Contracts.Accounts;

public sealed record MyDataResponse(
    Guid AccountId,
    DateTimeOffset CreatedAt,
    IReadOnlyList<ConsentResponse> Consents,
    int SpamReportCount,
    int ContactContributionCount);

public sealed record ConsentResponse(string Type, string Version, DateTimeOffset AcceptedAt, DateTimeOffset? RevokedAt);
