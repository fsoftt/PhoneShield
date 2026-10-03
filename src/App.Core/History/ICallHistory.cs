using Tranqui.Contracts.Reports;

namespace Tranqui.App.Core.History;

/// <summary>Device-only call history. Implementations keep at most <see cref="CallHistoryRules"/> entries and age.</summary>
public interface ICallHistory
{
    Task AddAsync(CallRecord record);

    Task<IReadOnlyList<CallRecord>> ListAsync();

    Task SetVerdictAsync(Guid id, ReportVerdictDto verdict);

    Task ClearAsync();
}
