using Querify.Models.QnA.Dtos.Answer;
using Querify.Models.QnA.Dtos.Question;
using Querify.Models.QnA.Enums;
using Querify.QnA.Portal.Business.Answer.Commands.AddSource;
using Querify.QnA.Portal.Business.Answer.Queries.GetAnswerList;
using Querify.QnA.Portal.Business.Answer.Queries.GetAnswer;
using Querify.QnA.Portal.Business.Question.Commands.AddSource;
using Querify.QnA.Portal.Business.Question.Commands.AddTag;
using Querify.QnA.Portal.Business.Question.Queries.GetQuestion;
using Querify.QnA.Portal.Business.Question.Queries.GetQuestionList;
using Querify.QnA.Portal.Test.IntegrationTests.Helpers;
using Xunit;

namespace Querify.QnA.Portal.Test.IntegrationTests.Tests.Relationships;

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
                Role = SourceRole.Context,
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
        Assert.Equal(SourceRole.Context, result.Sources[0].Role);
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

    [Fact]
    public async Task QuestionList_FiltersBySourceAndTagRelationships()
    {
        using var context = TestContext.Create();
        var tenantId = context.SessionService.TenantId;
        var space = await TestDataFactory.SeedSpaceAsync(context.DbContext, tenantId);
        var matchingQuestion = await TestDataFactory.SeedQuestionAsync(
            context.DbContext,
            tenantId,
            space.Id,
            "How do I update billing?");
        await TestDataFactory.SeedQuestionAsync(
            context.DbContext,
            tenantId,
            space.Id,
            "How do I reset my password?");
        var tag = await TestDataFactory.SeedTagAsync(context.DbContext, tenantId, "billing");
        var source = await TestDataFactory.SeedSourceAsync(context.DbContext, tenantId);

        var addTagHandler = new QuestionsAddTagCommandHandler(context.DbContext, context.SessionService);
        await addTagHandler.Handle(new QuestionsAddTagCommand
        {
            Request = new QuestionTagCreateRequestDto
            {
                QuestionId = matchingQuestion.Id,
                TagId = tag.Id
            }
        }, CancellationToken.None);

        var addSourceHandler = new QuestionsAddSourceCommandHandler(context.DbContext, context.SessionService);
        await addSourceHandler.Handle(new QuestionsAddSourceCommand
        {
            Request = new QuestionSourceLinkCreateRequestDto
            {
                QuestionId = matchingQuestion.Id,
                SourceId = source.Id,
                Role = SourceRole.Context,
                Order = 1
            }
        }, CancellationToken.None);

        var listHandler = new QuestionsGetQuestionListQueryHandler(context.DbContext, context.SessionService);
        var sourceResult = await listHandler.Handle(new QuestionsGetQuestionListQuery
        {
            Request = new QuestionGetAllRequestDto
            {
                SourceId = source.Id,
                SkipCount = 0,
                MaxResultCount = 10,
                Sorting = "LastActivityAtUtc DESC"
            }
        }, CancellationToken.None);
        var tagResult = await listHandler.Handle(new QuestionsGetQuestionListQuery
        {
            Request = new QuestionGetAllRequestDto
            {
                TagId = tag.Id,
                SkipCount = 0,
                MaxResultCount = 10,
                Sorting = "LastActivityAtUtc DESC"
            }
        }, CancellationToken.None);

        Assert.Single(sourceResult.Items);
        Assert.Equal(matchingQuestion.Id, sourceResult.Items[0].Id);
        Assert.Single(tagResult.Items);
        Assert.Equal(matchingQuestion.Id, tagResult.Items[0].Id);
    }

    [Fact]
    public async Task AnswerList_FiltersBySourceRelationship()
    {
        using var context = TestContext.Create();
        var tenantId = context.SessionService.TenantId;
        var space = await TestDataFactory.SeedSpaceAsync(context.DbContext, tenantId);
        var question = await TestDataFactory.SeedQuestionAsync(context.DbContext, tenantId, space.Id);
        var matchingAnswer = await TestDataFactory.SeedAnswerAsync(
            context.DbContext,
            tenantId,
            question.Id,
            "Billing answer");
        await TestDataFactory.SeedAnswerAsync(
            context.DbContext,
            tenantId,
            question.Id,
            "Password answer");
        var source = await TestDataFactory.SeedSourceAsync(context.DbContext, tenantId);

        var addSourceHandler = new AnswersAddSourceCommandHandler(context.DbContext, context.SessionService);
        await addSourceHandler.Handle(new AnswersAddSourceCommand
        {
            Request = new AnswerSourceLinkCreateRequestDto
            {
                AnswerId = matchingAnswer.Id,
                SourceId = source.Id,
                Role = SourceRole.Evidence,
                Order = 1
            }
        }, CancellationToken.None);

        var listHandler = new AnswersGetAnswerListQueryHandler(context.DbContext, context.SessionService);
        var result = await listHandler.Handle(new AnswersGetAnswerListQuery
        {
            Request = new AnswerGetAllRequestDto
            {
                SourceId = source.Id,
                SkipCount = 0,
                MaxResultCount = 10,
                Sorting = "LastUpdatedAtUtc DESC"
            }
        }, CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal(matchingAnswer.Id, result.Items[0].Id);
    }
}
