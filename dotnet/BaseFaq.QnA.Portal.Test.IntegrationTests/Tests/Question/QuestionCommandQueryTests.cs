using BaseFaq.Models.QnA.Dtos.Question;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Portal.Business.Question.Commands.AddSource;
using BaseFaq.QnA.Portal.Business.Question.Commands.AddTopic;
using BaseFaq.QnA.Portal.Business.Question.Commands.ApproveQuestion;
using BaseFaq.QnA.Portal.Business.Question.Commands.CreateQuestion;
using BaseFaq.QnA.Portal.Business.Question.Commands.DeleteQuestion;
using BaseFaq.QnA.Portal.Business.Question.Commands.EscalateQuestion;
using BaseFaq.QnA.Portal.Business.Question.Commands.RejectQuestion;
using BaseFaq.QnA.Portal.Business.Question.Commands.RemoveSource;
using BaseFaq.QnA.Portal.Business.Question.Commands.RemoveTopic;
using BaseFaq.QnA.Portal.Business.Question.Commands.SubmitQuestion;
using BaseFaq.QnA.Portal.Business.Question.Commands.UpdateQuestion;
using BaseFaq.QnA.Portal.Business.Question.Queries.GetQuestion;
using BaseFaq.QnA.Portal.Business.Question.Queries.GetQuestionList;
using BaseFaq.QnA.Portal.Test.IntegrationTests.Helpers;
using Xunit;

namespace BaseFaq.QnA.Portal.Test.IntegrationTests.Tests.Question;

public class QuestionCommandQueryTests
{
    [Fact]
    public async Task UpdateQuestion_AcceptsAnswerAndWritesActivity()
    {
        using var context = TestContext.Create();
        var space = await TestDataFactory.SeedQuestionSpaceAsync(context.DbContext, context.SessionService.TenantId);
        var question = await TestDataFactory.SeedQuestionAsync(
            context.DbContext,
            context.SessionService.TenantId,
            space.Id,
            visibility: VisibilityScope.PublicIndexed);
        var acceptedAnswer = await TestDataFactory.SeedAnswerAsync(
            context.DbContext,
            context.SessionService.TenantId,
            question.Id,
            visibility: VisibilityScope.PublicIndexed,
            accept: false);
        var updateHandler = new QuestionsUpdateQuestionCommandHandler(context.DbContext, context.SessionService);
        await updateHandler.Handle(new QuestionsUpdateQuestionCommand
        {
            Id = question.Id,
            Request = new QuestionUpdateRequestDto
            {
                SpaceId = question.SpaceId,
                Title = question.Title,
                Key = question.Key,
                Summary = question.Summary,
                ContextNote = question.ContextNote,
                ThreadSummary = question.ThreadSummary,
                Kind = question.Kind,
                Status = question.Status,
                Visibility = question.Visibility,
                OriginChannel = question.OriginChannel,
                Language = question.Language,
                ProductScope = question.ProductScope,
                JourneyScope = question.JourneyScope,
                AudienceScope = question.AudienceScope,
                ContextKey = question.ContextKey,
                OriginUrl = question.OriginUrl,
                OriginReference = question.OriginReference,
                ConfidenceScore = question.ConfidenceScore,
                AcceptedAnswerId = acceptedAnswer.Id
            }
        }, CancellationToken.None);

        var getHandler = new QuestionsGetQuestionQueryHandler(context.DbContext, context.SessionService);
        var result = await getHandler.Handle(new QuestionsGetQuestionQuery { Id = question.Id }, CancellationToken.None);

        Assert.Equal(acceptedAnswer.Id, result.AcceptedAnswerId);
        Assert.Equal(QuestionStatus.Answered, result.Status);
        Assert.NotNull(result.AcceptedAnswer);
        Assert.True(result.AcceptedAnswer!.IsAccepted);
        Assert.Contains(result.Activity, activity => activity.Kind == ActivityKind.AnswerAccepted);
    }
}
