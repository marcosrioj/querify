using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Dtos.Space;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.Space.Queries.GetSpaceList;

public sealed class SpacesGetSpaceListQuery : IRequest<PagedResultDto<SpaceDto>>
{
    public required SpaceGetAllRequestDto Request { get; set; }
}