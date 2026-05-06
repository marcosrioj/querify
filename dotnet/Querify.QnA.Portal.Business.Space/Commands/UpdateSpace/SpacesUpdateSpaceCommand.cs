using Querify.Models.QnA.Dtos.Space;
using MediatR;

namespace Querify.QnA.Portal.Business.Space.Commands.UpdateSpace;

public sealed class SpacesUpdateSpaceCommand : IRequest<Guid>
{
    public required Guid Id { get; set; }
    public required SpaceUpdateRequestDto Request { get; set; }
}