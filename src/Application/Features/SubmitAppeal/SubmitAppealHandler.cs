using MediatR;
using Tranqui.Application.Abstractions;
using Tranqui.Application.Appeals;
using Tranqui.Application.Errors;
using Tranqui.Domain.Abstractions;
using Tranqui.Domain.Appeals;
using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.Application.Features.SubmitAppeal;

internal sealed class SubmitAppealHandler(
    IPhoneProofValidator phoneProofs,
    AppealGate gate,
    IAppealRepository appeals,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IRequestHandler<SubmitAppealCommand, AppealStatus>
{
    public async Task<AppealStatus> Handle(SubmitAppealCommand request, CancellationToken cancellationToken)
    {
        var phoneNumber = PhoneNumber.TryParse(await phoneProofs.VerifiedNumberAsync(request.PhoneProof, cancellationToken))
            ?? throw new PhoneNotVerifiedException();
        var deviceId = DeviceId.TryParse(request.DeviceId)
            ?? throw new InvalidOperationException("The validator guarantees a valid device id.");

        var phoneHash = await gate.ConsumeAsync(AppealAction.Appeal, phoneNumber, deviceId, request.IntegrityToken, cancellationToken);

        var now = timeProvider.GetUtcNow();
        var appeal = Appeal.File(phoneHash, request.Kind, request.Reason, request.ContactEmail, now);
        appeals.AddAppeal(appeal);

        if (request.Kind == AppealKind.HideNames && !await appeals.IsHiddenAsync(phoneHash, cancellationToken))
        {
            appeals.Hide(HiddenNumber.Create(phoneHash, now));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return appeal.Status;
    }
}
