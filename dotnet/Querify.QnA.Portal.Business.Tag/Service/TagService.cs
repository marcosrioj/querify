using Querify.Models.Common.Dtos;
using Querify.Models.QnA.Dtos.Tag;
using Querify.QnA.Portal.Business.Tag.Abstractions;
using Querify.QnA.Portal.Business.Tag.Commands.CreateTag;
using Querify.QnA.Portal.Business.Tag.Commands.DeleteTag;
using Querify.QnA.Portal.Business.Tag.Commands.UpdateTag;
using Querify.QnA.Portal.Business.Tag.Queries.GetTag;
using Querify.QnA.Portal.Business.Tag.Queries.GetTagList;
using MediatR;

namespace Querify.QnA.Portal.Business.Tag.Service;

public sealed class TagService(IMediator mediator) : ITagService
{
    public Task<Guid> Create(TagCreateRequestDto dto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return mediator.Send(new TagsCreateTagCommand { Request = dto }, token);
    }

    public Task<PagedResultDto<TagDto>> GetAll(TagGetAllRequestDto requestDto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(requestDto);
        return mediator.Send(new TagsGetTagListQuery { Request = requestDto }, token);
    }

    public Task<TagDto> GetById(Guid id, CancellationToken token)
    {
        return mediator.Send(new TagsGetTagQuery { Id = id }, token);
    }

    public Task<Guid> Update(Guid id, TagUpdateRequestDto dto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return mediator.Send(new TagsUpdateTagCommand { Id = id, Request = dto }, token);
    }

    public Task Delete(Guid id, CancellationToken token)
    {
        return mediator.Send(new TagsDeleteTagCommand { Id = id }, token);
    }
}