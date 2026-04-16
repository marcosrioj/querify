using BaseFaq.Models.QnA.Dtos.Space;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.Space.Commands.UpdateSpace;

public sealed class SpacesUpdateSpaceCommand : IRequest<Guid>
{
    public Guid Id { get; set; }
    public required SpaceUpdateRequestDto Request { get; set; }
}