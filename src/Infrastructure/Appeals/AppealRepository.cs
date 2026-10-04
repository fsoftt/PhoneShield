using Microsoft.EntityFrameworkCore;
using Tranqui.Domain.Appeals;
using Tranqui.Domain.PhoneNumbers;
using Tranqui.Infrastructure.Persistence;

namespace Tranqui.Infrastructure.Appeals;

internal sealed class AppealRepository(TranquiDbContext dbContext) : IAppealRepository
{
    public Task<int> CountUsagesSinceAsync(
        QuotaSubject subject,
        AppealAction action,
        DateTimeOffset since,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(subject);
        var key = subject.Key.ToArray();

        return dbContext.AppealQuotaUsages.CountAsync(
            usage => usage.SubjectKind == subject.Kind
                && usage.SubjectKey == key
                && usage.Action == action
                && usage.UsedAt >= since,
            cancellationToken);
    }

    public void AddUsage(AppealQuotaUsage usage) => dbContext.AppealQuotaUsages.Add(usage);

    public void AddAppeal(Appeal appeal) => dbContext.Appeals.Add(appeal);

    public Task<Appeal?> GetAppealAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Appeals.FirstOrDefaultAsync(appeal => appeal.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Appeal>> ListAppealsAsync(AppealStatus status, int limit, CancellationToken cancellationToken) =>
        await dbContext.Appeals.AsNoTracking()
            .Where(appeal => appeal.Status == status)
            .OrderBy(appeal => appeal.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);

    public Task<bool> IsHiddenAsync(PhoneHash phoneHash, CancellationToken cancellationToken)
    {
        var hash = phoneHash.Value.ToArray();

        return dbContext.HiddenNumbers.AnyAsync(hidden => hidden.PhoneHash == hash, cancellationToken);
    }

    public void Hide(HiddenNumber hiddenNumber) => dbContext.HiddenNumbers.Add(hiddenNumber);

    public Task<ClearedNumber?> GetClearedAsync(PhoneHash phoneHash, CancellationToken cancellationToken)
    {
        var hash = phoneHash.Value.ToArray();

        return dbContext.ClearedNumbers.FirstOrDefaultAsync(cleared => cleared.PhoneHash == hash, cancellationToken);
    }

    public void Clear(ClearedNumber clearedNumber) => dbContext.ClearedNumbers.Add(clearedNumber);
}
