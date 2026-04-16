using BaseFaq.Models.QnA.Dtos.KnowledgeSource;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.KnowledgeSource.Commands.CreateKnowledgeSource;

public sealed class KnowledgeSourcesCreateKnowledgeSourceCommand : IRequest<Guid>
{
    public required KnowledgeSourceCreateRequestDto Request { get; set; }
}