using MediatR;
using Tranqui.Application.Abstractions;
using Tranqui.Application.Errors;
using Tranqui.Domain.Abstractions;
using Tranqui.Domain.Reputation;
using Tranqui.Domain.Users;

namespace Tranqui.Application.Features.WithdrawContacts;

internal sealed class WithdrawContactsHandler(
    ICurrentUser currentUser,
    IUserRepository users,
    IContributorIdProvider contributorIds,
    IContactContributionRepository contributions,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IRequestHandler<WithdrawContactsCommand, int>
{
    public async Task<int> Handle(WithdrawContactsCommand request, CancellationToken cancellationToken)
    {
        var user = await users.GetByFirebaseUidAsync(currentUser.FirebaseUid, cancellationToken)
            ?? throw new AccountNotRegisteredException();

        user.RevokeContactUpload(timeProvider.GetUtcNow());
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await contributions.RemoveAllForContributorAsync(contributorIds.FromFirebaseUid(user.FirebaseUid), cancellationToken);
    }
}
