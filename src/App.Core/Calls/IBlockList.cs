using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.App.Core.Calls;

/// <summary>
/// Numbers the user blocked, kept on the device. The server only learns a block (as a hash) when the user shares blocks
/// as a spam signal; see <see cref="BlockingService"/>.
/// </summary>
public interface IBlockList
{
    Task<bool> ContainsAsync(PhoneNumber number);

    Task AddAsync(PhoneNumber number);

    Task RemoveAsync(PhoneNumber number);

    Task<IReadOnlyList<PhoneNumber>> ListAsync();
}
