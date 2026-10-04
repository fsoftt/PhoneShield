using Tranqui.Domain.Reputation;

namespace Tranqui.Application.Features.RecalculateReporterReputation;

/// <summary>
/// Every number that has reports, with its signals at plain weights (no reputation applied, to avoid feedback loops)
/// and each spam vote tagged with its contributor.
/// </summary>
public interface IReporterEvidenceReader
{
    IAsyncEnumerable<ReputationSignals> ReadAllAsync(CancellationToken cancellationToken);
}
