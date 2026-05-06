using MediatR;

namespace Querify.QnA.Portal.Business.Space.Commands.DeleteSpace;

public sealed class SpacesDeleteSpaceCommand : IRequest
{
    public required Guid Id { get; set; }
}