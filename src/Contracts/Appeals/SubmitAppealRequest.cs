namespace Tranqui.Contracts.Appeals;

public sealed record SubmitAppealRequest(AppealKindDto Kind, string? Reason, string? ContactEmail);
