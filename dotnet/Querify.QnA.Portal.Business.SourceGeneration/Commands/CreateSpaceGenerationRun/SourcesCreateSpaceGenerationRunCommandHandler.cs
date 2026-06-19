using System.Net;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Models.Common.Enums;
using Querify.Models.QnA.Dtos.SourceGeneration;
using Querify.Models.QnA.Enums;
using Querify.QnA.Common.Domain.Entities;
using Querify.QnA.Common.Domain.BusinessRules.Spaces;
using Querify.QnA.Common.Persistence.QnADb.DbContext;

namespace Querify.QnA.Portal.Business.SourceGeneration.Commands.CreateSpaceGenerationRun;

public sealed class SourcesCreateSpaceGenerationRunCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<SourcesCreateSpaceGenerationRunCommand, Guid>
{
    public async Task<Guid> Handle(SourcesCreateSpaceGenerationRunCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
        var userId = sessionService.GetUserId().ToString();
        var source = await dbContext.Sources
            .AsNoTracking()
            .SingleOrDefaultAsync(entity => entity.TenantId == tenantId && entity.Id == request.SourceId,
                cancellationToken);

        if (source is null)
            throw new ApiErrorException(
                $"Source '{request.SourceId}' was not found.",
                (int)HttpStatusCode.NotFound);

        EnsureSourceCanGenerate(source);

        var dto = Normalize(request.Request);

        var draftSpace = new Space
        {
            TenantId = tenantId,
            Name = dto.SpaceName,
            Slug = dto.SpaceSlug ?? dto.SpaceName,
            Language = dto.Language,
            Status = dto.Status,
            Visibility = dto.Visibility,
            AcceptsQuestions = dto.AcceptsQuestions,
            AcceptsAnswers = dto.AcceptsAnswers,
            CreatedBy = userId,
            UpdatedBy = userId
        };
        SpaceRules.EnsureVisibilityAllowed(draftSpace, draftSpace.Visibility);

        var entity = new SourceGenerationRun
        {
            TenantId = tenantId,
            SourceId = source.Id,
            Status = SourceGenerationRunStatus.Pending,
            SpaceName = dto.SpaceName,
            SpaceSlug = dto.SpaceSlug,
            Language = dto.Language,
            Visibility = dto.Visibility,
            SpaceStatus = dto.Status,
            AcceptsQuestions = dto.AcceptsQuestions,
            AcceptsAnswers = dto.AcceptsAnswers,
            ExtractionGoal = dto.ExtractionGoal,
            MaxTopLevelQuestions = dto.MaxTopLevelQuestions,
            MaxFollowUpDepth = dto.MaxFollowUpDepth,
            MaxAnswersPerQuestion = dto.MaxAnswersPerQuestion,
            IncludeFollowUpQuestions = dto.IncludeFollowUpQuestions,
            TagGenerationMode = dto.TagGenerationMode,
            SourceRole = dto.SourceRole,
            RequireEveryAnswerToCiteSource = dto.RequireEveryAnswerToCiteSource,
            ContentHint = dto.ContentHint,
            CreatedBy = userId,
            UpdatedBy = userId
        };

        dbContext.SourceGenerationRuns.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }

    private static void EnsureSourceCanGenerate(Source source)
    {
        if (!string.IsNullOrWhiteSpace(source.StorageKey) &&
            source.UploadStatus is not SourceUploadStatus.Verified)
            throw new ApiErrorException(
                "Uploaded sources must be verified before generating a QnA space.",
                (int)HttpStatusCode.UnprocessableEntity);

        if (string.IsNullOrWhiteSpace(source.Locator) && string.IsNullOrWhiteSpace(source.StorageKey))
            throw new ApiErrorException(
                "The source does not include usable locator or storage content.",
                (int)HttpStatusCode.UnprocessableEntity);
    }

    private static SourceGenerateSpaceRequestDto Normalize(SourceGenerateSpaceRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.SpaceName))
            throw new ApiErrorException(
                "Space name is required.",
                (int)HttpStatusCode.UnprocessableEntity);

        if (request.Visibility is VisibilityScope.Public)
            throw new ApiErrorException(
                "Generated content cannot be made public during source generation.",
                (int)HttpStatusCode.UnprocessableEntity);

        if (!Enum.IsDefined(request.Status))
            throw new ApiErrorException(
                "Unsupported generated space status.",
                (int)HttpStatusCode.UnprocessableEntity);

        if (!Enum.IsDefined(request.Visibility))
            throw new ApiErrorException(
                "Unsupported generated space visibility.",
                (int)HttpStatusCode.UnprocessableEntity);

        if (!Enum.IsDefined(request.TagGenerationMode))
            throw new ApiErrorException(
                "Unsupported source generation tag mode.",
                (int)HttpStatusCode.UnprocessableEntity);

        if (!Enum.IsDefined(request.SourceRole))
            throw new ApiErrorException(
                "Unsupported source role.",
                (int)HttpStatusCode.UnprocessableEntity);

        return new SourceGenerateSpaceRequestDto
        {
            SpaceName = TrimRequired(request.SpaceName, SourceGenerationRun.MaxSpaceNameLength),
            SpaceSlug = TrimOptional(request.SpaceSlug, SourceGenerationRun.MaxSpaceSlugLength),
            Language = TrimRequired(request.Language, SourceGenerationRun.MaxLanguageLength, "en-US"),
            Visibility = request.Visibility,
            Status = request.Status,
            AcceptsQuestions = request.AcceptsQuestions,
            AcceptsAnswers = request.AcceptsAnswers,
            ExtractionGoal = TrimOptional(request.ExtractionGoal, SourceGenerationRun.MaxExtractionGoalLength),
            MaxTopLevelQuestions = Math.Clamp(request.MaxTopLevelQuestions, 1, 12),
            MaxFollowUpDepth = Math.Clamp(request.MaxFollowUpDepth, 0, 3),
            MaxAnswersPerQuestion = Math.Clamp(request.MaxAnswersPerQuestion, 1, 3),
            IncludeFollowUpQuestions = request.IncludeFollowUpQuestions,
            TagGenerationMode = request.TagGenerationMode,
            SourceRole = request.SourceRole,
            RequireEveryAnswerToCiteSource = request.RequireEveryAnswerToCiteSource,
            ContentHint = TrimOptional(request.ContentHint, SourceGenerationRun.MaxContentHintLength)
        };
    }

    private static string TrimRequired(string? value, int maxLength, string? fallback = null)
    {
        var resolved = string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

        if (string.IsNullOrWhiteSpace(resolved))
            throw new ApiErrorException(
                "A required source generation value was empty.",
                (int)HttpStatusCode.UnprocessableEntity);

        return resolved.Length <= maxLength ? resolved : resolved[..maxLength].Trim();
    }

    private static string? TrimOptional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var resolved = value.Trim();
        return resolved.Length <= maxLength ? resolved : resolved[..maxLength].Trim();
    }
}
