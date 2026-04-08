using BaseFaq.Faq.Public.Business.Faq.Queries.GetFaq;
using BaseFaq.Faq.Public.Business.Faq.Queries.GetFaqList;
using BaseFaq.Faq.Public.Test.IntegrationTests.Helpers;
using BaseFaq.Models.Faq.Dtos.Faq;
using BaseFaq.Models.Faq.Enums;
using Xunit;

namespace BaseFaq.Faq.Public.Test.IntegrationTests.Tests.Faq;

public class FaqQueryTests
{
    [Fact]
    public async Task GetFaqList_FiltersByFaqIdsAndIncludesRelations()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.TenantId, "Public FAQ");
        var otherFaq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.TenantId, "Other");
        var contentRef = await TestDataFactory.SeedContentRefAsync(context.DbContext, context.TenantId);
        var tag = await TestDataFactory.SeedTagAsync(context.DbContext, context.TenantId);
        await TestDataFactory.SeedFaqItemAsync(context.DbContext, context.TenantId, faq.Id);
        await TestDataFactory.SeedFaqContentRefAsync(
            context.DbContext,
            context.TenantId,
            faq.Id,
            contentRef.Id);
        await TestDataFactory.SeedFaqTagAsync(context.DbContext, context.TenantId, faq.Id, tag.Id);
        await TestDataFactory.SeedFaqItemAsync(context.DbContext, context.TenantId, otherFaq.Id);

        var clientKeyContextService = new TestClientKeyContextService(context.ClientKey);
        var tenantClientKeyResolver =
            new TestTenantClientKeyResolver(context.TenantId, context.ClientKey);
        var handler = new FaqsGetFaqListQueryHandler(
            context.DbContext,
            clientKeyContextService,
            tenantClientKeyResolver,
            context.HttpContextAccessor);
        var request = new FaqsGetFaqListQuery
        {
            Request = new FaqGetAllRequestDto
            {
                SkipCount = 0,
                MaxResultCount = 10,
                FaqIds = [faq.Id],
                IncludeFaqItems = true,
                IncludeContentRefs = true,
                IncludeTags = true
            }
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(1, result.TotalCount);
        var dto = result.Items.Single();
        Assert.Equal(faq.Id, dto.Id);
        Assert.NotNull(dto.Items);
        Assert.Single(dto.Items!);
        Assert.NotNull(dto.ContentRefs);
        Assert.Single(dto.ContentRefs!);
        Assert.NotNull(dto.Tags);
        Assert.Single(dto.Tags!);
    }

    [Fact]
    public async Task GetFaqById_RespectsIncludeFlags()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.TenantId);
        var contentRef = await TestDataFactory.SeedContentRefAsync(context.DbContext, context.TenantId);
        var tag = await TestDataFactory.SeedTagAsync(context.DbContext, context.TenantId);
        await TestDataFactory.SeedFaqItemAsync(context.DbContext, context.TenantId, faq.Id);
        await TestDataFactory.SeedFaqContentRefAsync(
            context.DbContext,
            context.TenantId,
            faq.Id,
            contentRef.Id);
        await TestDataFactory.SeedFaqTagAsync(context.DbContext, context.TenantId, faq.Id, tag.Id);

        var clientKeyContextService = new TestClientKeyContextService(context.ClientKey);
        var tenantClientKeyResolver =
            new TestTenantClientKeyResolver(context.TenantId, context.ClientKey);
        var handler = new FaqsGetFaqQueryHandler(
            context.DbContext,
            clientKeyContextService,
            tenantClientKeyResolver,
            context.HttpContextAccessor);
        var request = new FaqsGetFaqQuery
        {
            Id = faq.Id,
            Request = new FaqGetRequestDto
            {
                IncludeFaqItems = true,
                IncludeContentRefs = false,
                IncludeTags = true
            }
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotNull(result!.Items);
        Assert.Single(result.Items!);
        Assert.Null(result.ContentRefs);
        Assert.NotNull(result.Tags);
        Assert.Single(result.Tags!);
    }

    [Fact]
    public async Task GetFaqList_AppliesSortingAndPagination()
    {
        using var context = TestContext.Create();
        await TestDataFactory.SeedFaqAsync(context.DbContext, context.TenantId, "Zulu");
        await TestDataFactory.SeedFaqAsync(context.DbContext, context.TenantId, "Bravo");
        await TestDataFactory.SeedFaqAsync(context.DbContext, context.TenantId, "Alpha");

        var clientKeyContextService = new TestClientKeyContextService(context.ClientKey);
        var tenantClientKeyResolver =
            new TestTenantClientKeyResolver(context.TenantId, context.ClientKey);
        var handler = new FaqsGetFaqListQueryHandler(
            context.DbContext,
            clientKeyContextService,
            tenantClientKeyResolver,
            context.HttpContextAccessor);
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
    public async Task GetFaqList_FallsBackToUpdatedDateSortWhenSortingFieldIsInvalid()
    {
        using var context = TestContext.Create();
        var first = await TestDataFactory.SeedFaqAsync(context.DbContext, context.TenantId, "First");
        await TestDataFactory.SeedFaqAsync(context.DbContext, context.TenantId, "Second");
        first.Status = first.Status == FaqStatus.Draft ? FaqStatus.Published : FaqStatus.Draft;
        await context.DbContext.SaveChangesAsync();

        var clientKeyContextService = new TestClientKeyContextService(context.ClientKey);
        var tenantClientKeyResolver =
            new TestTenantClientKeyResolver(context.TenantId, context.ClientKey);
        var handler = new FaqsGetFaqListQueryHandler(
            context.DbContext,
            clientKeyContextService,
            tenantClientKeyResolver,
            context.HttpContextAccessor);
        var request = new FaqsGetFaqListQuery
        {
            Request = new FaqGetAllRequestDto
            {
                SkipCount = 0,
                MaxResultCount = 10,
                Sorting = "nonexistent DESC"
            }
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(first.Id, result.Items[0].Id);
    }
}