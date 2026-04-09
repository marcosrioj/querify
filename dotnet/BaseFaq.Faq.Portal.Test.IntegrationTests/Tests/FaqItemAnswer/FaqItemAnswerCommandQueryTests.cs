using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Faq.Portal.Business.FaqItemAnswer.Commands.CreateFaqItemAnswer;
using BaseFaq.Faq.Portal.Business.FaqItemAnswer.Commands.DeleteFaqItemAnswer;
using BaseFaq.Faq.Portal.Business.FaqItemAnswer.Commands.UpdateFaqItemAnswer;
using BaseFaq.Faq.Portal.Business.FaqItemAnswer.Queries.GetFaqItemAnswer;
using BaseFaq.Faq.Portal.Business.FaqItemAnswer.Queries.GetFaqItemAnswerList;
using BaseFaq.Faq.Portal.Test.IntegrationTests.Helpers;
using BaseFaq.Models.Faq.Dtos.FaqItemAnswer;
using Xunit;

namespace BaseFaq.Faq.Portal.Test.IntegrationTests.Tests.FaqItemAnswer;

public class FaqItemAnswerCommandQueryTests
{
    [Fact]
    public async Task CreateFaqItemAnswer_PersistsEntityAndReturnsId()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var faqItem = await TestDataFactory.SeedFaqItemAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faq.Id);

        var handler = new FaqItemAnswersCreateFaqItemAnswerCommandHandler(
            context.DbContext,
            context.SessionService);
        var id = await handler.Handle(new FaqItemAnswersCreateFaqItemAnswerCommand
        {
            ShortAnswer = "Alternative short answer",
            Answer = "Alternative long answer",
            Sort = 2,
            IsActive = true,
            FaqItemId = faqItem.Id
        }, CancellationToken.None);

        var answer = await context.DbContext.FaqItemAnswers.FindAsync(id);
        Assert.NotNull(answer);
        Assert.Equal("Alternative short answer", answer!.ShortAnswer);
        Assert.Equal(faqItem.Id, answer.FaqItemId);
        Assert.Equal(context.SessionService.TenantId, answer.TenantId);
    }

    [Fact]
    public async Task UpdateFaqItemAnswer_UpdatesExistingEntity()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var faqItem = await TestDataFactory.SeedFaqItemAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faq.Id);
        var faqItemAnswer = await TestDataFactory.SeedFaqItemAnswerAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faqItem.Id);

        var handler = new FaqItemAnswersUpdateFaqItemAnswerCommandHandler(context.DbContext);
        await handler.Handle(new FaqItemAnswersUpdateFaqItemAnswerCommand
        {
            Id = faqItemAnswer.Id,
            ShortAnswer = "Updated short answer",
            Answer = "Updated long answer",
            Sort = 5,
            IsActive = false,
            FaqItemId = faqItem.Id
        }, CancellationToken.None);

        var updated = await context.DbContext.FaqItemAnswers.FindAsync(faqItemAnswer.Id);
        Assert.NotNull(updated);
        Assert.Equal("Updated short answer", updated!.ShortAnswer);
        Assert.Equal("Updated long answer", updated.Answer);
        Assert.Equal(5, updated.Sort);
        Assert.False(updated.IsActive);
    }

    [Fact]
    public async Task UpdateFaqItemAnswer_ThrowsWhenMissing()
    {
        using var context = TestContext.Create();
        var handler = new FaqItemAnswersUpdateFaqItemAnswerCommandHandler(context.DbContext);

        var exception = await Assert.ThrowsAsync<ApiErrorException>(() => handler.Handle(
            new FaqItemAnswersUpdateFaqItemAnswerCommand
            {
                Id = Guid.NewGuid(),
                ShortAnswer = "Missing",
                Answer = "Missing",
                Sort = 1,
                IsActive = true,
                FaqItemId = Guid.NewGuid()
            },
            CancellationToken.None));

        Assert.Equal(404, exception.ErrorCode);
    }

    [Fact]
    public async Task DeleteFaqItemAnswer_SoftDeletesEntity()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var faqItem = await TestDataFactory.SeedFaqItemAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faq.Id);
        var faqItemAnswer = await TestDataFactory.SeedFaqItemAnswerAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faqItem.Id);

        var handler = new FaqItemAnswersDeleteFaqItemAnswerCommandHandler(context.DbContext);
        await handler.Handle(new FaqItemAnswersDeleteFaqItemAnswerCommand { Id = faqItemAnswer.Id }, CancellationToken.None);

        context.DbContext.SoftDeleteFiltersEnabled = false;
        var deleted = await context.DbContext.FaqItemAnswers.FindAsync(faqItemAnswer.Id);
        Assert.NotNull(deleted);
        Assert.True(deleted!.IsDeleted);
    }

    [Fact]
    public async Task GetFaqItemAnswer_ReturnsDto()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var faqItem = await TestDataFactory.SeedFaqItemAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faq.Id);
        var faqItemAnswer = await TestDataFactory.SeedFaqItemAnswerAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faqItem.Id,
            shortAnswer: "Portal answer");

        var handler = new FaqItemAnswersGetFaqItemAnswerQueryHandler(context.DbContext);
        var result = await handler.Handle(
            new FaqItemAnswersGetFaqItemAnswerQuery { Id = faqItemAnswer.Id },
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(faqItemAnswer.Id, result!.Id);
        Assert.Equal("Portal answer", result.ShortAnswer);
        Assert.Equal(faqItem.Id, result.FaqItemId);
    }

    [Fact]
    public async Task GetFaqItemAnswerList_FiltersAndSortsItems()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var faqItemA = await TestDataFactory.SeedFaqItemAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faq.Id,
            question: "Question A");
        var faqItemB = await TestDataFactory.SeedFaqItemAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faq.Id,
            question: "Question B");

        await TestDataFactory.SeedFaqItemAnswerAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faqItemA.Id,
            shortAnswer: "Bravo",
            answer: "Billing instructions",
            sort: 2,
            voteScore: 5,
            isActive: true);
        await TestDataFactory.SeedFaqItemAnswerAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faqItemB.Id,
            shortAnswer: "Alpha",
            answer: "Support instructions",
            sort: 1,
            voteScore: 1,
            isActive: false);

        var handler = new FaqItemAnswersGetFaqItemAnswerListQueryHandler(context.DbContext);
        var result = await handler.Handle(new FaqItemAnswersGetFaqItemAnswerListQuery
        {
            Request = new FaqItemAnswerGetAllRequestDto
            {
                SkipCount = 0,
                MaxResultCount = 10,
                FaqItemId = faqItemA.Id,
                SearchText = "billing",
                IsActive = true,
                Sorting = "votescore DESC"
            }
        }, CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal("Bravo", result.Items[0].ShortAnswer);
    }
}
