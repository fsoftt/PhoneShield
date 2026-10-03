using System.Security.Cryptography;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Tranqui.Application.Abstractions;
using Tranqui.Application.Errors;
using Tranqui.Application.Features.UploadContacts;
using Tranqui.Domain.Abstractions;
using Tranqui.Domain.Legal;
using Tranqui.Domain.PhoneNumbers;
using Tranqui.Domain.Reputation;
using Tranqui.Domain.Users;

namespace Tranqui.Application.Tests.Features.UploadContacts;

public sealed class UploadContactsHandlerTests
{
    private const string FirebaseUid = "firebase-uid-123";

    private static readonly DateTimeOffset now = new(2026, 10, 3, 12, 0, 0, TimeSpan.Zero);
    private static readonly ContributorId contributor = new(RandomNumberGenerator.GetBytes(ContributorId.SizeInBytes));
    private static readonly ProtectedName protectedName = new([1], [1]);

    private readonly IUserRepository users = Substitute.For<IUserRepository>();
    private readonly INameProtector nameProtector = Substitute.For<INameProtector>();
    private readonly IContactContributionRepository contributions = Substitute.For<IContactContributionRepository>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly Dictionary<string, PhoneHash> hashes = [];
    private readonly UploadContactsHandler handler;

    public UploadContactsHandlerTests()
    {
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.FirebaseUid.Returns(FirebaseUid);
        var hasher = Substitute.For<IPhoneNumberHasher>();
        hasher.Hash(Arg.Any<PhoneNumber>()).Returns(call => HashOf(call.Arg<PhoneNumber>().E164));
        var contributorIds = Substitute.For<IContributorIdProvider>();
        contributorIds.FromFirebaseUid(FirebaseUid).Returns(contributor);
        nameProtector.Protect(Arg.Any<PhoneNumber>(), Arg.Any<CallerName>()).Returns(protectedName);
        contributions.GetForContributorAsync(contributor, Arg.Any<IReadOnlyCollection<byte[]>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, ContactContribution>());

        handler = new UploadContactsHandler(
            currentUser, users, hasher, nameProtector, contributorIds, contributions, unitOfWork, new FakeTimeProvider(now));
    }

    [Fact]
    public async Task Handle_WithoutContactUploadConsent_Throws()
    {
        GivenUser(withContactConsent: false);

        var act = () => handler.Handle(Command(("3001234567", "Pizzería Juan")), CancellationToken.None);

        await act.Should().ThrowAsync<ConsentRequiredException>();
    }

    [Fact]
    public async Task Handle_CallerName_IsProtectedAndStored()
    {
        GivenUser();

        var result = await handler.Handle(Command(("3001234567", "Pizzería Juan")), CancellationToken.None);

        result.Should().Be(new UploadContactsResult(1, 0));
        contributions.Received(1).Add(Arg.Is<ContactContribution>(contribution => contribution.Name == protectedName));
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PersonalName_StoresTheNumberWithoutTheName()
    {
        GivenUser();

        await handler.Handle(Command(("3001234567", "Mamá")), CancellationToken.None);

        nameProtector.DidNotReceive().Protect(Arg.Any<PhoneNumber>(), Arg.Any<CallerName>());
        contributions.Received(1).Add(Arg.Is<ContactContribution>(contribution => contribution.Name == null));
    }

    [Fact]
    public async Task Handle_InvalidAndDuplicateNumbers_AreSkipped()
    {
        GivenUser();

        var result = await handler.Handle(
            Command(("123", "Nadie"), ("300 123 4567", "Juan"), ("+573001234567", "Juan Pérez")),
            CancellationToken.None);

        result.Should().Be(new UploadContactsResult(1, 2));
        nameProtector.Received(1).Protect(Arg.Any<PhoneNumber>(), Arg.Is<CallerName>(name => name.DisplayValue == "Juan Pérez"));
    }

    [Fact]
    public async Task Handle_AlreadyContributedNumber_IsRenamedNotDuplicated()
    {
        GivenUser();
        var existing = ContactContribution.Create(HashOf("+573001234567"), contributor, null, now.AddDays(-1));
        contributions.GetForContributorAsync(contributor, Arg.Any<IReadOnlyCollection<byte[]>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, ContactContribution> { [HashOf("+573001234567").ToHex()] = existing });

        await handler.Handle(Command(("3001234567", "Pizzería Juan")), CancellationToken.None);

        contributions.DidNotReceive().Add(Arg.Any<ContactContribution>());
        existing.Name.Should().Be(protectedName);
    }

    [Fact]
    public async Task Handle_AccountAtCapacity_SkipsNewNumbers()
    {
        GivenUser();
        contributions.CountForContributorAsync(contributor, Arg.Any<CancellationToken>())
            .Returns(ContactUploadRules.MaxContactsPerAccount);

        var result = await handler.Handle(Command(("3001234567", "Pizzería Juan")), CancellationToken.None);

        result.Should().Be(new UploadContactsResult(0, 1));
        contributions.DidNotReceive().Add(Arg.Any<ContactContribution>());
    }

    private static UploadContactsCommand Command(params (string Number, string? Name)[] contacts) =>
        new(contacts.Select(contact => new ContactEntry(contact.Number, contact.Name)).ToList());

    private PhoneHash HashOf(string e164)
    {
        if (!hashes.TryGetValue(e164, out var hash))
        {
            hash = new PhoneHash(RandomNumberGenerator.GetBytes(PhoneHash.SizeInBytes), 1);
            hashes[e164] = hash;
        }

        return hash;
    }

    private void GivenUser(bool withContactConsent = true)
    {
        var user = User.Register(FirebaseUid, LegalDocuments.CurrentTermsVersion, now.AddDays(-10));
        if (withContactConsent)
        {
            user.AcceptContactUpload(LegalDocuments.CurrentContactUploadVersion, now.AddDays(-10));
        }

        users.GetByFirebaseUidAsync(FirebaseUid, Arg.Any<CancellationToken>()).Returns(user);
    }
}
