using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Dtos.Tag;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.Tag.Queries.GetTagList;

public sealed class TagsGetTagListQuery : IRequest<PagedResultDto<TagDto>>
{
    public required TagGetAllRequestDto Request { get; set; }
}