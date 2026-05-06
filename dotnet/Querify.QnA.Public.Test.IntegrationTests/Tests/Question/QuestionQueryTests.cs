using System.Text.Json;
using Querify.Common.Infrastructure.Core.Services;
using Querify.Models.QnA.Dtos.Question;
using Querify.Models.QnA.Enums;
using Querify.QnA.Public.Business.Question.Commands.CreateQuestion;
using Querify.QnA.Public.Business.Question.Queries.GetQuestion;
using Querify.QnA.Public.Test.IntegrationTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Querify.QnA.Public.Test.IntegrationTests.Tests.Question;

public class QuestionQueryTests
{
    [Fact]
    public async Task CreateQuestion_AppendsCreatedActivityWithPublicUserPrint()
    {
        using var context = TestContext.Create();
        var space = await TestDataFactory.SeedSpaceAsync(context.DbContext, context.TenantId);
        var handler = new QuestionsCreateQuestionCommandHandler(
            context.DbContext,
            new TestClientKeyContextService(context.ClientKey),
            new TestTenantClientKeyResolver(context.TenantId, context.ClientKey),
            context.SessionService,
            new ClaimService(context.HttpContextAccessor),
            context.HttpContextAccessor);

        var questionId = await handler.Handle(new QuestionsCreateQuestionCommand
        {
            Request = new QuestionCreateRequestDto
            {
                SpaceId = space.Id,
                Title = "Can I invite teammates?",
                Summary = "Team invitations",
                ContextNote = "Asked from public help center",
                Status = QuestionStatus.Active,
                Visibility = VisibilityScope.Public,
                OriginChannel = ChannelKind.Widget,
                Sort = 3
            }
        }, CancellationToken.None);

        var activity = await context.DbContext.Activities
            .SingleAsync(entity => entity.QuestionId == questionId && entity.Kind == ActivityKind.QuestionCreated);

        Assert.Contains(activity.UserPrint, activity.Notes);

        using var metadata = JsonDocument.Parse(activity.MetadataJson!);
        var metadataContext = metadata.RootElement.GetProperty("Context");
        Assert.Equal("Public", metadataContext.GetProperty("ActorSource").GetString());
        Assert.Equal(activity.UserPrint, metadataContext.GetProperty("ActorUserId").GetString());
        Assert.Equal(activity.UserPrint, metadataContext.GetProperty("ActorUserName").GetString());
        Assert.NotEqual(context.SessionService.UserName, metadataContext.GetProperty("ActorUserName").GetString());
    }

    [Fact]
    public async Task GetQuestionById_ReturnsOnlyPublicAnswersAndAcceptedAnswer()
    {
        using var context = TestContext.Create();
        var space = await TestDataFactory.SeedSpaceAsync(context.DbContext, context.TenantId);
        var question = await TestDataFactory.SeedQuestionAsync(context.DbContext, context.TenantId, space.Id);
        var accepted = await TestDataFactory.SeedAnswerAsync(
            context.DbContext,
            context.TenantId,
            question.Id,
            "Public accepted answer",
            accept: true);
        await TestDataFactory.SeedAnswerAsync(
            context.DbContext,
            context.TenantId,
            question.Id,
            "Internal answer",
            AnswerStatus.Active,
            VisibilityScope.Internal,
            rank: 9);
        await TestDataFactory.SeedAnswerAsync(
            context.DbContext,
            context.TenantId,
            question.Id,
            "Authenticated answer",
            AnswerStatus.Active,
            VisibilityScope.Authenticated,
            rank: 10);

        var handler = new QuestionsGetQuestionQueryHandler(
            context.DbContext,
            new TestClientKeyContextService(context.ClientKey),
            new TestTenantClientKeyResolver(context.TenantId, context.ClientKey),
            context.HttpContextAccessor);
        var result = await handler.Handle(new QuestionsGetQuestionQuery
        {
            Id = question.Id,
            Request = new QuestionGetRequestDto()
        }, CancellationToken.None);

        Assert.NotNull(result.AcceptedAnswer);
        Assert.Equal(accepted.Id, result.AcceptedAnswer!.Id);
        Assert.Single(result.Answers);
        Assert.Equal("Public accepted answer", result.Answers[0].Headline);
    }
}
