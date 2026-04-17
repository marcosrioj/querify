using BaseFaq.Models.QnA.Dtos.Space;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.Space.Commands.CreateSpace;

public sealed class SpacesCreateSpaceCommand : IRequest<Guid>
{
    public required SpaceCreateRequestDto Request { get; set; }
}