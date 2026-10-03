using MediatR;
using Tranqui.Application.Appeals;
using Tranqui.Domain.Abstractions;
using Tranqui.Domain.Appeals;
using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.Application.Features.RequestAppealVerification;

internal sealed class RequestAppealVerificationHandler(AppealGate gate, IUnitOfWork unitOfWork)
    : IRequestHandler<RequestAppealVerificationCommand, string>
{
    public async Task<string> Handle(RequestAppealVerificationCommand request, CancellationToken cancellationToken)
    {
        var phoneNumber = PhoneNumber.TryParse(request.PhoneNumber)
            ?? throw new InvalidOperationException("The validator guarantees a valid phone number.");
        var deviceId = DeviceId.TryParse(request.DeviceId)
            ?? throw new InvalidOperationException("The validator guarantees a valid device id.");

        await gate.ConsumeAsync(AppealAction.SmsVerification, phoneNumber, deviceId, request.IntegrityToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return phoneNumber.E164;
    }
}
