using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.KnowledgeSource;
using BaseFaq.QnA.Common.Persistence.QnADb;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.KnowledgeSource.Commands;

public sealed class KnowledgeSourcesCreateKnowledgeSourceCommand : IRequest<Guid>
{
    public required KnowledgeSourceCreateRequestDto Request { get; set; }
}
