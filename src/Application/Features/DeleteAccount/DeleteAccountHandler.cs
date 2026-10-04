using MediatR;
using Tranqui.Application.Abstractions;
using Tranqui.Domain.Abstractions;
using Tranqui.Domain.Reputation;
using Tranqui.Domain.Users;

namespace Tranqui.Application.Features.DeleteAccount;

internal sealed class DeleteAccountHandler(
    ICurrentUser currentUser,
    IUserRepository users,
    IContributorIdProvider contributorIds,
    ISpamReportRepository reports,
    IContactContributionRepository contributions,
    IBlockSignalRepository blocks,
    IContributorReputationRepository reputations,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteAccountCommand>
{
    public async Task Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        var contributor = contributorIds.FromFirebaseUid(currentUser.FirebaseUid);
        await reports.RemoveAllForContributorAsync(contributor, cancellationToken);
        await contributions.RemoveAllForContributorAsync(contributor, cancellationToken);
        await blocks.RemoveAllForContributorAsync(contributor, cancellationToken);
        await reputations.RemoveAsync(contributor, cancellationToken);

        var user = await users.GetByFirebaseUidAsync(currentUser.FirebaseUid, cancellationToken);
        if (user is not null)
        {
            users.Remove(user);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
