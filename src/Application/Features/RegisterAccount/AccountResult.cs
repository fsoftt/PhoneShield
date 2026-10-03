namespace Tranqui.Application.Features.RegisterAccount;

public sealed record AccountResult(Guid Id, DateTimeOffset CreatedAt, string? AcceptedTermsVersion);
