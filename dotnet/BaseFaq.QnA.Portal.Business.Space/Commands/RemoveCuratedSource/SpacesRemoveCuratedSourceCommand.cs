using MediatR;

namespace BaseFaq.QnA.Portal.Business.Space.Commands.RemoveCuratedSource;

public sealed class SpacesRemoveCuratedSourceCommand : IRequest
{
    public required Guid SpaceId { get; set; }
    public required Guid SourceId { get; set; }
}