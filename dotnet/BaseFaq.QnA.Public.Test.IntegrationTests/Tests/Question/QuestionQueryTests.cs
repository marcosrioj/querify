using BaseFaq.Models.QnA.Dtos.Question;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Public.Business.Question.Queries.GetQuestion;
using BaseFaq.QnA.Public.Business.Question.Queries.GetQuestionByKey;
using BaseFaq.QnA.Public.Business.Question.Queries.GetQuestionList;
using BaseFaq.QnA.Public.Test.IntegrationTests.Helpers;
using Xunit;

namespace BaseFaq.QnA.Public.Test.IntegrationTests.Tests.Question;

public class QuestionQueryTests
{
    [Fact]
    public async Task GetQuestionById_ReturnsOnlyPublicAnswersAndAcceptedAnswer()
    {
        using var context = TestContext.Create();
        var space = await TestDataFactory.SeedQuestionSpaceAsync(context.DbContext, context.TenantId);
        var question = await TestDataFactory.SeedQuestionAsync(context.DbContext, context.TenantId, space.Id);
        var accepted = await TestDataFactory.SeedAnswerAsync(
            context.DbContext,
            context.TenantId,
            question.Id,
            headline: "Public accepted answer",
            accept: true);
        await TestDataFactory.SeedAnswerAsync(
            context.DbContext,
            context.TenantId,
            question.Id,
            headline: "Internal draft answer",
            status: AnswerStatus.Draft,
            visibility: VisibilityScope.Internal,
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
