using Querify.Models.QnA.Dtos.Search;
using Querify.Models.QnA.Enums;
using Querify.QnA.Public.Business.Search.Queries.Search;
using Querify.QnA.Public.Test.IntegrationTests.Helpers;
using Xunit;

namespace Querify.QnA.Public.Test.IntegrationTests.Tests.Search;

public class QnASearchQueryTests
{
    [Fact]
    public async Task Search_FiltersByTenantVisibilityStatusSpaceAndSearchText()
    {
        using var context = TestContext.Create();
        var targetSpace = await TestDataFactory.SeedSpaceAsync(
            context.DbContext,
            context.TenantId,
            "Security Operations",
            "security-operations",
            VisibilityScope.Internal);
        var targetQuestion = await TestDataFactory.SeedQuestionAsync(
            context.DbContext,
            context.TenantId,
            targetSpace.Id,
            "How do I rotate API credentials?",
            status: QuestionStatus.Active,
            visibility: VisibilityScope.Internal);
        var acceptedAnswer = await TestDataFactory.SeedAnswerAsync(
            context.DbContext,
            context.TenantId,
            targetQuestion.Id,
            "Rotate credentials from workspace settings",
            visibility: VisibilityScope.Internal,
            accept: true);
        await SeedNoiseAsync(context, targetSpace.Id);

        var handler = new QnASearchQueryHandler(context.DbContext, context.SessionService);
        var result = await handler.Handle(new QnASearchQuery
        {
            Request = new QnASearchRequestDto
            {
                SearchText = "credentials",
                SpaceId = targetSpace.Id,
                Status = QuestionStatus.Active,
                Visibility = VisibilityScope.Internal,
                MaxResultCount = 20
            }
        }, CancellationToken.None);

        var item = Assert.Single(result.Items);
        Assert.Equal(targetQuestion.Id, item.QuestionId);
        Assert.Equal(targetSpace.Id, item.SpaceId);
        Assert.Equal(acceptedAnswer.Id, item.AcceptedAnswerId);
        Assert.Equal("Security Operations", item.SpaceName);
        Assert.True(item.MatchedQuestion);
        Assert.True(item.MatchedAnswer);
    }

    [Fact]
    public async Task Search_MatchesAnswerAndTagText()
    {
        using var context = TestContext.Create();
        var space = await TestDataFactory.SeedSpaceAsync(
            context.DbContext,
            context.TenantId,
            "Billing Knowledge",
            "billing-knowledge",
            VisibilityScope.Internal);
        var question = await TestDataFactory.SeedQuestionAsync(
            context.DbContext,
            context.TenantId,
            space.Id,
            "Where can I update payment settings?",
            visibility: VisibilityScope.Internal);
        await TestDataFactory.SeedAnswerAsync(
            context.DbContext,
            context.TenantId,
            question.Id,
            "Use the account billing panel",
            visibility: VisibilityScope.Internal,
            accept: true);
        await AddQuestionTagAsync(context, question.Id, "renewal-runbook");

        var handler = new QnASearchQueryHandler(context.DbContext, context.SessionService);
        var answerResult = await handler.Handle(new QnASearchQuery
        {
            Request = new QnASearchRequestDto
            {
                SearchText = "billing panel",
                Visibility = VisibilityScope.Internal,
                MaxResultCount = 20
            }
        }, CancellationToken.None);
        var tagResult = await handler.Handle(new QnASearchQuery
        {
            Request = new QnASearchRequestDto
            {
                SearchText = "renewal-runbook",
                Visibility = VisibilityScope.Internal,
                MaxResultCount = 20
            }
        }, CancellationToken.None);

        var answerMatch = Assert.Single(answerResult.Items);
        Assert.Equal(question.Id, answerMatch.QuestionId);
        Assert.True(answerMatch.MatchedAnswer);

        var tagMatch = Assert.Single(tagResult.Items);
        Assert.Equal(question.Id, tagMatch.QuestionId);
        Assert.True(tagMatch.MatchedTag);
        Assert.Contains("renewal-runbook", tagMatch.Tags);
    }

    private static async Task SeedNoiseAsync(TestContext context, Guid targetSpaceId)
    {
        await TestDataFactory.SeedQuestionAsync(
            context.DbContext,
            context.TenantId,
            targetSpaceId,
            "How do I rotate API credentials publicly?",
            status: QuestionStatus.Active,
            visibility: VisibilityScope.Public);
        await TestDataFactory.SeedQuestionAsync(
            context.DbContext,
            context.TenantId,
            targetSpaceId,
            "How do I rotate API credentials in draft?",
            status: QuestionStatus.Draft,
            visibility: VisibilityScope.Internal);

        var otherSpace = await TestDataFactory.SeedSpaceAsync(
            context.DbContext,
            context.TenantId,
            "Other Operations",
            "other-operations",
            VisibilityScope.Internal);
        await TestDataFactory.SeedQuestionAsync(
            context.DbContext,
            context.TenantId,
            otherSpace.Id,
            "How do I rotate API credentials elsewhere?",
            status: QuestionStatus.Active,
            visibility: VisibilityScope.Internal);

        var otherTenantId = Guid.NewGuid();
        var otherTenantSpaceId = Guid.NewGuid();
        var otherTenantQuestionId = Guid.NewGuid();
        context.DbContext.Spaces.Add(new Querify.QnA.Common.Domain.Entities.Space
        {
            Id = otherTenantSpaceId,
            TenantId = otherTenantId,
            Name = "Other Tenant Security",
            Slug = $"other-tenant-{Guid.NewGuid():N}".Substring(0, 20),
            Language = "en-US",
            Status = SpaceStatus.Active,
            Visibility = VisibilityScope.Internal,
            AcceptsQuestions = true,
            AcceptsAnswers = true,
            CreatedBy = "test",
            UpdatedBy = "test"
        });
        context.DbContext.Questions.Add(new Querify.QnA.Common.Domain.Entities.Question
        {
            Id = otherTenantQuestionId,
            TenantId = otherTenantId,
            SpaceId = otherTenantSpaceId,
            Title = "How do I rotate API credentials for another tenant?",
            Status = QuestionStatus.Active,
            Visibility = VisibilityScope.Internal,
            OriginChannel = ChannelKind.Manual,
            AiConfidenceScore = 80,
            FeedbackScore = 0,
            Sort = 0,
            CreatedBy = "test",
            UpdatedBy = "test"
        });
        await context.DbContext.SaveChangesAsync();
        context.DbContext.ChangeTracker.Clear();
    }

    private static async Task AddQuestionTagAsync(TestContext context, Guid questionId, string tagName)
    {
        var tag = new Querify.QnA.Common.Domain.Entities.Tag
        {
            TenantId = context.TenantId,
            Name = tagName,
            CreatedBy = "test",
            UpdatedBy = "test"
        };
        context.DbContext.Tags.Add(tag);
        await context.DbContext.SaveChangesAsync();

        context.DbContext.QuestionTags.Add(new Querify.QnA.Common.Domain.Entities.QuestionTag
        {
            TenantId = context.TenantId,
            QuestionId = questionId,
            TagId = tag.Id,
            CreatedBy = "test",
            UpdatedBy = "test"
        });
        await context.DbContext.SaveChangesAsync();
        context.DbContext.ChangeTracker.Clear();
    }
}
