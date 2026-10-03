using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Tranqui.Domain.Reputation;

namespace Tranqui.Infrastructure.Reputation;

/// <summary>Contributor id = HMAC-SHA256(key, Firebase uid): stable per account, unlinkable without the key.</summary>
internal sealed class HmacContributorIdProvider(IOptions<ContributorIdOptions> options) : IContributorIdProvider
{
    private readonly byte[] key = Convert.FromBase64String(options.Value.Key);

    public ContributorId FromFirebaseUid(string firebaseUid)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firebaseUid);

        return new ContributorId(HMACSHA256.HashData(key, Encoding.UTF8.GetBytes(firebaseUid)));
    }
}
