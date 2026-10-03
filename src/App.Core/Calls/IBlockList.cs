using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.App.Core.Calls;

/// <summary>Numbers the user blocked. Kept only on the device; the server never learns them.</summary>
public interface IBlockList
{
    Task<bool> ContainsAsync(PhoneNumber number);

    Task AddAsync(PhoneNumber number);

    Task RemoveAsync(PhoneNumber number);

    Task<IReadOnlyList<PhoneNumber>> ListAsync();
}
