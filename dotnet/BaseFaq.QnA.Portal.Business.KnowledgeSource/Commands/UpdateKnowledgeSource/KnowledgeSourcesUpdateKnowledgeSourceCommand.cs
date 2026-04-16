using BaseFaq.Models.QnA.Dtos.KnowledgeSource;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.KnowledgeSource.Commands.UpdateKnowledgeSource;

public sealed class KnowledgeSourcesUpdateKnowledgeSourceCommand : IRequest<Guid>
{
    public Guid Id { get; set; }
    public required KnowledgeSourceUpdateRequestDto Request { get; set; }
}