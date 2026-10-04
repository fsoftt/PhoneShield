using MediatR;
using Tranqui.Application.Abstractions;
using Tranqui.Domain.Abstractions;
using Tranqui.Domain.PhoneNumbers;
using Tranqui.Domain.Reputation;

namespace Tranqui.Application.Features.WithdrawReport;

internal sealed class WithdrawReportHandler(
    ICurrentUser currentUser,
    IPhoneNumberHasher hasher,
    IContributorIdProvider contributorIds,
    ISpamReportRepository reports,
    IUnitOfWork unitOfWork) : IRequestHandler<WithdrawReportCommand>
{
    public async Task Handle(WithdrawReportCommand request, CancellationToken cancellationToken)
    {
        var phoneNumber = PhoneNumber.TryParse(request.PhoneNumber)
            ?? throw new InvalidOperationException("The validator guarantees a valid phone number.");
        var report = await reports.GetAsync(
            hasher.Hash(phoneNumber), contributorIds.FromFirebaseUid(currentUser.FirebaseUid), cancellationToken);
        if (report is null)
        {
            return;
        }

        reports.Remove(report);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
