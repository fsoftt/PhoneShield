using Tranqui.Domain.PhoneNumbers;
using Tranqui.Domain.Reputation;

namespace Tranqui.Application.Features.LookupNumber;

/// <summary>Read-side projection of every contribution about one number.</summary>
public interface IReputationSignalsReader
{
    Task<ReputationSignals> ReadAsync(PhoneHash phoneHash, CancellationToken cancellationToken);
}
