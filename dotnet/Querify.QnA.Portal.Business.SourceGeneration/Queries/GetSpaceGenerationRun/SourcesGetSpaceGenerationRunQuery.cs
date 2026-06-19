using MediatR;
using Querify.Models.QnA.Dtos.SourceGeneration;

namespace Querify.QnA.Portal.Business.SourceGeneration.Queries.GetSpaceGenerationRun;

public sealed class SourcesGetSpaceGenerationRunQuery : IRequest<SourceGenerationRunDto>
{
    public required Guid RunId { get; set; }
}
