using System.Net;
using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Models.Common.Enums;
using Querify.Models.QnA.Enums;
using Querify.QnA.Common.Domain.BusinessRules.Activities;
using Querify.QnA.Common.Domain.BusinessRules.Answers;
using Querify.QnA.Common.Domain.BusinessRules.Questions;
using Querify.QnA.Common.Domain.BusinessRules.Spaces;
using Querify.QnA.Common.Domain.Entities;
using Querify.QnA.Common.Persistence.QnADb.DbContext;
using Querify.QnA.Worker.Business.SourceGeneration.Planning;

namespace Querify.QnA.Worker.Business.SourceGeneration.Commands.ExecuteSpaceGenerationRun;

public sealed class SourcesExecuteSpaceGenerationRunCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<SourcesExecuteSpaceGenerationRunCommand, Guid>
{
    public async Task<Guid> Handle(SourcesExecuteSpaceGenerationRunCommand request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
        var userId = sessionService.GetUserId().ToString();
        var run = await dbContext.SourceGenerationRuns
            .Include(entity => entity.Source)
            .SingleOrDefaultAsync(entity => entity.TenantId == tenantId && entity.Id == request.RunId,
                cancellationToken);

        if (run is null)
            throw new ApiErrorException(
                $"Source generation run '{request.RunId}' was not found.",
                (int)HttpStatusCode.NotFound);

        if (run.Status is SourceGenerationRunStatus.Completed or SourceGenerationRunStatus.Failed)
            return run.CreatedSpaceId ?? run.Id;

        run.Status = SourceGenerationRunStatus.Running;
        run.StartedAtUtc ??= DateTime.UtcNow;
        run.UpdatedBy = userId;
        await dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            var plan = SourceGenerationPlanFactory.Create(run, run.Source);
            SourceGenerationPlanValidator.Validate(run, plan);

            var createdSpaceId = await WriteGraphAsync(run, plan, tenantId, userId, cancellationToken);
            run.CreatedSpaceId = createdSpaceId;
            run.Status = SourceGenerationRunStatus.Completed;
            run.CompletedAtUtc = DateTime.UtcNow;
            run.Warning = TrimOptional(string.Join(" ", plan.Warnings), SourceGenerationRun.MaxWarningLength);
            run.RawOutputJson = TrimOptional(JsonSerializer.Serialize(plan), SourceGenerationRun.MaxRawOutputJsonLength);
            run.UpdatedBy = userId;

            await dbContext.SaveChangesAsync(cancellationToken);
            return createdSpaceId;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            await MarkFailedAsync(run.Id, tenantId, userId, ex, cancellationToken);
            return run.Id;
        }
    }

    private async Task<Guid> WriteGraphAsync(
        SourceGenerationRun run,
        SourceGenerationPlan plan,
        Guid tenantId,
        string userId,
        CancellationToken cancellationToken)
    {
        var source = run.Source;
        var slug = await ResolveSlugAsync(
            tenantId,
            run.SpaceSlug,
            run.SpaceName,
            cancellationToken);

        var space = new Space
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = plan.Space.Name,
            Slug = slug,
            Summary = BuildSpaceSummary(run, source),
            Language = plan.Space.Language,
            Status = run.SpaceStatus,
            Visibility = run.Visibility,
            AcceptsQuestions = run.AcceptsQuestions,
            AcceptsAnswers = run.AcceptsAnswers,
            CreatedBy = userId,
            UpdatedBy = userId
        };
        SpaceRules.EnsureVisibilityAllowed(space, space.Visibility);
        dbContext.Spaces.Add(space);
        SpaceRules.EnsureSourceLink(space, source, tenantId, userId, run.SourceRole);

        var tags = await ResolveTagsAsync(plan.Tags, run.TagGenerationMode, tenantId, userId, cancellationToken);
        foreach (var tag in tags)
            SpaceRules.EnsureTagLink(space, tag, tenantId, userId);

        var answerMap = new Dictionary<string, Answer>(StringComparer.OrdinalIgnoreCase);
        var sort = 0;

        foreach (var questionPlan in plan.Questions)
        {
            var parentAnswer = questionPlan.ParentAnswerTempId is null
                ? null
                : answerMap[questionPlan.ParentAnswerTempId];
            var question = new Question
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                SpaceId = space.Id,
                Space = space,
                ParentAnswerId = parentAnswer?.Id,
                ParentAnswer = parentAnswer,
                Title = TrimRequired(questionPlan.Title, Question.MaxTitleLength),
                Summary = TrimOptional(questionPlan.Summary, Question.MaxSummaryLength),
                ContextNote = TrimOptional(questionPlan.ContextNote, Question.MaxContextNoteLength),
                Status = QuestionStatus.Draft,
                Visibility = VisibilityScope.Internal,
                OriginChannel = ChannelKind.Import,
                AiConfidenceScore = 0,
                FeedbackScore = 0,
                Sort = sort++,
                CreatedBy = userId,
                UpdatedBy = userId
            };
            QuestionRules.EnsureVisibilityAllowed(question, question.Visibility);

            space.Questions.Add(question);
            parentAnswer?.FollowUpQuestions.Add(question);
            dbContext.Questions.Add(question);
            var questionLink = QuestionRules.CreateSourceLink(
                question,
                source,
                run.SourceRole,
                0,
                tenantId,
                userId);
            question.Sources.Add(questionLink);
            dbContext.QuestionSourceLinks.Add(questionLink);

            foreach (var tag in tags)
                QuestionRules.EnsureTagLink(question, tag, tenantId, userId);

            AddQuestionActivity(question, tenantId, userId);

            var answerSort = 0;
            foreach (var answerPlan in questionPlan.Answers)
            {
                var answer = new Answer
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    QuestionId = question.Id,
                    Question = question,
                    Headline = TrimRequired(answerPlan.Headline, Answer.MaxHeadlineLength),
                    Body = TrimOptional(answerPlan.Body, Answer.MaxBodyLength),
                    ContextNote = TrimOptional(answerPlan.ContextNote, Answer.MaxContextNoteLength),
                    AuthorLabel = "Querify Source Generation",
                    Kind = AnswerKind.Imported,
                    Status = AnswerStatus.Draft,
                    Visibility = VisibilityScope.Internal,
                    AiConfidenceScore = 0,
                    Score = 0,
                    Sort = answerSort++,
                    CreatedBy = userId,
                    UpdatedBy = userId
                };
                AnswerRules.EnsureVisibilityAllowed(answer, answer.Visibility);

                question.Answers.Add(answer);
                dbContext.Answers.Add(answer);
                var answerLink = AnswerRules.CreateSourceLink(
                    answer,
                    source,
                    run.SourceRole,
                    0,
                    tenantId,
                    userId);
                answer.Sources.Add(answerLink);
                dbContext.AnswerSourceLinks.Add(answerLink);
                AddAnswerActivity(question, answer, tenantId, userId);
                answerMap[answerPlan.TempId] = answer;
            }
        }

        return space.Id;
    }

    private async Task<List<Tag>> ResolveTagsAsync(
        IReadOnlyList<string> requestedTags,
        SourceGenerationTagMode tagMode,
        Guid tenantId,
        string userId,
        CancellationToken cancellationToken)
    {
        if (tagMode is not SourceGenerationTagMode.CreateAndAttach)
            return [];

        var normalizedNames = requestedTags
            .Select(NormalizeTagName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(12)
            .ToList();

        if (normalizedNames.Count == 0)
            return [];

        var existingTags = await dbContext.Tags
            .Where(tag => tag.TenantId == tenantId && normalizedNames.Contains(tag.Name))
            .ToListAsync(cancellationToken);
        var existingByName = existingTags.ToDictionary(tag => tag.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var name in normalizedNames)
        {
            if (existingByName.ContainsKey(name))
                continue;

            var tag = new Tag
            {
                TenantId = tenantId,
                Name = name,
                CreatedBy = userId,
                UpdatedBy = userId
            };
            dbContext.Tags.Add(tag);
            existingTags.Add(tag);
            existingByName[name] = tag;
        }

        return existingTags;
    }

    private async Task<string> ResolveSlugAsync(
        Guid tenantId,
        string? requestedSlug,
        string name,
        CancellationToken cancellationToken)
    {
        var baseSlug = string.IsNullOrWhiteSpace(requestedSlug)
            ? SpaceSlugRules.GenerateSlug(name)
            : SpaceSlugRules.GenerateSlug(requestedSlug);

        if (string.IsNullOrWhiteSpace(baseSlug))
            baseSlug = SpaceSlugRules.GenerateFallbackSlug();

        var candidate = baseSlug;
        var counter = 2;

        while (await dbContext.Spaces.AnyAsync(
                   space => space.TenantId == tenantId && space.Slug == candidate,
                   cancellationToken))
        {
            candidate = SpaceSlugRules.WithSuffix(baseSlug, counter);
            counter++;
        }

        return candidate;
    }

    private async Task MarkFailedAsync(
        Guid runId,
        Guid tenantId,
        string userId,
        Exception exception,
        CancellationToken cancellationToken)
    {
        dbContext.ChangeTracker.Clear();
        var failedRun = await dbContext.SourceGenerationRuns
            .SingleAsync(run => run.TenantId == tenantId && run.Id == runId, cancellationToken);

        failedRun.Status = SourceGenerationRunStatus.Failed;
        failedRun.FailureReason = TrimOptional(exception.Message, SourceGenerationRun.MaxFailureReasonLength);
        failedRun.CompletedAtUtc = DateTime.UtcNow;
        failedRun.UpdatedBy = userId;

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private void AddQuestionActivity(Question question, Guid tenantId, string userId)
    {
        var activity = new Activity
        {
            TenantId = tenantId,
            QuestionId = question.Id,
            Question = question,
            Kind = ActivityKindStatusMap.ForQuestionStatus(question.Status),
            ActorKind = ActorKind.Integration,
            ActorLabel = "Source Generation",
            UserPrint = userId,
            Ip = "source-generation",
            UserAgent = "Querify.SourceGeneration/1.0",
            OccurredAtUtc = DateTime.UtcNow,
            CreatedBy = userId,
            UpdatedBy = userId
        };
        question.Activities.Add(activity);
        question.LastActivityAtUtc = activity.OccurredAtUtc;
        dbContext.Activities.Add(activity);
    }

    private void AddAnswerActivity(Question question, Answer answer, Guid tenantId, string userId)
    {
        var activity = new Activity
        {
            TenantId = tenantId,
            QuestionId = question.Id,
            Question = question,
            AnswerId = answer.Id,
            Answer = answer,
            Kind = ActivityKindStatusMap.ForAnswerStatus(answer.Status),
            ActorKind = ActorKind.Integration,
            ActorLabel = "Source Generation",
            UserPrint = userId,
            Ip = "source-generation",
            UserAgent = "Querify.SourceGeneration/1.0",
            OccurredAtUtc = DateTime.UtcNow,
            CreatedBy = userId,
            UpdatedBy = userId
        };
        question.Activities.Add(activity);
        question.LastActivityAtUtc = activity.OccurredAtUtc;
        dbContext.Activities.Add(activity);
    }

    private static string BuildSpaceSummary(SourceGenerationRun run, Source source)
    {
        var sourceLabel = string.IsNullOrWhiteSpace(source.Label) ? source.Locator : source.Label;
        var goal = string.IsNullOrWhiteSpace(run.ExtractionGoal)
            ? "Generated from the selected source for human review."
            : run.ExtractionGoal.Trim();

        return TrimOptional($"{goal} Source: {sourceLabel}", Space.MaxSummaryLength) ??
               "Generated from the selected source for human review.";
    }

    private static string NormalizeTagName(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        normalized = normalized.Replace('/', '-').Replace('\\', '-').Replace(' ', '-');
        return normalized.Length <= Tag.MaxNameLength ? normalized : normalized[..Tag.MaxNameLength].Trim('-');
    }

    private static string TrimRequired(string value, int maxLength)
    {
        var trimmed = value.Trim();

        if (string.IsNullOrWhiteSpace(trimmed))
            throw new ApiErrorException(
                "The generation plan included an empty required value.",
                (int)HttpStatusCode.UnprocessableEntity);

        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength].Trim();
    }

    private static string? TrimOptional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength].Trim();
    }
}
