using MediatR;
using Tranqui.Application.Abstractions;
using Tranqui.Application.Errors;
using Tranqui.Domain.Abstractions;
using Tranqui.Domain.Appeals;
using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.Application.Features.SubmitAppeal;

internal sealed class SubmitAppealHandler(
    IVerifiedPhone verifiedPhone,
    IPhoneNumberHasher hasher,
    IAppealRepository appeals,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IRequestHandler<SubmitAppealCommand, AppealStatus>
{
    public async Task<AppealStatus> Handle(SubmitAppealCommand request, CancellationToken cancellationToken)
    {
        var phoneNumber = PhoneNumber.TryParse(verifiedPhone.E164)
            ?? throw new InvalidOperationException("Firebase verified a number that cannot be parsed.");
        var phoneHash = hasher.Hash(phoneNumber);
        var now = timeProvider.GetUtcNow();

        var recent = await appeals.CountAppealsSinceAsync(phoneHash, now - AppealRules.LimitWindow, cancellationToken);
        if (recent >= AppealRules.AppealsPerWindow)
        {
            throw new AppealLimitReachedException();
        }

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
