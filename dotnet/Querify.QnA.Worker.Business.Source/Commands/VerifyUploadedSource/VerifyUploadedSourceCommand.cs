using MediatR;

namespace Querify.QnA.Worker.Business.Source.Commands.VerifyUploadedSource;

public sealed class VerifyUploadedSourceCommand : IRequest<Guid>
{
    public required Guid TenantId { get; set; }
    public required Guid SourceId { get; set; }
    public required string StorageKey { get; set; }
}
