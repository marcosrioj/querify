using Querify.Models.QnA.Dtos.Question;
using Querify.Models.QnA.Enums;
using Querify.QnA.Common.Domain.BusinessRules.Activities;
using Querify.QnA.Common.Domain.Entities;
using Querify.QnA.Public.Business.Question.Queries.GetQuestion;
using Querify.QnA.Public.Test.IntegrationTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Querify.QnA.Public.Test.IntegrationTests.Tests.Question;

public class QuestionSignalTests
{
    [Fact]
    public async Task GetQuestion_DeduplicatesVoteAndFeedbackByExplicitUserPrint()
    {
        using var context = TestContext.Create();
        var space = await TestDataFactory.SeedSpaceAsync(context.DbContext, context.TenantId);
        var seededQuestion = await TestDataFactory.SeedQuestionAsync(context.DbContext, context.TenantId, space.Id);
        var seededAnswer = await TestDataFactory.SeedAnswerAsync(
            context.DbContext,
            context.TenantId,
            seededQuestion.Id,
            "Public accepted answer",
            accept: true);

        context.DbContext.ChangeTracker.Clear();

        var question = await context.DbContext.Questions
            .Include(entity => entity.Activities)
            .SingleAsync(entity => entity.Id == seededQuestion.Id);
        var answer = await context.DbContext.Answers
            .SingleAsync(entity => entity.Id == seededAnswer.Id);
        var baseTime = DateTime.UtcNow;

        var feedbackUp = new Activity
        {
            TenantId = question.TenantId,
            QuestionId = question.Id,
            Question = question,
            Kind = ActivityKind.FeedbackReceived,
            ActorKind = ActorKind.Customer,
            ActorLabel = "legacy-feedback-a",
            UserPrint = "canonical-user",
            Ip = "192.0.2.60",
            UserAgent = "QnAFeedbackLegacy/1.0",
            MetadataJson = ActivitySignals.CreateFeedbackMetadata(
                "legacy-feedback-a",
                "192.0.2.60",
                "QnAFeedbackLegacy/1.0",
                true,
                null),
            OccurredAtUtc = baseTime,
            CreatedBy = "test",
            UpdatedBy = "test"
        };

        var feedbackDown = new Activity
        {
            TenantId = question.TenantId,
            QuestionId = question.Id,
            Question = question,
            Kind = ActivityKind.FeedbackReceived,
            ActorKind = ActorKind.Customer,
            ActorLabel = "legacy-feedback-b",
            UserPrint = "canonical-user",
            Ip = "192.0.2.60",
            UserAgent = "QnAFeedbackLegacy/1.1",
            MetadataJson = ActivitySignals.CreateFeedbackMetadata(
                "legacy-feedback-b",
                "192.0.2.60",
                "QnAFeedbackLegacy/1.1",
                false,
                "Not relevant"),
            OccurredAtUtc = baseTime.AddMinutes(1),
            CreatedBy = "test",
            UpdatedBy = "test"
        };

        var voteUp = new Activity
        {
            TenantId = question.TenantId,
            QuestionId = question.Id,
            Question = question,
            AnswerId = answer.Id,
            Answer = answer,
            Kind = ActivityKind.VoteReceived,
            ActorKind = ActorKind.Customer,
            ActorLabel = "legacy-vote-a",
            UserPrint = "canonical-user",
            Ip = "192.0.2.61",
            UserAgent = "QnAVoteLegacy/1.0",
            MetadataJson = ActivitySignals.CreateVoteMetadata(
                "legacy-vote-a",
                "192.0.2.61",
                "QnAVoteLegacy/1.0",
                1),
            OccurredAtUtc = baseTime.AddMinutes(2),
            CreatedBy = "test",
            UpdatedBy = "test"
        };

        var voteDown = new Activity
        {
            TenantId = question.TenantId,
            QuestionId = question.Id,
            Question = question,
            AnswerId = answer.Id,
            Answer = answer,
            Kind = ActivityKind.VoteReceived,
            ActorKind = ActorKind.Customer,
            ActorLabel = "legacy-vote-b",
            UserPrint = "canonical-user",
            Ip = "192.0.2.61",
            UserAgent = "QnAVoteLegacy/1.1",
            MetadataJson = ActivitySignals.CreateVoteMetadata(
                "legacy-vote-b",
                "192.0.2.61",
                "QnAVoteLegacy/1.1",
                -1),
            OccurredAtUtc = baseTime.AddMinutes(3),
            CreatedBy = "test",
            UpdatedBy = "test"
        };

        question.Activities.Add(feedbackUp);
        question.Activities.Add(feedbackDown);
        question.Activities.Add(voteUp);
        question.Activities.Add(voteDown);
        question.LastActivityAtUtc = voteDown.OccurredAtUtc;
        question.FeedbackScore = ActivitySignals.ComputeFeedbackScore(question.Activities.Select(activity =>
            new ActivitySignalEntry(
                activity.Kind,
                activity.AnswerId,
                activity.OccurredAtUtc,
                activity.UserPrint,
                activity.MetadataJson)));
        context.DbContext.Activities.AddRange(feedbackUp, feedbackDown, voteUp, voteDown);
        await context.DbContext.SaveChangesAsync();

        var result = await new QuestionsGetQuestionQueryHandler(
            context.DbContext,
            new TestClientKeyContextService(context.ClientKey),
            new TestTenantClientKeyResolver(context.TenantId, context.ClientKey),
            context.HttpContextAccessor).Handle(new QuestionsGetQuestionQuery
        {
            Id = question.Id,
            Request = new QuestionGetRequestDto()
        }, CancellationToken.None);

        Assert.Equal(-1, result.FeedbackScore);
        Assert.Equal(-1, result.AcceptedAnswer!.VoteScore);
    }
}
