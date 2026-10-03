using MediatR;

namespace Tranqui.Application.Features.AcceptContactUpload;

/// <summary>The user opts in to contribute their address book. Separate from, and optional on top of, the terms.</summary>
public sealed record AcceptContactUploadCommand(string AcceptedVersion) : IRequest;
