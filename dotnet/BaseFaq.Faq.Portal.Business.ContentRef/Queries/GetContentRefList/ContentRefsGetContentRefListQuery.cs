using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Faq.Dtos.ContentRef;
using MediatR;

namespace BaseFaq.Faq.Portal.Business.ContentRef.Queries.GetContentRefList;

public sealed class ContentRefsGetContentRefListQuery : IRequest<PagedResultDto<ContentRefDto>>
{
    public required ContentRefGetAllRequestDto Request { get; set; }
}