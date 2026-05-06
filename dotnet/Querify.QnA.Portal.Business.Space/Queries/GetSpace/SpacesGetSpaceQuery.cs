using Querify.Models.QnA.Dtos.Space;
using MediatR;

namespace Querify.QnA.Portal.Business.Space.Queries.GetSpace;

public sealed class SpacesGetSpaceQuery : IRequest<SpaceDetailDto>
{
    public required Guid Id { get; set; }
}