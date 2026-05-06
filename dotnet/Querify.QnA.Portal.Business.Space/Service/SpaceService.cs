using Querify.Models.Common.Dtos;
using Querify.Models.QnA.Dtos.Space;
using Querify.QnA.Portal.Business.Space.Abstractions;
using Querify.QnA.Portal.Business.Space.Commands.AddCuratedSource;
using Querify.QnA.Portal.Business.Space.Commands.AddTag;
using Querify.QnA.Portal.Business.Space.Commands.CreateSpace;
using Querify.QnA.Portal.Business.Space.Commands.DeleteSpace;
using Querify.QnA.Portal.Business.Space.Commands.RemoveCuratedSource;
using Querify.QnA.Portal.Business.Space.Commands.RemoveTag;
using Querify.QnA.Portal.Business.Space.Commands.UpdateSpace;
using Querify.QnA.Portal.Business.Space.Queries.GetSpace;
using Querify.QnA.Portal.Business.Space.Queries.GetSpaceList;
using MediatR;

namespace Querify.QnA.Portal.Business.Space.Service;

public sealed class SpaceService(IMediator mediator) : ISpaceService
{
    public Task<Guid> Create(SpaceCreateRequestDto dto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return mediator.Send(new SpacesCreateSpaceCommand { Request = dto }, token);
    }

    public Task<PagedResultDto<SpaceDto>> GetAll(SpaceGetAllRequestDto requestDto,
        CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(requestDto);
        return mediator.Send(new SpacesGetSpaceListQuery { Request = requestDto }, token);
    }

    public Task<SpaceDetailDto> GetById(Guid id, CancellationToken token)
    {
        return mediator.Send(new SpacesGetSpaceQuery { Id = id }, token);
    }

    public Task<Guid> Update(Guid id, SpaceUpdateRequestDto dto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return mediator.Send(new SpacesUpdateSpaceCommand { Id = id, Request = dto }, token);
    }

    public Task Delete(Guid id, CancellationToken token)
    {
        return mediator.Send(new SpacesDeleteSpaceCommand { Id = id }, token);
    }

    public Task<Guid> AddTag(SpaceTagCreateRequestDto dto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return mediator.Send(new SpacesAddTagCommand { Request = dto }, token);
    }

    public Task RemoveTag(Guid spaceId, Guid tagId, CancellationToken token)
    {
        return mediator.Send(new SpacesRemoveTagCommand
        {
            SpaceId = spaceId,
            TagId = tagId
        }, token);
    }

    public Task<Guid> AddCuratedSource(SpaceSourceCreateRequestDto dto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return mediator.Send(new SpacesAddCuratedSourceCommand { Request = dto }, token);
    }

    public Task RemoveCuratedSource(Guid spaceId, Guid sourceId, CancellationToken token)
    {
        return mediator.Send(new SpacesRemoveCuratedSourceCommand
        {
            SpaceId = spaceId,
            SourceId = sourceId
        }, token);
    }
}