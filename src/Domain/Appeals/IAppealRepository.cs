using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.Domain.Appeals;

public interface IAppealRepository
{
    Task<int> CountUsagesSinceAsync(QuotaSubject subject, AppealAction action, DateTimeOffset since, CancellationToken cancellationToken);

    void AddUsage(AppealQuotaUsage usage);

    void AddAppeal(Appeal appeal);

    Task<bool> IsHiddenAsync(PhoneHash phoneHash, CancellationToken cancellationToken);

    void Hide(HiddenNumber hiddenNumber);
}
