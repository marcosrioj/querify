using System.Net;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Models.Common.Dtos;
using Querify.Models.Common.Enums;
using Querify.Models.QnA.Dtos.SourceGeneration;
using Querify.QnA.Common.Persistence.QnADb.DbContext;

namespace Querify.QnA.Portal.Business.SourceGeneration.Queries.GetSpaceGenerationRunList;

public sealed class SourcesGetSpaceGenerationRunListQueryHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<SourcesGetSpaceGenerationRunListQuery, PagedResultDto<SourceGenerationRunSummaryDto>>
{
    public async Task<PagedResultDto<SourceGenerationRunSummaryDto>> Handle(SourcesGetSpaceGenerationRunListQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
        var sourceExists = await dbContext.Sources
            .AsNoTracking()
            .AnyAsync(source => source.TenantId == tenantId && source.Id == request.SourceId, cancellationToken);

        if (!sourceExists)
            throw new ApiErrorException(
                $"Source '{request.SourceId}' was not found.",
                (int)HttpStatusCode.NotFound);

        var query = dbContext.SourceGenerationRuns
            .AsNoTracking()
            .Where(run => run.TenantId == tenantId && run.SourceId == request.SourceId)
            .OrderByDescending(run => run.CreatedDate);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip(Math.Max(0, request.SkipCount))
            .Take(Math.Clamp(request.MaxResultCount, 1, 50))
            .Select(run => new SourceGenerationRunSummaryDto
            {
                Id = run.Id,
                SourceId = run.SourceId,
                CreatedSpaceId = run.CreatedSpaceId,
                Status = run.Status,
                FailureReason = run.FailureReason,
                SpaceName = run.SpaceName,
                TagGenerationMode = run.TagGenerationMode,
                CreatedAtUtc = run.CreatedDate ?? DateTime.MinValue,
                CompletedAtUtc = run.CompletedAtUtc
            })
            .ToListAsync(cancellationToken);

        return new PagedResultDto<SourceGenerationRunSummaryDto>(totalCount, items);
    }
}
