using MediatR;
using Tranqui.Application.Abstractions;
using Tranqui.Application.Errors;
using Tranqui.Domain.Abstractions;
using Tranqui.Domain.Legal;
using Tranqui.Domain.PhoneNumbers;
using Tranqui.Domain.Reputation;
using Tranqui.Domain.Users;

namespace Tranqui.Application.Features.UploadContacts;

internal sealed class UploadContactsHandler(
    ICurrentUser currentUser,
    IUserRepository users,
    IPhoneNumberHasher hasher,
    INameProtector nameProtector,
    IContributorIdProvider contributorIds,
    IContactContributionRepository contributions,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IRequestHandler<UploadContactsCommand, UploadContactsResult>
{
    public async Task<UploadContactsResult> Handle(UploadContactsCommand request, CancellationToken cancellationToken)
    {
        var user = await users.GetByFirebaseUidAsync(currentUser.FirebaseUid, cancellationToken)
            ?? throw new AccountNotRegisteredException();
        if (!user.HasActiveConsent(ConsentType.ContactUpload, LegalDocuments.CurrentContactUploadVersion))
        {
            throw new ConsentRequiredException();
        }

        var now = timeProvider.GetUtcNow();
        var contributor = contributorIds.FromFirebaseUid(user.FirebaseUid);
        var contacts = Normalize(request.Contacts);
        var existing = await contributions.GetForContributorAsync(
            contributor, contacts.Select(contact => contact.Hash.Value.ToArray()).ToList(), cancellationToken);
        var remainingCapacity = ContactUploadRules.MaxContactsPerAccount
            - await contributions.CountForContributorAsync(contributor, cancellationToken);

        var accepted = 0;
        foreach (var contact in contacts)
        {
            var name = Protect(contact);
            if (existing.TryGetValue(contact.Hash.ToHex(), out var contribution))
            {
                contribution.Rename(name, now);
                accepted++;
            }
            else if (remainingCapacity > 0)
            {
                contributions.Add(ContactContribution.Create(contact.Hash, contributor, name, now));
                remainingCapacity--;
                accepted++;
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new UploadContactsResult(accepted, request.Contacts.Count - accepted);
    }

    /// <summary>Drops invalid numbers and keeps one entry per number (the last one wins, like the device's own list).</summary>
    private List<NormalizedContact> Normalize(IEnumerable<ContactEntry> entries) =>
        entries
            .Select(entry => (Number: PhoneNumber.TryParse(entry.PhoneNumber), entry.Name))
            .Where(entry => entry.Number is not null)
            .GroupBy(entry => entry.Number!.E164)
            .Select(group => group.Last())
            .Select(entry => new NormalizedContact(entry.Number!, hasher.Hash(entry.Number!), entry.Name))
            .ToList();

    /// <summary>Personal ("Mamá") and offensive names are dropped: only the fact that someone saved the number is kept.</summary>
    private ProtectedName? Protect(NormalizedContact contact) =>
        CallerName.TryCreate(contact.RawName) is { } name && CallerNameFilter.IsShareable(name)
            ? nameProtector.Protect(contact.Number, name)
            : null;

    private sealed record NormalizedContact(PhoneNumber Number, PhoneHash Hash, string? RawName);
}
