using MediatR;
using Tranqui.Application.Abstractions;
using Tranqui.Application.Errors;
using Tranqui.Domain.Abstractions;
using Tranqui.Domain.PhoneNumbers;
using Tranqui.Domain.Reputation;
using Tranqui.Domain.Users;

namespace Tranqui.Application.Features.ReportCall;

internal sealed class ReportCallHandler(
    ICurrentUser currentUser,
    IUserRepository users,
    IPhoneNumberHasher hasher,
    INameProtector nameProtector,
    IContributorIdProvider contributorIds,
    ISpamReportRepository reports,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IRequestHandler<ReportCallCommand>
{
    public async Task Handle(ReportCallCommand request, CancellationToken cancellationToken)
    {
        var user = await users.GetByFirebaseUidAsync(currentUser.FirebaseUid, cancellationToken)
            ?? throw new AccountNotRegisteredException();

        var now = timeProvider.GetUtcNow();
        var phoneNumber = PhoneNumber.TryParse(request.PhoneNumber)
            ?? throw new InvalidOperationException("The validator guarantees a valid phone number.");
        var phoneHash = hasher.Hash(phoneNumber);
        var contributor = contributorIds.FromFirebaseUid(user.FirebaseUid);
        var weight = ReputationRules.ReporterWeight(now - user.CreatedAt);
        // An offensive or personal label is dropped; the vote itself still counts.
        var label = CallerName.TryCreate(request.Label) is { } name && CallerNameFilter.IsShareable(name)
            ? nameProtector.Protect(phoneNumber, name)
            : null;

        var existing = await reports.GetAsync(phoneHash, contributor, cancellationToken);
        if (existing is null)
        {
            reports.Add(SpamReport.Create(phoneHash, contributor, request.Verdict, weight, label, now));
        }
        else
        {
            existing.Revise(request.Verdict, weight, label, now);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
