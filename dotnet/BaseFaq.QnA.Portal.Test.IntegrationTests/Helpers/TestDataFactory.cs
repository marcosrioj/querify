using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Test.IntegrationTests.Helpers;

public static class TestDataFactory
{
    public static async Task<QuestionSpace> SeedQuestionSpaceAsync(
        QnADbContext dbContext,
        Guid tenantId,
        string? name = null,
        string? key = null,
        VisibilityScope visibility = VisibilityScope.Internal,
        SearchMarkupMode searchMarkupMode = SearchMarkupMode.Off,
        bool acceptsQuestions = true,
        bool acceptsAnswers = true)
    {
        var entity = new QuestionSpace
        {
            TenantId = tenantId,
            Name = name ?? "Support Questions",
            Key = key ?? $"space-{Guid.NewGuid():N}".Substring(0, 12),
            DefaultLanguage = "en-US",
            Kind = SpaceKind.CuratedKnowledge,
            Summary = "Support knowledge",
            ModerationPolicy = ModerationPolicy.PreModeration,
            AcceptsQuestions = acceptsQuestions,
            AcceptsAnswers = acceptsAnswers,
            RequiresQuestionReview = true,
            RequiresAnswerReview = true,
            Visibility = visibility,
            SearchMarkupMode = searchMarkupMode,
            PublishedAtUtc = visibility is VisibilityScope.Public or VisibilityScope.PublicIndexed
                ? DateTime.UtcNow
                : null,
            CreatedBy = "test",
            UpdatedBy = "test"
        };

        dbContext.QuestionSpaces.Add(entity);
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
        VisibilityScope visibility = VisibilityScope.Internal)
    {
        var space = await dbContext.QuestionSpaces
            .Include(entity => entity.Questions)
            .SingleAsync(entity => entity.Id == spaceId);

        var entity = new Question
        {
            TenantId = tenantId,
            SpaceId = space.Id,
            Space = space,
            Title = title ?? "How do I reset my password?",
            Key = key ?? $"question-{Guid.NewGuid():N}".Substring(0, 14),
            Summary = "Short summary",
            ContextNote = "Context",
            ThreadSummary = "Thread summary",
            Kind = QuestionKind.Curated,
            Status = status,
            Visibility = visibility,
            OriginChannel = ChannelKind.Manual,
            Language = "en-US",
            ProductScope = "Portal",
            JourneyScope = "Onboarding",
            AudienceScope = "Customer",
            ContextKey = "default",
            ConfidenceScore = 85,
            RevisionNumber = 1,
            ValidatedAtUtc = status == QuestionStatus.Validated ? DateTime.UtcNow : null,
            CreatedBy = "test",
            UpdatedBy = "test"
        };
        space.Questions.Add(entity);
        entity.Activities.Add(new ThreadActivity
        {
            TenantId = tenantId,
            Question = entity,
            QuestionId = entity.Id,
            Kind = ActivityKind.QuestionCreated,
            ActorKind = ActorKind.Moderator,
            ActorLabel = "tester",
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
        VisibilityScope visibility = VisibilityScope.Internal,
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
            Headline = headline ?? "Use the reset link from the sign-in page.",
            Body = "Click reset and follow the emailed link.",
            Kind = AnswerKind.Official,
            Language = "en-US",
            ContextKey = "default",
            ApplicabilityRulesJson = "{\"channel\":\"portal\"}",
            ConfidenceScore = 92,
            TrustNote = "Trusted",
            EvidenceSummary = "Backed by docs",
            Rank = rank,
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
        var createdActivity = new ThreadActivity
        {
            TenantId = tenantId,
            QuestionId = question.Id,
            Question = question,
            AnswerId = entity.Id,
            Answer = entity,
            Kind = ActivityKind.AnswerCreated,
            ActorKind = ActorKind.Moderator,
            ActorLabel = "tester",
            OccurredAtUtc = DateTime.UtcNow,
            CreatedBy = "test",
            UpdatedBy = "test"
        };
        question.Activities.Add(createdActivity);
        question.LastActivityAtUtc = createdActivity.OccurredAtUtc;
        dbContext.ThreadActivities.Add(createdActivity);

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
            var acceptedActivity = new ThreadActivity
            {
                TenantId = tenantId,
                QuestionId = question.Id,
                Question = question,
                AnswerId = entity.Id,
                Answer = entity,
                Kind = ActivityKind.AnswerAccepted,
                ActorKind = ActorKind.Moderator,
                ActorLabel = "tester",
                OccurredAtUtc = acceptedAtUtc,
                CreatedBy = "test",
                UpdatedBy = "test"
            };
            question.Activities.Add(acceptedActivity);
            question.LastActivityAtUtc = acceptedActivity.OccurredAtUtc;
            dbContext.ThreadActivities.Add(acceptedActivity);
        }

        dbContext.Answers.Add(entity);
        await dbContext.SaveChangesAsync();

        return entity;
    }

    public static async Task<Tag> SeedTagAsync(QnADbContext dbContext, Guid tenantId, string? name = null)
    {
        var entity = new Tag
        {
            TenantId = tenantId,
            Name = name ?? "billing",
            CreatedBy = "test",
            UpdatedBy = "test"
        };
        dbContext.Tags.Add(entity);
        await dbContext.SaveChangesAsync();
        return entity;
    }

    public static async Task<KnowledgeSource> SeedKnowledgeSourceAsync(
        QnADbContext dbContext,
        Guid tenantId,
        string? locator = null,
        VisibilityScope visibility = VisibilityScope.Internal)
    {
        var entity = new KnowledgeSource
        {
            TenantId = tenantId,
            Kind = SourceKind.Article,
            Locator = locator ?? "https://example.test/doc/1",
            Label = "Reset password doc",
            Scope = "Section 1",
            SystemName = "Docs",
            ExternalId = "DOC-1",
            Language = "en-US",
            MediaType = "text/html",
            MetadataJson = "{\"type\":\"doc\"}",
            IsAuthoritative = true,
            LastVerifiedAtUtc = DateTime.UtcNow,
            Visibility = visibility,
            AllowsPublicCitation = visibility != VisibilityScope.Internal,
            AllowsPublicExcerpt = visibility != VisibilityScope.Internal,
            CreatedBy = "test",
            UpdatedBy = "test"
        };
        dbContext.KnowledgeSources.Add(entity);
        await dbContext.SaveChangesAsync();
        return entity;
    }
}
