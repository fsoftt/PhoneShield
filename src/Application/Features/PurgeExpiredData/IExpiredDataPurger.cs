namespace Tranqui.Application.Features.PurgeExpiredData;

/// <summary>Deletes, in bulk, the rows older than the given cut-offs.</summary>
public interface IExpiredDataPurger
{
    Task<PurgeResult> PurgeAsync(PurgeCutoffs cutoffs, CancellationToken cancellationToken);
}
