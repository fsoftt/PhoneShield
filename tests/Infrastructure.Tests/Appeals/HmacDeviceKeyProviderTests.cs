using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Tranqui.Domain.Appeals;
using Tranqui.Infrastructure.Appeals;
using Tranqui.Infrastructure.Reputation;

namespace Tranqui.Infrastructure.Tests.Appeals;

public sealed class HmacDeviceKeyProviderTests
{
    private static readonly byte[] key = RandomNumberGenerator.GetBytes(32);
    private static readonly HmacDeviceKeyProvider provider =
        new(Options.Create(new ContributorIdOptions { Key = Convert.ToBase64String(key) }));

    [Fact]
    public void FromDeviceId_SameDevice_SameKey()
    {
        var first = provider.FromDeviceId(DeviceId.TryParse("9774D56D682E549C")!);
        var second = provider.FromDeviceId(DeviceId.TryParse("9774d56d682e549c")!);

        first.ToArray().Should().Equal(second.ToArray());
    }

    [Fact]
    public void FromDeviceId_DiffersFromAContributorIdOfTheSameText()
    {
        var device = provider.FromDeviceId(DeviceId.TryParse("abc123")!);

        device.ToArray().Should().NotEqual(HMACSHA256.HashData(key, Encoding.UTF8.GetBytes("abc123")));
    }
}
