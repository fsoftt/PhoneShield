using MediatR;
using Tranqui.Domain.PhoneNumbers;
using Tranqui.Domain.Reputation;

namespace Tranqui.Application.Features.LookupNumber;

internal sealed class LookupNumberHandler(
    IPhoneNumberHasher hasher,
    IReputationSignalsReader signalsReader,
    INameProtector nameProtector,
    TimeProvider timeProvider) : IRequestHandler<LookupNumberQuery, LookupResult>
{
    public async Task<LookupResult> Handle(LookupNumberQuery request, CancellationToken cancellationToken)
    {
        var phoneNumber = PhoneNumber.TryParse(request.PhoneNumber)
            ?? throw new InvalidOperationException("The validator guarantees a valid phone number.");

        var signals = await signalsReader.ReadAsync(hasher.Hash(phoneNumber), cancellationToken);
        var identification = CallerIdentification.Evaluate(signals, timeProvider.GetUtcNow());
        var names = identification.RankedNames.Select(name => nameProtector.Unprotect(phoneNumber, name)).ToList();

        return new LookupResult(
            identification.Status,
            names.FirstOrDefault(),
            names.Skip(1).ToList(),
            identification.SpamReportCount,
            identification.SavedByCount);
    }
}
