using MediatR;

namespace Tranqui.Application.Features.PurgeExpiredData;

internal sealed class PurgeExpiredDataHandler(IExpiredDataPurger purger, TimeProvider timeProvider)
    : IRequestHandler<PurgeExpiredDataCommand, PurgeResult>
{
    public Task<PurgeResult> Handle(PurgeExpiredDataCommand request, CancellationToken cancellationToken) =>
        purger.PurgeAsync(PurgeCutoffs.At(timeProvider.GetUtcNow()), cancellationToken);
}
