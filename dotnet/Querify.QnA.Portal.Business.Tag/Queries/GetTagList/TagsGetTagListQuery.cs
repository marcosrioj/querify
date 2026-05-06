using Querify.Models.Common.Dtos;
using Querify.Models.QnA.Dtos.Tag;
using MediatR;

namespace Querify.QnA.Portal.Business.Tag.Queries.GetTagList;

public sealed class TagsGetTagListQuery : IRequest<PagedResultDto<TagDto>>
{
    public required TagGetAllRequestDto Request { get; set; }
}