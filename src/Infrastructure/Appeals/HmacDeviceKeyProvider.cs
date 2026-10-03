using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Tranqui.Domain.Appeals;
using Tranqui.Infrastructure.Reputation;

namespace Tranqui.Infrastructure.Appeals;

/// <summary>
/// Device key = HMAC-SHA256(contributor key, "device:" + device id). The prefix keeps device keys apart from
/// contributor ids made with the same key.
/// </summary>
internal sealed class HmacDeviceKeyProvider(IOptions<ContributorIdOptions> options) : IDeviceKeyProvider
{
    private const string Domain = "device:";

    private readonly byte[] key = Convert.FromBase64String(options.Value.Key);

    public ReadOnlyMemory<byte> FromDeviceId(DeviceId deviceId)
    {
        ArgumentNullException.ThrowIfNull(deviceId);

        return HMACSHA256.HashData(key, Encoding.UTF8.GetBytes(Domain + deviceId.Value));
    }
}
