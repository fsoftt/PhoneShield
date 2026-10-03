using Microsoft.EntityFrameworkCore;
using Tranqui.Domain.Appeals;
using Tranqui.Domain.PhoneNumbers;
using Tranqui.Infrastructure.Persistence;

namespace Tranqui.Infrastructure.Appeals;

internal sealed class AppealRepository(TranquiDbContext dbContext) : IAppealRepository
{
    public Task<int> CountVerificationsSinceAsync(PhoneHash phoneHash, DateTimeOffset since, CancellationToken cancellationToken)
    {
        var hash = phoneHash.Value.ToArray();

        return dbContext.SmsVerificationRequests.CountAsync(
            request => request.PhoneHash == hash && request.RequestedAt >= since, cancellationToken);
    }

    public void AddVerification(SmsVerificationRequest request) => dbContext.SmsVerificationRequests.Add(request);

    public Task<int> CountAppealsSinceAsync(PhoneHash phoneHash, DateTimeOffset since, CancellationToken cancellationToken)
    {
        var hash = phoneHash.Value.ToArray();

        return dbContext.Appeals.CountAsync(appeal => appeal.PhoneHash == hash && appeal.CreatedAt >= since, cancellationToken);
    }

    public void AddAppeal(Appeal appeal) => dbContext.Appeals.Add(appeal);

    public Task<bool> IsHiddenAsync(PhoneHash phoneHash, CancellationToken cancellationToken)
    {
        var hash = phoneHash.Value.ToArray();

        return dbContext.HiddenNumbers.AnyAsync(hidden => hidden.PhoneHash == hash, cancellationToken);
    }

    public void Hide(HiddenNumber hiddenNumber) => dbContext.HiddenNumbers.Add(hiddenNumber);
}
