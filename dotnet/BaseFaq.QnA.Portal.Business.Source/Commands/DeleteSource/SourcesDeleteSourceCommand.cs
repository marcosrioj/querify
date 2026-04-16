using MediatR;

namespace BaseFaq.QnA.Portal.Business.Source.Commands.DeleteSource;

public sealed class SourcesDeleteSourceCommand : IRequest
{
    public Guid Id { get; set; }
}