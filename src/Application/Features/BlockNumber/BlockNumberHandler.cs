using MediatR;
using Tranqui.Application.Abstractions;
using Tranqui.Application.Errors;
using Tranqui.Domain.Abstractions;
using Tranqui.Domain.PhoneNumbers;
using Tranqui.Domain.Reputation;
using Tranqui.Domain.Users;

namespace Tranqui.Application.Features.BlockNumber;

internal sealed class BlockNumberHandler(
    ICurrentUser currentUser,
    IUserRepository users,
    IPhoneNumberHasher hasher,
    IContributorIdProvider contributorIds,
    IBlockSignalRepository blocks,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IRequestHandler<BlockNumberCommand>
{
    public async Task Handle(BlockNumberCommand request, CancellationToken cancellationToken)
    {
        var user = await users.GetByFirebaseUidAsync(currentUser.FirebaseUid, cancellationToken)
            ?? throw new AccountNotRegisteredException();
        var phoneNumber = PhoneNumber.TryParse(request.PhoneNumber)
            ?? throw new InvalidOperationException("The validator guarantees a valid phone number.");
        var phoneHash = hasher.Hash(phoneNumber);
        var contributor = contributorIds.FromFirebaseUid(user.FirebaseUid);

        if (await blocks.GetAsync(phoneHash, contributor, cancellationToken) is not null)
        {
            return;
        }

        var now = timeProvider.GetUtcNow();
        blocks.Add(BlockSignal.Create(phoneHash, contributor, ReputationRules.ReporterWeight(now - user.CreatedAt), now));
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
