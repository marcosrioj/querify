using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Portal.Test.IntegrationTests.Helpers;
using Xunit;
using AnswerEntity = BaseFaq.QnA.Common.Persistence.QnADb.Entities.Answer;
using SourceEntity = BaseFaq.QnA.Common.Persistence.QnADb.Entities.Source;
using QuestionEntity = BaseFaq.QnA.Common.Persistence.QnADb.Entities.Question;

namespace BaseFaq.QnA.Portal.Test.IntegrationTests.Tests.BusinessRules;

public class EntityConstraintsTests
{
    [Fact]
    public async Task Question_ThrowsWhenSpaceTenantDoesNotMatch()
    {
        using var context = TestContext.Create();
        var otherTenantId = Guid.NewGuid();
        var space = await TestDataFactory.SeedSpaceAsync(context.DbContext, otherTenantId);

        var question = new QuestionEntity
        {
            TenantId = context.SessionService.TenantId,
            SpaceId = space.Id,
            Title = "Cross-tenant question",
            Summary = "Summary",
            ContextNote = "Context",
            Status = QuestionStatus.Active,
            Visibility = VisibilityScope.Authenticated,
            OriginChannel = ChannelKind.Manual,
            AiConfidenceScore = 50,
            FeedbackScore = 0,
            Sort = 0,
            CreatedBy = "test",
            UpdatedBy = "test"
        };

        context.DbContext.Questions.Add(question);

        await Assert.ThrowsAsync<InvalidOperationException>(() => context.DbContext.SaveChangesAsync());
    }

    [Fact]
    public async Task Answer_ThrowsWhenPublicVisibilityUsesDraftStatus()
    {
        using var context = TestContext.Create();
        var space = await TestDataFactory.SeedSpaceAsync(context.DbContext, context.SessionService.TenantId);
        var question = await TestDataFactory.SeedQuestionAsync(
            context.DbContext,
            context.SessionService.TenantId,
            space.Id,
            visibility: VisibilityScope.Public);

        var answer = new AnswerEntity
        {
            TenantId = context.SessionService.TenantId,
            QuestionId = question.Id,
            Headline = "Draft answer",
            Body = "Body",
            Kind = AnswerKind.Official,
            Status = AnswerStatus.Draft,
            Visibility = VisibilityScope.Public,
            AiConfidenceScore = 50,
            Score = 1,
            Sort = 1,
            CreatedBy = "test",
            UpdatedBy = "test"
        };

        context.DbContext.Answers.Add(answer);

        await Assert.ThrowsAsync<InvalidOperationException>(() => context.DbContext.SaveChangesAsync());
    }

    [Fact]
    public async Task Source_ThrowsWhenInternalNoteIsPublic()
    {
        using var context = TestContext.Create();

        var source = new SourceEntity
        {
            TenantId = context.SessionService.TenantId,
            Kind = SourceKind.InternalNote,
            Locator = "internal://note/1",
            Label = "Internal note",
            Language = "en-US",
            Checksum = "sha256:internal-note-1",
            Visibility = VisibilityScope.Public,
            LastVerifiedAtUtc = DateTime.UtcNow,
            CreatedBy = "test",
            UpdatedBy = "test"
        };

        context.DbContext.Sources.Add(source);

        await Assert.ThrowsAsync<InvalidOperationException>(() => context.DbContext.SaveChangesAsync());
    }

    [Fact]
    public async Task Activity_ThrowsWhenExistingActivityIsModified()
    {
        using var context = TestContext.Create();
        var space = await TestDataFactory.SeedSpaceAsync(context.DbContext, context.SessionService.TenantId);
        var question =
            await TestDataFactory.SeedQuestionAsync(context.DbContext, context.SessionService.TenantId, space.Id);

        var activity = question.Activities.Single();
        activity.Notes = "Edited after creation";

        await Assert.ThrowsAsync<InvalidOperationException>(() => context.DbContext.SaveChangesAsync());
    }
}
