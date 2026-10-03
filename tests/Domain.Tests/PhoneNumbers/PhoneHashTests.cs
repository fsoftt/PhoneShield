using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.Domain.Tests.PhoneNumbers;

public sealed class PhoneHashTests
{
    private static readonly byte[] sampleBytes = Enumerable.Range(0, PhoneHash.SizeInBytes).Select(i => (byte)i).ToArray();

    [Fact]
    public void Equals_SameBytesAndVersion_AreEqual()
    {
        new PhoneHash(sampleBytes, 1).Should().Be(new PhoneHash(sampleBytes.ToArray(), 1));
    }

    [Fact]
    public void Equals_DifferentKeyVersion_AreNotEqual()
    {
        new PhoneHash(sampleBytes, 1).Should().NotBe(new PhoneHash(sampleBytes, 2));
    }

    [Fact]
    public void Constructor_WrongLength_Throws()
    {
        var act = () => new PhoneHash(new byte[PhoneHash.SizeInBytes - 1], 1);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_KeyVersionBelowOne_Throws()
    {
        var act = () => new PhoneHash(sampleBytes, 0);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Constructor_CopiesInput_SoLaterChangesDoNotAffectIt()
    {
        var bytes = sampleBytes.ToArray();
        var hash = new PhoneHash(bytes, 1);

        bytes[0] = byte.MaxValue;

        hash.Value[0].Should().Be(sampleBytes[0]);
    }
}
