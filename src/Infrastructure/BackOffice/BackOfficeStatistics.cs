using Microsoft.EntityFrameworkCore;
using Tranqui.Application.Features.GetBackOfficeOverview;
using Tranqui.Infrastructure.Persistence;

namespace Tranqui.Infrastructure.BackOffice;

internal sealed class BackOfficeStatistics(TranquiDbContext dbContext) : IBackOfficeStatistics
{
    public async Task<BackOfficeTotals> ReadAsync(CancellationToken cancellationToken) => new(
        await dbContext.Users.CountAsync(cancellationToken),
        await dbContext.SpamReports.CountAsync(cancellationToken),
        await dbContext.ContactContributions.CountAsync(cancellationToken),
        await dbContext.HiddenNumbers.CountAsync(cancellationToken),
        await dbContext.ClearedNumbers.CountAsync(cancellationToken));
}
