using Querify.Models.QnA.Dtos.Space;
using MediatR;

namespace Querify.QnA.Public.Business.Space.Queries.GetSpaceBySlug;

public sealed class SpacesGetSpaceBySlugQuery : IRequest<SpaceDto>
{
    public required string Slug { get; set; }
}
