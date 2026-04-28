using BaseFaq.Models.QnA.Dtos.Space;
using MediatR;

namespace BaseFaq.QnA.Public.Business.Space.Queries.GetSpaceBySlug;

public sealed class SpacesGetSpaceBySlugQuery : IRequest<SpaceDto>
{
    public required string Slug { get; set; }
}
