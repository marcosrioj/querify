using MediatR;

namespace Querify.QnA.Worker.Business.SourceGeneration.Commands.ExecuteSpaceGenerationRun;

public sealed class SourcesExecuteSpaceGenerationRunCommand : IRequest<Guid>
{
    public required Guid RunId { get; set; }
}
