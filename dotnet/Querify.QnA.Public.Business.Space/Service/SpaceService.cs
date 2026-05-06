using Querify.Models.Common.Dtos;
using Querify.Models.QnA.Dtos.Space;
using Querify.QnA.Public.Business.Space.Abstractions;
using Querify.QnA.Public.Business.Space.Queries.GetSpace;
using Querify.QnA.Public.Business.Space.Queries.GetSpaceBySlug;
using Querify.QnA.Public.Business.Space.Queries.GetSpaceList;
using MediatR;

namespace Querify.QnA.Public.Business.Space.Service;

public sealed class SpaceService(IMediator mediator) : ISpaceService
{
    public Task<PagedResultDto<SpaceDto>> GetAll(SpaceGetAllRequestDto requestDto,
        CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(requestDto);
        return mediator.Send(new SpacesGetSpaceListQuery { Request = requestDto }, token);
    }

    public Task<SpaceDto> GetById(Guid id, CancellationToken token)
    {
        return mediator.Send(new SpacesGetSpaceQuery { Id = id }, token);
    }

    public Task<SpaceDto> GetBySlug(string slug, CancellationToken token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        return mediator.Send(new SpacesGetSpaceBySlugQuery { Slug = slug }, token);
    }
}
