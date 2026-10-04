using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.Domain.Appeals;

public interface IAppealRepository
{
    Task<int> CountUsagesSinceAsync(QuotaSubject subject, AppealAction action, DateTimeOffset since, CancellationToken cancellationToken);

    void AddUsage(AppealQuotaUsage usage);

    void AddAppeal(Appeal appeal);

    Task<Appeal?> GetAppealAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Appeal>> ListAppealsAsync(AppealStatus status, int limit, CancellationToken cancellationToken);

    Task<bool> IsHiddenAsync(PhoneHash phoneHash, CancellationToken cancellationToken);

    void Hide(HiddenNumber hiddenNumber);

    Task<ClearedNumber?> GetClearedAsync(PhoneHash phoneHash, CancellationToken cancellationToken);

    void Clear(ClearedNumber clearedNumber);
}
