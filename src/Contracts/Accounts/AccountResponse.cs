namespace Tranqui.Contracts.Accounts;

public sealed record AccountResponse(Guid Id, DateTimeOffset CreatedAt, string? AcceptedTermsVersion);
