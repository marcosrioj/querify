using BaseFaq.Models.QnA.Dtos.KnowledgeSource;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.KnowledgeSource.Queries.GetKnowledgeSource;

public sealed class KnowledgeSourcesGetKnowledgeSourceQuery : IRequest<KnowledgeSourceDto>
{
    public Guid Id { get; set; }
}