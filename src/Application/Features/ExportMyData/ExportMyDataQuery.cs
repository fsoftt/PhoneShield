using MediatR;

namespace Tranqui.Application.Features.ExportMyData;

/// <summary>Right of access: everything stored about the caller. Numbers are only stored hashed, so they are counted, not listed.</summary>
public sealed record ExportMyDataQuery : IRequest<MyDataExport>;
