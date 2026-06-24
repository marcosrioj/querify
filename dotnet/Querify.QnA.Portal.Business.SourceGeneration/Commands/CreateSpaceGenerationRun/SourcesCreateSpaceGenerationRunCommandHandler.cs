using System.Globalization;
using System.Net;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Models.Common.Enums;
using Querify.Models.QnA.Dtos.SourceGeneration;
using Querify.Models.QnA.Enums;
using Querify.QnA.Common.Domain.BusinessRules.Spaces;
using Querify.QnA.Common.Domain.Entities;
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

        var dto = Normalize(request.Request, source);

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

    private static NormalizedSourceGenerationRequest Normalize(SourceGenerateSpaceRequestDto request, Source source)
    {
        ArgumentNullException.ThrowIfNull(request);

        var extractionGoal = TrimOptional(request.ExtractionGoal, SourceGenerationRun.MaxExtractionGoalLength);
        var contentHint = TrimOptional(request.ContentHint, SourceGenerationRun.MaxContentHintLength);
        var spaceName = ResolveSpaceName(source, extractionGoal, contentHint);
        var spaceSlug = SpaceSlugRules.GenerateSlug(spaceName);
        if (string.IsNullOrWhiteSpace(spaceSlug))
            spaceSlug = SpaceSlugRules.GenerateFallbackSlug();
        var shape = ResolveAutomaticShape(source, extractionGoal, contentHint);

        return new NormalizedSourceGenerationRequest(
            spaceName,
            TrimOptional(spaceSlug, SourceGenerationRun.MaxSpaceSlugLength),
            TrimRequired(source.Language, SourceGenerationRun.MaxLanguageLength, "en-US"),
            VisibilityScope.Internal,
            SpaceStatus.Draft,
            true,
            true,
            extractionGoal,
            shape.MaxTopLevelQuestions,
            shape.MaxFollowUpDepth,
            shape.MaxAnswersPerQuestion,
            shape.MaxFollowUpDepth > 0,
            SourceGenerationTagMode.CreateAndAttach,
            SourceRole.Evidence,
            true,
            contentHint);
    }

    private static string ResolveSpaceName(Source source, string? extractionGoal, string? contentHint)
    {
        var candidate =
            FirstNonEmpty(
                source.Label,
                ResolveLocatorLabel(source.Locator),
                source.ExternalId,
                extractionGoal,
                contentHint) ??
            "Generated source space";

        return TrimRequired(Humanize(candidate), SourceGenerationRun.MaxSpaceNameLength, "Generated source space");
    }

    private static SourceGenerationShape ResolveAutomaticShape(
        Source source,
        string? extractionGoal,
        string? contentHint)
    {
        var signal = string.Join(" ",
            source.Label,
            source.ContextNote,
            source.ExternalId,
            source.MediaType,
            source.MetadataJson,
            extractionGoal,
            contentHint);
        var wordCount = CountWords(signal);
        var topLevelQuestions = Math.Clamp(2 + wordCount / 40, 2, 8);

        if (IsStructuredSource(source))
            topLevelQuestions = Math.Min(10, topLevelQuestions + 1);

        var followUpDepth = !string.IsNullOrWhiteSpace(contentHint) || wordCount >= 80
            ? 2
            : 1;
        if (wordCount >= 180)
            followUpDepth = 3;

        var maxAnswersPerQuestion = NeedsAlternativeAnswers(extractionGoal, contentHint) ? 2 : 1;

        return new SourceGenerationShape(
            topLevelQuestions,
            Math.Clamp(followUpDepth, 1, 3),
            maxAnswersPerQuestion);
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim();
    }

    private static string? ResolveLocatorLabel(string? locator)
    {
        if (string.IsNullOrWhiteSpace(locator))
            return null;

        if (Uri.TryCreate(locator, UriKind.Absolute, out var uri))
        {
            var lastSegment = uri.Segments
                .LastOrDefault(segment => !string.IsNullOrWhiteSpace(segment.Trim('/')))
                ?.Trim('/');
            return string.IsNullOrWhiteSpace(lastSegment)
                ? uri.Host
                : WebUtility.UrlDecode(lastSegment);
        }

        var segments = locator.Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries);
        return segments.Length == 0
            ? locator
            : WebUtility.UrlDecode(segments[^1]);
    }

    private static string Humanize(string value)
    {
        var chars = value
            .Select(character => char.IsLetterOrDigit(character) || char.IsWhiteSpace(character)
                ? character
                : ' ')
            .ToArray();
        var normalized = string.Join(' ',
            new string(chars)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

        if (string.IsNullOrWhiteSpace(normalized))
            return "Generated source space";

        return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(normalized.ToLowerInvariant());
    }

    private static bool IsStructuredSource(Source source)
    {
        var mediaType = source.MediaType ?? string.Empty;
        return mediaType.Contains("html", StringComparison.OrdinalIgnoreCase) ||
               mediaType.Contains("pdf", StringComparison.OrdinalIgnoreCase) ||
               mediaType.Contains("json", StringComparison.OrdinalIgnoreCase) ||
               mediaType.Contains("markdown", StringComparison.OrdinalIgnoreCase) ||
               mediaType.Contains("text", StringComparison.OrdinalIgnoreCase);
    }

    private static bool NeedsAlternativeAnswers(string? extractionGoal, string? contentHint)
    {
        var signal = $"{extractionGoal} {contentHint}";
        return ContainsAny(signal,
            "alternative",
            "compare",
            "contrast",
            "scenario",
            "troubleshoot",
            "edge case",
            "risk",
            "policy");
    }

    private static bool ContainsAny(string value, params string[] terms)
    {
        return terms.Any(term => value.Contains(term, StringComparison.OrdinalIgnoreCase));
    }

    private static int CountWords(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return 0;

        var count = 0;
        var inWord = false;
        foreach (var character in value)
        {
            if (char.IsLetterOrDigit(character))
            {
                if (!inWord)
                    count++;
                inWord = true;
                continue;
            }

            inWord = false;
        }

        return count;
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

    private sealed record SourceGenerationShape(
        int MaxTopLevelQuestions,
        int MaxFollowUpDepth,
        int MaxAnswersPerQuestion);

    private sealed record NormalizedSourceGenerationRequest(
        string SpaceName,
        string? SpaceSlug,
        string Language,
        VisibilityScope Visibility,
        SpaceStatus Status,
        bool AcceptsQuestions,
        bool AcceptsAnswers,
        string? ExtractionGoal,
        int MaxTopLevelQuestions,
        int MaxFollowUpDepth,
        int MaxAnswersPerQuestion,
        bool IncludeFollowUpQuestions,
        SourceGenerationTagMode TagGenerationMode,
        SourceRole SourceRole,
        bool RequireEveryAnswerToCiteSource,
        string? ContentHint);
}
