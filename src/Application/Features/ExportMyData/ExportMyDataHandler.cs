using MediatR;
using Tranqui.Application.Abstractions;
using Tranqui.Application.Errors;
using Tranqui.Domain.Reputation;
using Tranqui.Domain.Users;

namespace Tranqui.Application.Features.ExportMyData;

internal sealed class ExportMyDataHandler(
    ICurrentUser currentUser,
    IUserRepository users,
    IContributorIdProvider contributorIds,
    ISpamReportRepository reports,
    IContactContributionRepository contributions,
    IBlockSignalRepository blocks) : IRequestHandler<ExportMyDataQuery, MyDataExport>
{
    public async Task<MyDataExport> Handle(ExportMyDataQuery request, CancellationToken cancellationToken)
    {
        var user = await users.GetByFirebaseUidAsync(currentUser.FirebaseUid, cancellationToken)
            ?? throw new AccountNotRegisteredException();
        var contributor = contributorIds.FromFirebaseUid(user.FirebaseUid);

        var consents = user.Consents
            .OrderBy(consent => consent.AcceptedAt)
            .Select(consent => new ConsentExport(consent.Type.ToString(), consent.Version, consent.AcceptedAt, consent.RevokedAt))
            .ToList();

        return new MyDataExport(
            user.Id,
            user.CreatedAt,
            consents,
            await reports.CountForContributorAsync(contributor, cancellationToken),
            await contributions.CountForContributorAsync(contributor, cancellationToken),
            await blocks.CountForContributorAsync(contributor, cancellationToken));
    }
}
