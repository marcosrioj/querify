using MediatR;
using Querify.Models.Common.Dtos;
using Querify.Models.QnA.Dtos.SourceGeneration;

namespace Querify.QnA.Portal.Business.SourceGeneration.Queries.GetSpaceGenerationRunList;

public sealed class SourcesGetSpaceGenerationRunListQuery : IRequest<PagedResultDto<SourceGenerationRunSummaryDto>>
{
    public required Guid SourceId { get; set; }
    public int SkipCount { get; set; }
    public int MaxResultCount { get; set; } = 20;
}
