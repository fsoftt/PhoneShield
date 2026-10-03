using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.Domain.Appeals;

public interface IAppealRepository
{
    Task<int> CountVerificationsSinceAsync(PhoneHash phoneHash, DateTimeOffset since, CancellationToken cancellationToken);

    void AddVerification(SmsVerificationRequest request);

    Task<int> CountAppealsSinceAsync(PhoneHash phoneHash, DateTimeOffset since, CancellationToken cancellationToken);

    void AddAppeal(Appeal appeal);

    Task<bool> IsHiddenAsync(PhoneHash phoneHash, CancellationToken cancellationToken);

    void Hide(HiddenNumber hiddenNumber);
}
