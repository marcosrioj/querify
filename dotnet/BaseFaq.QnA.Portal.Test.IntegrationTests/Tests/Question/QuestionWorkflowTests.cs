using BaseFaq.Models.QnA.Dtos.Question;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Portal.Business.Question.Commands.UpdateQuestion;
using BaseFaq.QnA.Portal.Business.Question.Queries.GetQuestion;
using BaseFaq.QnA.Portal.Test.IntegrationTests.Helpers;
using Xunit;

namespace BaseFaq.QnA.Portal.Test.IntegrationTests.Tests.Question;

public class QuestionWorkflowTests
{
    [Fact]
    public async Task UpdateQuestion_MarksDuplicateThroughCanonicalRelationship()
    {
        using var context = TestContext.Create();
        var space = await TestDataFactory.SeedSpaceAsync(context.DbContext, context.SessionService.TenantId);
        var canonical = await TestDataFactory.SeedQuestionAsync(
            context.DbContext,
            context.SessionService.TenantId,
            space.Id,
            title: "Canonical billing question");
        var duplicate = await TestDataFactory.SeedQuestionAsync(
            context.DbContext,
            context.SessionService.TenantId,
            space.Id,
            title: "Repeated billing question",
            visibility: VisibilityScope.Public);

        await new QuestionsUpdateQuestionCommandHandler(
            context.DbContext,
            context.SessionService,
            context.HttpContextAccessor).Handle(new QuestionsUpdateQuestionCommand
        {
            Id = duplicate.Id,
            Request = new QuestionUpdateRequestDto
            {
                Title = duplicate.Title,
                Summary = duplicate.Summary,
                ContextNote = duplicate.ContextNote,
                Status = QuestionStatus.Active,
                Visibility = VisibilityScope.Public,
                OriginChannel = duplicate.OriginChannel,
                Sort = duplicate.Sort,
                DuplicateOfQuestionId = canonical.Id
            }
        }, CancellationToken.None);

        var result = await new QuestionsGetQuestionQueryHandler(
            context.DbContext,
            context.SessionService).Handle(new QuestionsGetQuestionQuery
        {
            Id = duplicate.Id
        }, CancellationToken.None);

        Assert.Equal(QuestionStatus.Active, result.Status);
        Assert.Equal(VisibilityScope.Authenticated, result.Visibility);
        Assert.Equal(canonical.Id, result.DuplicateOfQuestionId);
    }
}
