using MediatR;

namespace Tranqui.Application.Features.PurgeExpiredData;

/// <summary>Deletes everything older than <see cref="Domain.Retention.RetentionRules"/> allows. Runs daily and on demand.</summary>
public sealed record PurgeExpiredDataCommand : IRequest<PurgeResult>;
