using Querify.Models.QnA.Dtos.Space;
using MediatR;

namespace Querify.QnA.Public.Business.Space.Queries.GetSpace;

public sealed class SpacesGetSpaceQuery : IRequest<SpaceDto>
{
    public required Guid Id { get; set; }
}