using MediatR;

namespace BaseFaq.QnA.Portal.Business.KnowledgeSource.Commands.DeleteKnowledgeSource;

public sealed class KnowledgeSourcesDeleteKnowledgeSourceCommand : IRequest
{
    public Guid Id { get; set; }
}