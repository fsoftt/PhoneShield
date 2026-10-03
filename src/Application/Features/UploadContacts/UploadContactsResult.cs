namespace Tranqui.Application.Features.UploadContacts;

/// <summary>Accepted entries were stored (new or updated); skipped ones were invalid numbers or over the account cap.</summary>
public sealed record UploadContactsResult(int Accepted, int Skipped);
