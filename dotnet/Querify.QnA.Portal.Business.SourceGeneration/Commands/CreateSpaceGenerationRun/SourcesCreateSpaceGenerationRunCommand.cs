using MediatR;
using Querify.Models.QnA.Dtos.SourceGeneration;

namespace Querify.QnA.Portal.Business.SourceGeneration.Commands.CreateSpaceGenerationRun;

public sealed class SourcesCreateSpaceGenerationRunCommand : IRequest<Guid>
{
    public required Guid SourceId { get; set; }
    public required SourceGenerateSpaceRequestDto Request { get; set; }
}
