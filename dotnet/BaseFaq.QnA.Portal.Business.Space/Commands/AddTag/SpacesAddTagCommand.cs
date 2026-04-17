using BaseFaq.Models.QnA.Dtos.Space;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.Space.Commands.AddTag;

public sealed class SpacesAddTagCommand : IRequest<Guid>
{
    public required SpaceTagCreateRequestDto Request { get; set; }
}