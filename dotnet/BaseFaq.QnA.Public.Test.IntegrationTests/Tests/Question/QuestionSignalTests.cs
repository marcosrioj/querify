using System.Net;
using BaseFaq.Common.Infrastructure.Core.Services;
using BaseFaq.Models.QnA.Dtos.Question;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Helper.Activities;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;
using BaseFaq.QnA.Public.Business.Question.Commands.CreateReport;
using BaseFaq.QnA.Public.Business.Question.Queries.GetQuestion;
using BaseFaq.QnA.Public.Test.IntegrationTests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BaseFaq.QnA.Public.Test.IntegrationTests.Tests.Question;

public class QuestionSignalTests
{
    [Fact]
    public async Task CreateReport_PersistsReportReceivedWithUserPrint()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse("192.0.2.48");
        httpContext.Request.Headers.UserAgent = "QnAPublicReport/1.0";

        using var context = TestContext.Create(httpContext: httpContext);
        var space = await TestDataFactory.SeedSpaceAsync(context.DbContext, context.TenantId);
        var question = await TestDataFactory.SeedQuestionAsync(context.DbContext, context.TenantId, space.Id);
        var expectedIdentity = ActivityIdentityResolver.ResolveActivityIdentity(
            context.SessionService,
            ActivityRequestInfo.GetRequiredIp(context.HttpContextAccessor.HttpContext!),
            ActivityRequestInfo.GetRequiredUserAgent(context.HttpContextAccessor.HttpContext!),
            new ClaimService(context.HttpContextAccessor).GetExternalUserId());

        var reportId = await new QuestionsCreateReportCommandHandler(
            context.DbContext,
            new TestClientKeyContextService(context.ClientKey),
            new TestTenantClientKeyResolver(context.TenantId, context.ClientKey),
            context.SessionService,
            new ClaimService(context.HttpContextAccessor),
            context.HttpContextAccessor).Handle(new QuestionsCreateReportCommand
        {
            Request = new QuestionReportCreateRequestDto
            {
                QuestionId = question.Id,
                Reason = "Incorrect content",
                Notes = "Needs moderation review"
            }
        }, CancellationToken.None);

        var result = await new QuestionsGetQuestionQueryHandler(
            context.DbContext,
            new TestClientKeyContextService(context.ClientKey),
            new TestTenantClientKeyResolver(context.TenantId, context.ClientKey),
            context.HttpContextAccessor).Handle(new QuestionsGetQuestionQuery
        {
            Id = question.Id,
            Request = new QuestionGetRequestDto { IncludeActivity = true }
        }, CancellationToken.None);

        var reportActivity = Assert.Single(result.Activity, item => item.Id == reportId);
        Assert.Equal(ActivityKind.ReportReceived, reportActivity.Kind);
        Assert.Equal(expectedIdentity.UserPrint, reportActivity.UserPrint);
        Assert.Equal(expectedIdentity.Ip, reportActivity.Ip);
        Assert.Equal(expectedIdentity.UserAgent, reportActivity.UserAgent);
        Assert.Equal(expectedIdentity.UserPrint, ActivitySignals.ParseReport(reportActivity.MetadataJson)?.UserPrint);
    }

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
