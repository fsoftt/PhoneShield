namespace Tranqui.Contracts.Lookups;

public sealed record LookupResponse(
    CallerStatusDto Status,
    string? DisplayName,
    IReadOnlyList<string> OtherNames,
    int SpamReportCount,
    int SavedByCount);
