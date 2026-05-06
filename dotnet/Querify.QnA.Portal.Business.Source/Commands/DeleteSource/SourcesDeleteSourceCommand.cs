using MediatR;

namespace Querify.QnA.Portal.Business.Source.Commands.DeleteSource;

public sealed class SourcesDeleteSourceCommand : IRequest
{
    public required Guid Id { get; set; }
}