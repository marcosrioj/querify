using MediatR;

namespace Querify.QnA.Portal.Business.Source.Commands.CompleteUpload;

public sealed class SourcesCompleteUploadCommand : IRequest<Guid>
{
    public required Guid SourceId { get; set; }
    public string? ClientChecksum { get; set; }
}
