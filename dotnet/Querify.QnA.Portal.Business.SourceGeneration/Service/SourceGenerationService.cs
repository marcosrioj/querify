using MediatR;
using Querify.Models.Common.Dtos;
using Querify.Models.QnA.Dtos.SourceGeneration;
using Querify.QnA.Portal.Business.SourceGeneration.Abstractions;
using Querify.QnA.Portal.Business.SourceGeneration.Commands.CreateSpaceGenerationRun;
using Querify.QnA.Portal.Business.SourceGeneration.Queries.GetSpaceGenerationRun;
using Querify.QnA.Portal.Business.SourceGeneration.Queries.GetSpaceGenerationRunList;
using Querify.QnA.Worker.Business.SourceGeneration.Commands.ExecuteSpaceGenerationRun;

namespace Querify.QnA.Portal.Business.SourceGeneration.Service;

public sealed class SourceGenerationService(IMediator mediator) : ISourceGenerationService
{
    public async Task<Guid> GenerateSpace(Guid sourceId, SourceGenerateSpaceRequestDto request, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(request);

        var runId = await mediator.Send(new SourcesCreateSpaceGenerationRunCommand
        {
            SourceId = sourceId,
            Request = request
        }, token);

        await mediator.Send(new SourcesExecuteSpaceGenerationRunCommand { RunId = runId }, token);

        return runId;
    }

    public Task<SourceGenerationRunDto> GetRun(Guid runId, CancellationToken token)
    {
        return mediator.Send(new SourcesGetSpaceGenerationRunQuery { RunId = runId }, token);
    }

    public Task<PagedResultDto<SourceGenerationRunSummaryDto>> GetRuns(Guid sourceId, int skipCount,
        int maxResultCount, CancellationToken token)
    {
        return mediator.Send(new SourcesGetSpaceGenerationRunListQuery
        {
            SourceId = sourceId,
            SkipCount = skipCount,
            MaxResultCount = maxResultCount
        }, token);
    }
}
