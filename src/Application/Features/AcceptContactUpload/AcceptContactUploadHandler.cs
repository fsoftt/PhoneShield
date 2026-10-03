using MediatR;
using Tranqui.Application.Abstractions;
using Tranqui.Application.Errors;
using Tranqui.Domain.Abstractions;
using Tranqui.Domain.Users;

namespace Tranqui.Application.Features.AcceptContactUpload;

internal sealed class AcceptContactUploadHandler(
    ICurrentUser currentUser,
    IUserRepository users,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IRequestHandler<AcceptContactUploadCommand>
{
    public async Task Handle(AcceptContactUploadCommand request, CancellationToken cancellationToken)
    {
        var user = await users.GetByFirebaseUidAsync(currentUser.FirebaseUid, cancellationToken)
            ?? throw new AccountNotRegisteredException();

        user.AcceptContactUpload(request.AcceptedVersion, timeProvider.GetUtcNow());
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
