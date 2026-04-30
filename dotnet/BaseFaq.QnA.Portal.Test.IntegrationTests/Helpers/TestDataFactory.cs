using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Helper.Activities;
using BaseFaq.QnA.Common.Persistence.QnADb.DbContext;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Test.IntegrationTests.Helpers;

public static class TestDataFactory
{
    public static async Task<Space> SeedSpaceAsync(
        QnADbContext dbContext,
        Guid tenantId,
        string? name = null,
        string? slug = null,
        VisibilityScope visibility = VisibilityScope.Internal,
        bool acceptsQuestions = true,
        bool acceptsAnswers = true)
    {
        var entity = new Space
        {
            TenantId = tenantId,
            Name = name ?? "Support Questions",
            Slug = slug ?? $"space-{Guid.NewGuid():N}".Substring(0, 12),
            Language = "en-US",
            Status = SpaceStatus.Active,
            Summary = "Support knowledge",
            AcceptsQuestions = acceptsQuestions,
            AcceptsAnswers = acceptsAnswers,
            Visibility = visibility,
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
        QuestionStatus status = QuestionStatus.Active,
        VisibilityScope visibility = VisibilityScope.Internal)
    {
        var space = await dbContext.Spaces
            .Include(entity => entity.Questions)
            .SingleAsync(entity => entity.Id == spaceId);

        var entity = new Question
        {
            TenantId = tenantId,
            SpaceId = space.Id,
            Space = space,
            Title = title ?? "How do I reset my password?",
            Summary = "Short summary",
            ContextNote = "Context",
            Status = status,
            Visibility = visibility,
            OriginChannel = ChannelKind.Manual,
            AiConfidenceScore = 85,
            FeedbackScore = 0,
            Sort = 0,
            CreatedBy = "test",
            UpdatedBy = "test"
        };
        space.Questions.Add(entity);
        entity.Activities.Add(new Activity
        {
            TenantId = tenantId,
            Question = entity,
            QuestionId = entity.Id,
            Kind = ActivityKindStatusMap.ForQuestionStatus(entity.Status),
            ActorKind = ActorKind.Moderator,
            ActorLabel = "tester",
            UserPrint = "tester",
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
        AnswerStatus status = AnswerStatus.Active,
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
            AiConfidenceScore = 92,
            ContextNote = "Trusted",
            Status = status,
            Visibility = visibility,
            Score = rank,
            Sort = rank,
            CreatedBy = "test",
            UpdatedBy = "test"
        };

        entity.Visibility = visibility;
        question.Answers.Add(entity);
        var createdActivity = new Activity
        {
            TenantId = tenantId,
            QuestionId = question.Id,
            Question = question,
            AnswerId = entity.Id,
            Answer = entity,
            Kind = ActivityKindStatusMap.ForAnswerStatus(entity.Status),
            ActorKind = ActorKind.Moderator,
            ActorLabel = "tester",
            UserPrint = "tester",
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
            question.AcceptedAnswerId = entity.Id;
            question.AcceptedAnswer = entity;
            question.Status = QuestionStatus.Active;
            question.LastActivityAtUtc = acceptedAtUtc;
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

    public static async Task<Source> SeedSourceAsync(
        QnADbContext dbContext,
        Guid tenantId,
        string? locator = null,
        VisibilityScope visibility = VisibilityScope.Internal)
    {
        var entity = new Source
        {
            TenantId = tenantId,
            Kind = SourceKind.Article,
            Locator = locator ?? "https://example.test/doc/1",
            Label = "Reset password doc",
            ContextNote = "Section 1",
            ExternalId = "DOC-1",
            Language = "en-US",
            MediaType = "text/html",
            Checksum = "sha256:test-source",
            MetadataJson = "{\"type\":\"doc\"}",
            LastVerifiedAtUtc = DateTime.UtcNow,
            Visibility = visibility,
            CreatedBy = "test",
            UpdatedBy = "test"
        };
        dbContext.Sources.Add(entity);
        await dbContext.SaveChangesAsync();
        return entity;
    }
}
