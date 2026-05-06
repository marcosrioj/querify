using Querify.Models.QnA.Dtos.Space;
using MediatR;

namespace Querify.QnA.Portal.Business.Space.Commands.AddTag;

public sealed class SpacesAddTagCommand : IRequest<Guid>
{
    public required SpaceTagCreateRequestDto Request { get; set; }
}