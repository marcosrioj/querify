using MediatR;

namespace BaseFaq.QnA.Portal.Business.Space.Commands.RemoveCuratedSource;

public sealed class SpacesRemoveCuratedSourceCommand : IRequest
{
    public Guid SpaceId { get; set; }
    public Guid SourceId { get; set; }
}