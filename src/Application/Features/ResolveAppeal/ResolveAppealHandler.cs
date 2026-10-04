using MediatR;
using Tranqui.Application.Errors;
using Tranqui.Domain.Abstractions;
using Tranqui.Domain.Appeals;

namespace Tranqui.Application.Features.ResolveAppeal;

internal sealed class ResolveAppealHandler(IAppealRepository appeals, IUnitOfWork unitOfWork, TimeProvider timeProvider)
    : IRequestHandler<ResolveAppealCommand, AppealStatus>
{
    public async Task<AppealStatus> Handle(ResolveAppealCommand request, CancellationToken cancellationToken)
    {
        var appeal = await appeals.GetAppealAsync(request.AppealId, cancellationToken) ?? throw new AppealNotFoundException();
        if (appeal.Status != AppealStatus.Pending)
        {
            throw new AppealAlreadyResolvedException();
        }

        var now = timeProvider.GetUtcNow();
        appeal.Resolve(request.Decision, now);

        if (request.Decision == AppealDecision.Approve)
        {
            var cleared = await appeals.GetClearedAsync(appeal.Hash, cancellationToken);
            if (cleared is null)
            {
                appeals.Clear(ClearedNumber.Create(appeal.Hash, now));
            }
            else
            {
                cleared.ClearAgain(now);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return appeal.Status;
    }
}
