namespace Tranqui.Contracts.Appeals;

/// <summary>The normalized number (E.164) the website must verify by SMS.</summary>
public sealed record AppealVerificationResponse(string PhoneNumber);
