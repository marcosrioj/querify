using MediatR;

namespace BaseFaq.QnA.Portal.Business.Space.Commands.DeleteSpace;

public sealed class SpacesDeleteSpaceCommand : IRequest
{
    public Guid Id { get; set; }
}