using MediatR;
using Tranqui.Application.Abstractions;
using Tranqui.Domain.Abstractions;
using Tranqui.Domain.Users;

namespace Tranqui.Application.Features.RegisterAccount;

internal sealed class RegisterAccountHandler(
    ICurrentUser currentUser,
    IUserRepository users,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IRequestHandler<RegisterAccountCommand, AccountResult>
{
    public async Task<AccountResult> Handle(RegisterAccountCommand request, CancellationToken cancellationToken)
    {
        var user = await users.GetByFirebaseUidAsync(currentUser.FirebaseUid, cancellationToken);

        if (user is null)
        {
            user = User.Register(currentUser.FirebaseUid, request.AcceptedTermsVersion, timeProvider.GetUtcNow());
            users.Add(user);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return new AccountResult(user.Id, user.CreatedAt, user.AcceptedVersionOf(ConsentType.TermsAndPrivacyPolicy));
    }
}
