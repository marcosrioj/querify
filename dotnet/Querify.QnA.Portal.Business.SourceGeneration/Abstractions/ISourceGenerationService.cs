using Querify.Models.Common.Dtos;
using Querify.Models.QnA.Dtos.SourceGeneration;

namespace Querify.QnA.Portal.Business.SourceGeneration.Abstractions;

public interface ISourceGenerationService
{
    Task<Guid> GenerateSpace(Guid sourceId, SourceGenerateSpaceRequestDto request, CancellationToken token);
    Task<SourceGenerationRunDto> GetRun(Guid runId, CancellationToken token);
    Task<PagedResultDto<SourceGenerationRunSummaryDto>> GetRuns(Guid sourceId, int skipCount, int maxResultCount,
        CancellationToken token);
}
