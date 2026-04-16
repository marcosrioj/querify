using MediatR;

namespace BaseFaq.QnA.Portal.Business.Space.Commands.RemoveTag;

public sealed class SpacesRemoveTagCommand : IRequest
{
    public required Guid SpaceId { get; set; }
    public required Guid TagId { get; set; }
}