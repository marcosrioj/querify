using Querify.Models.QnA.Enums;
using Querify.QnA.Common.Domain.BusinessRules.Activities;
using Querify.QnA.Common.Persistence.QnADb.DbContext;
using Querify.QnA.Common.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Querify.QnA.Public.Test.IntegrationTests.Helpers;

public static class TestDataFactory
{
    public static async Task<Space> SeedSpaceAsync(
        QnADbContext dbContext,
        Guid tenantId,
        string? name = null,
        string? slug = null,
        VisibilityScope visibility = VisibilityScope.Public)
    {
        var entity = new Space
        {
            TenantId = tenantId,
            Name = name ?? "Public Support",
            Slug = slug ?? $"public-{Guid.NewGuid():N}".Substring(0, 12),
            Language = "en-US",
            Status = SpaceStatus.Active,
            Summary = "Public knowledge",
            AcceptsQuestions = true,
            AcceptsAnswers = true,
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
        VisibilityScope visibility = VisibilityScope.Public)
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
            Summary = "Public summary",
            ContextNote = "Public context",
            Status = status,
            Visibility = visibility,
            OriginChannel = ChannelKind.Widget,
            AiConfidenceScore = 90,
            FeedbackScore = 0,
            Sort = 0,
            CreatedBy = "test",
            UpdatedBy = "test"
        };
        space.Questions.Add(entity);
        entity.Activities.Add(new Activity
        {
            TenantId = tenantId,
            QuestionId = entity.Id,
            Question = entity,
            Kind = ActivityKindStatusMap.ForQuestionStatus(entity.Status),
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
        AnswerStatus status = AnswerStatus.Active,
        VisibilityScope visibility = VisibilityScope.Public,
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
            AiConfidenceScore = 94,
            ContextNote = "Reviewed",
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
            question.AcceptedAnswerId = entity.Id;
            question.AcceptedAnswer = entity;
            question.Status = QuestionStatus.Active;
            question.LastActivityAtUtc = acceptedAtUtc;
        }

        dbContext.Answers.Add(entity);
        await dbContext.SaveChangesAsync();

        return entity;
    }
}
