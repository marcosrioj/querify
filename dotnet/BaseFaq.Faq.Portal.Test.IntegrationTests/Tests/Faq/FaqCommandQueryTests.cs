using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Faq.Portal.Business.Faq.Commands.CreateFaq;
using BaseFaq.Faq.Portal.Business.Faq.Commands.DeleteFaq;
using BaseFaq.Faq.Portal.Business.Faq.Commands.UpdateFaq;
using BaseFaq.Faq.Portal.Business.Faq.Queries.GetFaq;
using BaseFaq.Faq.Portal.Business.Faq.Queries.GetFaqList;
using BaseFaq.Faq.Portal.Test.IntegrationTests.Helpers;
using BaseFaq.Models.Faq.Dtos.Faq;
using BaseFaq.Models.Faq.Enums;
using Xunit;

namespace BaseFaq.Faq.Portal.Test.IntegrationTests.Tests.Faq;

public class FaqCommandQueryTests
{
    [Fact]
    public async Task CreateFaq_PersistsEntityAndReturnsId()
    {
        using var context = TestContext.Create();

        var handler = new FaqsCreateFaqCommandHandler(context.DbContext, context.SessionService);
        var request = new FaqsCreateFaqCommand
        {
            Name = "Returns",
            Language = "en-US",
            Status = FaqStatus.Draft,
            SortStrategy = FaqSortStrategy.Sort,
            CtaEnabled = true,
            CtaTarget = CtaTarget.Blank
        };

        var id = await handler.Handle(request, CancellationToken.None);

        var faq = await context.DbContext.Faqs.FindAsync(id);
        Assert.NotNull(faq);
        Assert.Equal("Returns", faq!.Name);
        Assert.Equal("en-US", faq.Language);
        Assert.Equal(FaqStatus.Draft, faq.Status);
        Assert.Equal(FaqSortStrategy.Sort, faq.SortStrategy);
        Assert.True(faq.CtaEnabled);
        Assert.Equal(CtaTarget.Blank, faq.CtaTarget);
        Assert.Equal(context.SessionService.TenantId, faq.TenantId);
    }

    [Fact]
    public async Task UpdateFaq_UpdatesExistingFaq()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);

        var handler = new FaqsUpdateFaqCommandHandler(context.DbContext);
        var request = new FaqsUpdateFaqCommand
        {
            Id = faq.Id,
            Name = "Updated",
            Language = "en-GB",
            Status = FaqStatus.Published,
            SortStrategy = FaqSortStrategy.Vote,
            CtaEnabled = true,
            CtaTarget = CtaTarget.Blank
        };

        await handler.Handle(request, CancellationToken.None);

        var updated = await context.DbContext.Faqs.FindAsync(faq.Id);
        Assert.NotNull(updated);
        Assert.Equal("Updated", updated!.Name);
        Assert.Equal("en-GB", updated.Language);
        Assert.Equal(FaqStatus.Published, updated.Status);
        Assert.Equal(FaqSortStrategy.Vote, updated.SortStrategy);
        Assert.True(updated.CtaEnabled);
        Assert.Equal(CtaTarget.Blank, updated.CtaTarget);
    }

    [Fact]
    public async Task UpdateFaq_ThrowsWhenMissing()
    {
        using var context = TestContext.Create();
        var handler = new FaqsUpdateFaqCommandHandler(context.DbContext);
        var request = new FaqsUpdateFaqCommand
        {
            Id = Guid.NewGuid(),
            Name = "Missing",
            Language = "en-US",
            Status = FaqStatus.Draft,
            SortStrategy = FaqSortStrategy.Sort,
            CtaEnabled = false,
            CtaTarget = CtaTarget.Self
        };

        var exception =
            await Assert.ThrowsAsync<ApiErrorException>(() => handler.Handle(request, CancellationToken.None));

        Assert.Equal(404, exception.ErrorCode);
    }

    [Fact]
    public async Task DeleteFaq_SoftDeletesEntity()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);

        var handler = new FaqsDeleteFaqCommandHandler(context.DbContext);
        await handler.Handle(new FaqsDeleteFaqCommand { Id = faq.Id }, CancellationToken.None);

        context.DbContext.SoftDeleteFiltersEnabled = false;
        var deleted = await context.DbContext.Faqs.FindAsync(faq.Id);
        Assert.NotNull(deleted);
        Assert.True(deleted!.IsDeleted);
    }

    [Fact]
    public async Task GetFaq_ReturnsDto()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);

        var handler = new FaqsGetFaqQueryHandler(context.DbContext);
        var result = await handler.Handle(new FaqsGetFaqQuery { Id = faq.Id }, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(faq.Id, result!.Id);
        Assert.Equal(faq.Name, result.Name);
        Assert.Equal(faq.Language, result.Language);
        Assert.Equal(faq.Status, result.Status);
        Assert.Equal(faq.SortStrategy, result.SortStrategy);
        Assert.Equal(faq.CtaEnabled, result.CtaEnabled);
        Assert.Equal(faq.CtaTarget, result.CtaTarget);
    }

    [Fact]
    public async Task GetFaqList_ReturnsItemsSortedByUpdatedDate()
    {
        using var context = TestContext.Create();
        var first = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId, "Zulu");
        await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId, "Alpha");
        first.CtaEnabled = !first.CtaEnabled;
        await context.DbContext.SaveChangesAsync();

        var handler = new FaqsGetFaqListQueryHandler(context.DbContext);
        var request = new FaqsGetFaqListQuery
        {
            Request = new FaqGetAllRequestDto { SkipCount = 0, MaxResultCount = 10 }
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal("Zulu", result.Items[0].Name);
        Assert.Equal("Alpha", result.Items[1].Name);
    }

    [Fact]
    public async Task GetFaqList_AppliesSortingAndPagination()
    {
        using var context = TestContext.Create();
        await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId, "Zulu");
        await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId, "Bravo");
        await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId, "Alpha");

        var handler = new FaqsGetFaqListQueryHandler(context.DbContext);
        var request = new FaqsGetFaqListQuery
        {
            Request = new FaqGetAllRequestDto
            {
                SkipCount = 1,
                MaxResultCount = 1,
                Sorting = "name ASC"
            }
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(3, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal("Bravo", result.Items[0].Name);
    }

    [Fact]
    public async Task GetFaqList_FallsBackToUpdatedDateWhenSortingFieldIsInvalid()
    {
        using var context = TestContext.Create();
        var first = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId, "First");
        await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId, "Second");
        first.CtaEnabled = !first.CtaEnabled;
        await context.DbContext.SaveChangesAsync();

        var handler = new FaqsGetFaqListQueryHandler(context.DbContext);
        var request = new FaqsGetFaqListQuery
        {
            Request = new FaqGetAllRequestDto
            {
                SkipCount = 0,
                MaxResultCount = 10,
                Sorting = "invalidField DESC"
            }
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(first.Id, result.Items[0].Id);
    }
}
