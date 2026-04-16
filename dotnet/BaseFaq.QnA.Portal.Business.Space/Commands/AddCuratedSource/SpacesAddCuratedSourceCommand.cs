using BaseFaq.Models.QnA.Dtos.Space;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.Space.Commands.AddCuratedSource;

public sealed class SpacesAddCuratedSourceCommand : IRequest<Guid>
{
    public required SpaceSourceCreateRequestDto Request { get; set; }
}