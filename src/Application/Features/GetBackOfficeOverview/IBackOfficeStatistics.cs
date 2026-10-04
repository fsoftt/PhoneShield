namespace Tranqui.Application.Features.GetBackOfficeOverview;

public interface IBackOfficeStatistics
{
    Task<BackOfficeTotals> ReadAsync(CancellationToken cancellationToken);
}
