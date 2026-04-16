using MediatR;

namespace BaseFaq.QnA.Portal.Business.Source.Commands.DeleteSource;

public sealed class SourcesDeleteSourceCommand : IRequest
{
    public required Guid Id { get; set; }
}