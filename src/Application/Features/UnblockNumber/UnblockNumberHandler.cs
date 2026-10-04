using MediatR;
using Tranqui.Application.Abstractions;
using Tranqui.Domain.Abstractions;
using Tranqui.Domain.PhoneNumbers;
using Tranqui.Domain.Reputation;

namespace Tranqui.Application.Features.UnblockNumber;

internal sealed class UnblockNumberHandler(
    ICurrentUser currentUser,
    IPhoneNumberHasher hasher,
    IContributorIdProvider contributorIds,
    IBlockSignalRepository blocks,
    IUnitOfWork unitOfWork) : IRequestHandler<UnblockNumberCommand>
{
    public async Task Handle(UnblockNumberCommand request, CancellationToken cancellationToken)
    {
        var phoneNumber = PhoneNumber.TryParse(request.PhoneNumber)
            ?? throw new InvalidOperationException("The validator guarantees a valid phone number.");
        var block = await blocks.GetAsync(
            hasher.Hash(phoneNumber), contributorIds.FromFirebaseUid(currentUser.FirebaseUid), cancellationToken);
        if (block is null)
        {
            return;
        }

        blocks.Remove(block);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
