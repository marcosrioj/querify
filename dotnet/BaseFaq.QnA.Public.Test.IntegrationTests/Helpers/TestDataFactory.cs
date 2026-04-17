using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Public.Test.IntegrationTests.Helpers;

public static class TestDataFactory
{
    public static async Task<Space> SeedSpaceAsync(
        QnADbContext dbContext,
        Guid tenantId,
        string? name = null,
        string? key = null,
        VisibilityScope visibility = VisibilityScope.PublicIndexed)
    {
        var entity = new Space
        {
            TenantId = tenantId,
            Name = name ?? "Public Support",
            Key = key ?? $"public-{Guid.NewGuid():N}".Substring(0, 12),
            DefaultLanguage = "en-US",
            Kind = SpaceKind.CuratedKnowledge,
            Summary = "Public knowledge",
            ModerationPolicy = ModerationPolicy.PostModeration,
            AcceptsQuestions = true,
            AcceptsAnswers = true,
            RequiresQuestionReview = false,
            RequiresAnswerReview = true,
            Visibility = visibility,
            SearchMarkupMode = SearchMarkupMode.Hybrid,
            PublishedAtUtc = visibility is VisibilityScope.Public or VisibilityScope.PublicIndexed
                ? DateTime.UtcNow
                : null,
            CreatedBy = "test",
            UpdatedBy = "test"
        };
        dbContext.Spaces.Add(entity);
        await dbContext.SaveChangesAsync();
        return entity;
    }

    public static async Task<Question> SeedQuestionAsync(
        QnADbContext dbContext,
        Guid tenantId,
        Guid spaceId,
        string? title = null,
        string? key = null,
        QuestionStatus status = QuestionStatus.Open,
        VisibilityScope visibility = VisibilityScope.PublicIndexed)
    {
        var space = await dbContext.Spaces
            .Include(entity => entity.Questions)
            .SingleAsync(entity => entity.Id == spaceId);

        var entity = new Question
        {
            TenantId = tenantId,
            SpaceId = space.Id,
            Space = space,
            Title = title ?? "How can I reset my password?",
            Key = key ?? $"question-{Guid.NewGuid():N}".Substring(0, 14),
            Summary = "Public summary",
            ContextNote = "Public context",
            ThreadSummary = "Public thread summary",
            Kind = QuestionKind.Curated,
            Status = status,
            Visibility = visibility,
            OriginChannel = ChannelKind.Widget,
            Language = "en-US",
            ProductScope = "Portal",
            JourneyScope = "Support",
            AudienceScope = "Customer",
            ContextKey = "default",
            ConfidenceScore = 90,
            RevisionNumber = 1,
            ValidatedAtUtc = status == QuestionStatus.Validated ? DateTime.UtcNow : null,
            CreatedBy = "test",
            UpdatedBy = "test"
        };
        space.Questions.Add(entity);
        entity.Activities.Add(new Activity
        {
            TenantId = tenantId,
            QuestionId = entity.Id,
            Question = entity,
            Kind = ActivityKind.QuestionCreated,
            ActorKind = ActorKind.Customer,
            ActorLabel = "customer",
            UserPrint = "customer",
            Ip = "127.0.0.1",
            UserAgent = "QnATest/1.0",
            OccurredAtUtc = DateTime.UtcNow,
            CreatedBy = "test",
            UpdatedBy = "test"
        });
        entity.LastActivityAtUtc = entity.Activities.Max(activity => activity.OccurredAtUtc);

        dbContext.Questions.Add(entity);
        await dbContext.SaveChangesAsync();
        return entity;
    }

    public static async Task<Answer> SeedAnswerAsync(
        QnADbContext dbContext,
        Guid tenantId,
        Guid questionId,
        string? headline = null,
        AnswerStatus status = AnswerStatus.Published,
        VisibilityScope visibility = VisibilityScope.PublicIndexed,
        bool accept = false,
        int rank = 1)
    {
        dbContext.ChangeTracker.Clear();

        var question = await dbContext.Questions
            .Include(entity => entity.Answers)
            .SingleAsync(entity => entity.Id == questionId);

        var entity = new Answer
        {
            TenantId = tenantId,
            QuestionId = question.Id,
            Question = question,
            Headline = headline ?? "Use the reset link from sign-in.",
            Body = "Open sign-in and request a reset link.",
            Kind = AnswerKind.Official,
            Language = "en-US",
            ContextKey = "default",
            ApplicabilityRulesJson = "{\"surface\":\"public\"}",
            ConfidenceScore = 94,
            TrustNote = "Reviewed",
            EvidenceSummary = "Docs backed",
            Status = status,
            Visibility = visibility,
            Rank = rank,
            RevisionNumber = status == AnswerStatus.Validated ? 2 : 1,
            CreatedBy = "test",
            UpdatedBy = "test"
        };

        switch (status)
        {
            case AnswerStatus.Validated:
                entity.Status = AnswerStatus.Validated;
                entity.ValidatedAtUtc = DateTime.UtcNow;
                entity.RevisionNumber = 2;
                break;
            case AnswerStatus.Published:
                entity.Status = AnswerStatus.Published;
                entity.PublishedAtUtc = DateTime.UtcNow;
                entity.RevisionNumber = 1;
                break;
            default:
                entity.Status = status;
                break;
        }

        entity.Visibility = visibility;
        question.Answers.Add(entity);
        var createdActivity = new Activity
        {
            TenantId = tenantId,
            QuestionId = question.Id,
            Question = question,
            AnswerId = entity.Id,
            Answer = entity,
            Kind = ActivityKind.AnswerCreated,
            ActorKind = ActorKind.Customer,
            ActorLabel = "customer",
            UserPrint = "customer",
            Ip = "127.0.0.1",
            UserAgent = "QnATest/1.0",
            OccurredAtUtc = DateTime.UtcNow,
            CreatedBy = "test",
            UpdatedBy = "test"
        };
        question.Activities.Add(createdActivity);
        question.LastActivityAtUtc = createdActivity.OccurredAtUtc;
        dbContext.Activities.Add(createdActivity);

        if (accept)
        {
            var acceptedAtUtc = DateTime.UtcNow;
            entity.AcceptedAtUtc = acceptedAtUtc;
            question.AcceptedAnswerId = entity.Id;
            question.AcceptedAnswer = entity;
            question.AnsweredAtUtc = acceptedAtUtc;
            question.ResolvedAtUtc = acceptedAtUtc;
            question.Status = question.Status == QuestionStatus.Validated
                ? QuestionStatus.Validated
                : QuestionStatus.Answered;
            var acceptedActivity = new Activity
            {
                TenantId = tenantId,
                QuestionId = question.Id,
                Question = question,
                AnswerId = entity.Id,
                Answer = entity,
                Kind = ActivityKind.AnswerAccepted,
                ActorKind = ActorKind.Customer,
                ActorLabel = "customer",
                UserPrint = "customer",
                Ip = "127.0.0.1",
                UserAgent = "QnATest/1.0",
                OccurredAtUtc = acceptedAtUtc,
                CreatedBy = "test",
                UpdatedBy = "test"
            };
            question.Activities.Add(acceptedActivity);
            question.LastActivityAtUtc = acceptedActivity.OccurredAtUtc;
            dbContext.Activities.Add(acceptedActivity);
        }

        dbContext.Answers.Add(entity);
        await dbContext.SaveChangesAsync();

        return entity;
    }
}