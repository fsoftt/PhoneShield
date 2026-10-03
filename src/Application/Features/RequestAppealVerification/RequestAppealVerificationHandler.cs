using MediatR;
using Tranqui.Application.Errors;
using Tranqui.Domain.Abstractions;
using Tranqui.Domain.Appeals;
using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.Application.Features.RequestAppealVerification;

internal sealed class RequestAppealVerificationHandler(
    IPhoneNumberHasher hasher,
    IAppealRepository appeals,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IRequestHandler<RequestAppealVerificationCommand, string>
{
    public async Task<string> Handle(RequestAppealVerificationCommand request, CancellationToken cancellationToken)
    {
        var phoneNumber = PhoneNumber.TryParse(request.PhoneNumber)
            ?? throw new InvalidOperationException("The validator guarantees a valid phone number.");
        var phoneHash = hasher.Hash(phoneNumber);
        var now = timeProvider.GetUtcNow();

        var recent = await appeals.CountVerificationsSinceAsync(phoneHash, now - AppealRules.LimitWindow, cancellationToken);
        if (recent >= AppealRules.VerificationsPerWindow)
        {
            throw new AppealLimitReachedException();
        }

        appeals.AddVerification(SmsVerificationRequest.Create(phoneHash, now));
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return phoneNumber.E164;
    }
}
