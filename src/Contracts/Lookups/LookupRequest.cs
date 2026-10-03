namespace Tranqui.Contracts.Lookups;

/// <summary>Sent in the body, never in the URL, so the number cannot end up in access logs.</summary>
public sealed record LookupRequest(string PhoneNumber);
