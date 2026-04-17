using BaseFaq.Models.QnA.Dtos.Space;
using MediatR;

namespace BaseFaq.QnA.Public.Business.Space.Queries.GetSpaceByKey;

public sealed class SpacesGetSpaceByKeyQuery : IRequest<SpaceDto>
{
    public required string Key { get; set; }
}