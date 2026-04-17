using BaseFaq.Models.QnA.Dtos.Answer;
using BaseFaq.Models.QnA.Dtos.Question;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Portal.Business.Answer.Commands.AddSource;
using BaseFaq.QnA.Portal.Business.Answer.Queries.GetAnswer;
using BaseFaq.QnA.Portal.Business.Question.Commands.AddSource;
using BaseFaq.QnA.Portal.Business.Question.Commands.AddTag;
using BaseFaq.QnA.Portal.Business.Question.Queries.GetQuestion;
using BaseFaq.QnA.Portal.Test.IntegrationTests.Helpers;
using Xunit;

namespace BaseFaq.QnA.Portal.Test.IntegrationTests.Tests.Relationships;

public class RelationshipMappingTests
{
    [Fact]
    public async Task Question_Query_ReturnsTagAndSourceRelationships()
    {
        using var context = TestContext.Create();
        var tenantId = context.SessionService.TenantId;
        var space = await TestDataFactory.SeedSpaceAsync(context.DbContext, tenantId);
        var question = await TestDataFactory.SeedQuestionAsync(context.DbContext, tenantId, space.Id);
        var tag = await TestDataFactory.SeedTagAsync(context.DbContext, tenantId, "billing");
        var source = await TestDataFactory.SeedSourceAsync(context.DbContext, tenantId);

        var addTagHandler = new QuestionsAddTagCommandHandler(context.DbContext, context.SessionService);
        await addTagHandler.Handle(new QuestionsAddTagCommand
        {
            Request = new QuestionTagCreateRequestDto
            {
                QuestionId = question.Id,
                TagId = tag.Id
            }
        }, CancellationToken.None);

        var addSourceHandler = new QuestionsAddSourceCommandHandler(context.DbContext, context.SessionService);
        await addSourceHandler.Handle(new QuestionsAddSourceCommand
        {
            Request = new QuestionSourceLinkCreateRequestDto
            {
                QuestionId = question.Id,
                SourceId = source.Id,
                Role = SourceRole.SupportingContext,
                Order = 1
            }
        }, CancellationToken.None);

        var queryHandler = new QuestionsGetQuestionQueryHandler(context.DbContext, context.SessionService);
        var result =
            await queryHandler.Handle(new QuestionsGetQuestionQuery { Id = question.Id }, CancellationToken.None);

        Assert.Single(result.Tags);
        Assert.Equal(tag.Id, result.Tags[0].Id);
        Assert.Single(result.Sources);
        Assert.Equal(source.Id, result.Sources[0].SourceId);
        Assert.Equal(SourceRole.SupportingContext, result.Sources[0].Role);
    }

    [Fact]
    public async Task Answer_Query_ReturnsSourceRelationship()
    {
        using var context = TestContext.Create();
        var tenantId = context.SessionService.TenantId;
        var space = await TestDataFactory.SeedSpaceAsync(context.DbContext, tenantId);
        var question = await TestDataFactory.SeedQuestionAsync(context.DbContext, tenantId, space.Id);
        var answer = await TestDataFactory.SeedAnswerAsync(context.DbContext, tenantId, question.Id);
        var source = await TestDataFactory.SeedSourceAsync(context.DbContext, tenantId);

        var addSourceHandler = new AnswersAddSourceCommandHandler(context.DbContext, context.SessionService);
        await addSourceHandler.Handle(new AnswersAddSourceCommand
        {
            Request = new AnswerSourceLinkCreateRequestDto
            {
                AnswerId = answer.Id,
                SourceId = source.Id,
                Role = SourceRole.Evidence,
                Order = 1
            }
        }, CancellationToken.None);

        var queryHandler = new AnswersGetAnswerQueryHandler(context.DbContext, context.SessionService);
        var result = await queryHandler.Handle(new AnswersGetAnswerQuery { Id = answer.Id }, CancellationToken.None);

        Assert.Single(result.Sources);
        Assert.Equal(source.Id, result.Sources[0].SourceId);
        Assert.Equal(SourceRole.Evidence, result.Sources[0].Role);
    }
}