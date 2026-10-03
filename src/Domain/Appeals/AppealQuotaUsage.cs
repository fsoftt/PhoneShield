namespace Tranqui.Domain.Appeals;

/// <summary>
/// One use of an appeal quota by a number, an account or a device. Each subject gets its own row with only its keyed
/// hash, so the rows never record which account or device acted for which number.
/// </summary>
public sealed class AppealQuotaUsage
{
    private AppealQuotaUsage()
    {
        SubjectKey = [];
    }

    private AppealQuotaUsage(QuotaSubjectKind subjectKind, byte[] subjectKey, AppealAction action, DateTimeOffset usedAt)
    {
        Id = Guid.CreateVersion7(usedAt);
        SubjectKind = subjectKind;
        SubjectKey = subjectKey;
        Action = action;
        UsedAt = usedAt;
    }

    public Guid Id { get; private set; }

    public QuotaSubjectKind SubjectKind { get; private set; }

    public byte[] SubjectKey { get; private set; }

    public AppealAction Action { get; private set; }

    public DateTimeOffset UsedAt { get; private set; }

    public static AppealQuotaUsage Record(QuotaSubject subject, AppealAction action, DateTimeOffset usedAt)
    {
        ArgumentNullException.ThrowIfNull(subject);

        return new AppealQuotaUsage(subject.Kind, subject.Key.ToArray(), action, usedAt);
    }
}
