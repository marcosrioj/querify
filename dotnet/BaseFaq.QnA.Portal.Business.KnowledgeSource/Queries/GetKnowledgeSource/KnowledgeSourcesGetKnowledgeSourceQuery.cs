using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.KnowledgeSource;
using BaseFaq.QnA.Common.Persistence.QnADb;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.KnowledgeSource.Queries.GetKnowledgeSource;

public sealed class KnowledgeSourcesGetKnowledgeSourceQuery : IRequest<KnowledgeSourceDto>
{
    public Guid Id { get; set; }
}
