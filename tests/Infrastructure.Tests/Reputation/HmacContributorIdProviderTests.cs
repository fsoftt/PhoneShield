using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Tranqui.Infrastructure.Reputation;

namespace Tranqui.Infrastructure.Tests.Reputation;

public sealed class HmacContributorIdProviderTests
{
    [Fact]
    public void FromFirebaseUid_SameUid_IsStable()
    {
        var provider = CreateProvider();

        provider.FromFirebaseUid("uid-1").Should().Be(provider.FromFirebaseUid("uid-1"));
    }

    [Fact]
    public void FromFirebaseUid_DifferentUids_Differ()
    {
        var provider = CreateProvider();

        provider.FromFirebaseUid("uid-1").Should().NotBe(provider.FromFirebaseUid("uid-2"));
    }

    [Fact]
    public void FromFirebaseUid_DifferentKeys_Differ()
    {
        CreateProvider().FromFirebaseUid("uid-1").Should().NotBe(CreateProvider().FromFirebaseUid("uid-1"));
    }

    private static HmacContributorIdProvider CreateProvider() =>
        new(Options.Create(new ContributorIdOptions { Key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)) }));
}
