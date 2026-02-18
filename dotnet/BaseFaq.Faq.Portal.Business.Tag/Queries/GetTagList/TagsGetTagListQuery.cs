using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Faq.Dtos.Tag;
using MediatR;

namespace BaseFaq.Faq.Portal.Business.Tag.Queries.GetTagList;

public sealed class TagsGetTagListQuery : IRequest<PagedResultDto<TagDto>>
{
    public required TagGetAllRequestDto Request { get; set; }
}