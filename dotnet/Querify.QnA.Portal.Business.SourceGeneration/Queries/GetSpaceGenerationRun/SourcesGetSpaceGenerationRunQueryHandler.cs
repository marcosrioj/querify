using System.Net;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Models.Common.Enums;
using Querify.Models.QnA.Dtos.SourceGeneration;
using Querify.QnA.Common.Persistence.QnADb.DbContext;

namespace Querify.QnA.Portal.Business.SourceGeneration.Queries.GetSpaceGenerationRun;

public sealed class SourcesGetSpaceGenerationRunQueryHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<SourcesGetSpaceGenerationRunQuery, SourceGenerationRunDto>
{
    public async Task<SourceGenerationRunDto> Handle(SourcesGetSpaceGenerationRunQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
        var entity = await dbContext.SourceGenerationRuns
            .AsNoTracking()
            .Where(run => run.TenantId == tenantId && run.Id == request.RunId)
            .Select(run => new SourceGenerationRunDto
            {
                Id = run.Id,
                SourceId = run.SourceId,
                CreatedSpaceId = run.CreatedSpaceId,
                Status = run.Status,
                FailureReason = run.FailureReason,
                Warning = run.Warning,
                SpaceName = run.SpaceName,
                SpaceSlug = run.SpaceSlug,
                Language = run.Language,
                Visibility = run.Visibility,
                SpaceStatus = run.SpaceStatus,
                AcceptsQuestions = run.AcceptsQuestions,
                AcceptsAnswers = run.AcceptsAnswers,
                ExtractionGoal = run.ExtractionGoal,
                MaxTopLevelQuestions = run.MaxTopLevelQuestions,
                MaxFollowUpDepth = run.MaxFollowUpDepth,
                MaxAnswersPerQuestion = run.MaxAnswersPerQuestion,
                IncludeFollowUpQuestions = run.IncludeFollowUpQuestions,
                TagGenerationMode = run.TagGenerationMode,
                SourceRole = run.SourceRole,
                RequireEveryAnswerToCiteSource = run.RequireEveryAnswerToCiteSource,
                ContentHint = run.ContentHint,
                CreatedAtUtc = run.CreatedDate ?? DateTime.MinValue,
                StartedAtUtc = run.StartedAtUtc,
                CompletedAtUtc = run.CompletedAtUtc
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (entity is null)
            throw new ApiErrorException(
                $"Source generation run '{request.RunId}' was not found.",
                (int)HttpStatusCode.NotFound);

        return entity;
    }
}
