using Tranqui.Contracts.Lookups;
using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.App.Core.Calls;

public interface ICallerLookup
{
    /// <summary>Community data for the number, or null when it could not be obtained in time (offline, slow network).</summary>
    Task<LookupResponse?> LookupAsync(PhoneNumber number, CancellationToken cancellationToken);
}
