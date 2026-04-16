using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Dtos.KnowledgeSource;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.KnowledgeSource.Queries.GetKnowledgeSourceList;

public sealed class KnowledgeSourcesGetKnowledgeSourceListQuery : IRequest<PagedResultDto<KnowledgeSourceDto>>
{
    public required KnowledgeSourceGetAllRequestDto Request { get; set; }
}