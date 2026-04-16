using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Dtos.Space;
using BaseFaq.QnA.Public.Business.Space.Abstractions;
using BaseFaq.QnA.Public.Business.Space.Queries.GetSpace;
using BaseFaq.QnA.Public.Business.Space.Queries.GetSpaceByKey;
using BaseFaq.QnA.Public.Business.Space.Queries.GetSpaceList;
using MediatR;

namespace BaseFaq.QnA.Public.Business.Space.Service;

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

    public Task<SpaceDto> GetByKey(string key, CancellationToken token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return mediator.Send(new SpacesGetSpaceByKeyQuery { Key = key }, token);
    }
}