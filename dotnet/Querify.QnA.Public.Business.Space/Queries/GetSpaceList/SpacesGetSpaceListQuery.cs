using Querify.Models.Common.Dtos;
using Querify.Models.QnA.Dtos.Space;
using MediatR;

namespace Querify.QnA.Public.Business.Space.Queries.GetSpaceList;

public sealed class SpacesGetSpaceListQuery : IRequest<PagedResultDto<SpaceDto>>
{
    public required SpaceGetAllRequestDto Request { get; set; }
}