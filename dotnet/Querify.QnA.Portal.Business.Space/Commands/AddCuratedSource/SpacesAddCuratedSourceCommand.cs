using Querify.Models.QnA.Dtos.Space;
using MediatR;

namespace Querify.QnA.Portal.Business.Space.Commands.AddCuratedSource;

public sealed class SpacesAddCuratedSourceCommand : IRequest<Guid>
{
    public required SpaceSourceCreateRequestDto Request { get; set; }
}