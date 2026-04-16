using BaseFaq.Models.QnA.Dtos.Answer;
using BaseFaq.Models.QnA.Dtos.Question;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Portal.Business.Answer.Commands.AddSource;
using BaseFaq.QnA.Portal.Business.Answer.Queries.GetAnswer;
using BaseFaq.QnA.Portal.Business.Question.Commands.AddSource;
using BaseFaq.QnA.Portal.Business.Question.Commands.AddTopic;
using BaseFaq.QnA.Portal.Business.Question.Queries.GetQuestion;
using BaseFaq.QnA.Portal.Test.IntegrationTests.Helpers;
using Xunit;

namespace BaseFaq.QnA.Portal.Test.IntegrationTests.Tests.Relationships;

public class RelationshipMappingTests
{
    [Fact]
    public async Task Question_Query_ReturnsTopicAndSourceRelationships()
    {
        using var context = TestContext.Create();
        var tenantId = context.SessionService.TenantId;
        var space = await TestDataFactory.SeedQuestionSpaceAsync(context.DbContext, tenantId);
        var question = await TestDataFactory.SeedQuestionAsync(context.DbContext, tenantId, space.Id);
        var topic = await TestDataFactory.SeedTopicAsync(context.DbContext, tenantId, "billing");
        var source = await TestDataFactory.SeedKnowledgeSourceAsync(context.DbContext, tenantId);

        var addTopicHandler = new QuestionsAddTopicCommandHandler(context.DbContext, context.SessionService);
        await addTopicHandler.Handle(new QuestionsAddTopicCommand
        {
            Request = new QuestionTopicCreateRequestDto
            {
                QuestionId = question.Id,
                TopicId = topic.Id
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
                Label = "Primary source",
                Order = 1,
                ConfidenceScore = 95,
                IsPrimary = true
            }
        }, CancellationToken.None);

        var queryHandler = new QuestionsGetQuestionQueryHandler(context.DbContext, context.SessionService);
        var result =
            await queryHandler.Handle(new QuestionsGetQuestionQuery { Id = question.Id }, CancellationToken.None);

        Assert.Single(result.Topics);
        Assert.Equal(topic.Id, result.Topics[0].Id);
        Assert.Single(result.Sources);
        Assert.Equal(source.Id, result.Sources[0].SourceId);
        Assert.Equal("Primary source", result.Sources[0].Label);
    }

    [Fact]
    public async Task Answer_Query_ReturnsSourceRelationship()
    {
        using var context = TestContext.Create();
        var tenantId = context.SessionService.TenantId;
        var space = await TestDataFactory.SeedQuestionSpaceAsync(context.DbContext, tenantId);
        var question = await TestDataFactory.SeedQuestionAsync(context.DbContext, tenantId, space.Id);
        var answer = await TestDataFactory.SeedAnswerAsync(context.DbContext, tenantId, question.Id);
        var source = await TestDataFactory.SeedKnowledgeSourceAsync(context.DbContext, tenantId);

        var addSourceHandler = new AnswersAddSourceCommandHandler(context.DbContext, context.SessionService);
        await addSourceHandler.Handle(new AnswersAddSourceCommand
        {
            Request = new AnswerSourceLinkCreateRequestDto
            {
                AnswerId = answer.Id,
                SourceId = source.Id,
                Role = SourceRole.Evidence,
                Label = "Evidence source",
                Order = 1,
                ConfidenceScore = 90,
                IsPrimary = true
            }
        }, CancellationToken.None);

        var queryHandler = new AnswersGetAnswerQueryHandler(context.DbContext, context.SessionService);
        var result = await queryHandler.Handle(new AnswersGetAnswerQuery { Id = answer.Id }, CancellationToken.None);

        Assert.Single(result.Sources);
        Assert.Equal(source.Id, result.Sources[0].SourceId);
        Assert.Equal("Evidence source", result.Sources[0].Label);
    }
}