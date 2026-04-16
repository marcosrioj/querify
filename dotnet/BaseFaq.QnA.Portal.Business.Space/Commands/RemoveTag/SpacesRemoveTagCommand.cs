using MediatR;

namespace BaseFaq.QnA.Portal.Business.Space.Commands.RemoveTag;

public sealed class SpacesRemoveTagCommand : IRequest
{
    public Guid SpaceId { get; set; }
    public Guid TagId { get; set; }
}