namespace Tranqui.Domain.Appeals;

/// <summary>Who an appeal quota is counted against: a number, an account or a device, by keyed hash only.</summary>
public sealed record QuotaSubject(QuotaSubjectKind Kind, ReadOnlyMemory<byte> Key);
