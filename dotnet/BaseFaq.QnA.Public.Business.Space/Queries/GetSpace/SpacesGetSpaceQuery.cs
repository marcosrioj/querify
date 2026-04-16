using BaseFaq.Models.QnA.Dtos.Space;
using MediatR;

namespace BaseFaq.QnA.Public.Business.Space.Queries.GetSpace;

public sealed class SpacesGetSpaceQuery : IRequest<SpaceDto>
{
    public Guid Id { get; set; }
}