using BaseFaq.Models.QnA.Dtos.Space;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.Space.Queries.GetSpace;

public sealed class SpacesGetSpaceQuery : IRequest<SpaceDetailDto>
{
    public Guid Id { get; set; }
}