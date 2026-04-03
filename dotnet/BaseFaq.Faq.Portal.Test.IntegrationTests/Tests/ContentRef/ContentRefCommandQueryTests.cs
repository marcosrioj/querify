using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Faq.Portal.Business.ContentRef.Commands.CreateContentRef;
using BaseFaq.Faq.Portal.Business.ContentRef.Commands.DeleteContentRef;
using BaseFaq.Faq.Portal.Business.ContentRef.Commands.UpdateContentRef;
using BaseFaq.Faq.Portal.Business.ContentRef.Queries.GetContentRef;
using BaseFaq.Faq.Portal.Business.ContentRef.Queries.GetContentRefList;
using BaseFaq.Faq.Portal.Test.IntegrationTests.Helpers;
using BaseFaq.Models.Faq.Dtos.ContentRef;
using BaseFaq.Models.Faq.Enums;
using Xunit;

namespace BaseFaq.Faq.Portal.Test.IntegrationTests.Tests.ContentRef;

public class ContentRefCommandQueryTests
{
    [Fact]
    public async Task CreateContentRef_PersistsEntityAndReturnsId()
    {
        using var context = TestContext.Create();

        var handler = new ContentRefsCreateContentRefCommandHandler(context.DbContext, context.SessionService);
        var request = new ContentRefsCreateContentRefCommand
        {
            Kind = ContentRefKind.Web,
            Locator = "https://www.example.com/guide",
            Label = "Guide",
            Scope = "Section 2"
        };

        var id = await handler.Handle(request, CancellationToken.None);

        var contentRef = await context.DbContext.ContentRefs.FindAsync(id);
        Assert.NotNull(contentRef);
        Assert.Equal(ContentRefKind.Web, contentRef!.Kind);
        Assert.Equal("https://www.example.com/guide", contentRef.Locator);
        Assert.Equal("Guide", contentRef.Label);
        Assert.Equal("Section 2", contentRef.Scope);
        Assert.Equal(context.SessionService.TenantId, contentRef.TenantId);
    }

    [Fact]
    public async Task UpdateContentRef_UpdatesExistingContentRef()
    {
        using var context = TestContext.Create();
        var contentRef = await TestDataFactory.SeedContentRefAsync(
            context.DbContext,
            context.SessionService.TenantId,
            ContentRefKind.Manual,
            "manual-1");

        var handler = new ContentRefsUpdateContentRefCommandHandler(context.DbContext);
        var request = new ContentRefsUpdateContentRefCommand
        {
            Id = contentRef.Id,
            Kind = ContentRefKind.Pdf,
            Locator = "https://www.example.com/files/manual-2.pdf",
            Label = "Updated",
            Scope = "Chapter 3"
        };

        await handler.Handle(request, CancellationToken.None);

        var updated = await context.DbContext.ContentRefs.FindAsync(contentRef.Id);
        Assert.NotNull(updated);
        Assert.Equal(ContentRefKind.Pdf, updated!.Kind);
        Assert.Equal("https://www.example.com/files/manual-2.pdf", updated.Locator);
        Assert.Equal("Updated", updated.Label);
        Assert.Equal("Chapter 3", updated.Scope);
    }

    [Fact]
    public async Task UpdateContentRef_ThrowsWhenMissing()
    {
        using var context = TestContext.Create();
        var handler = new ContentRefsUpdateContentRefCommandHandler(context.DbContext);
        var request = new ContentRefsUpdateContentRefCommand
        {
            Id = Guid.NewGuid(),
            Kind = ContentRefKind.Other,
            Locator = "missing"
        };

        var exception =
            await Assert.ThrowsAsync<ApiErrorException>(() => handler.Handle(request, CancellationToken.None));

        Assert.Equal(404, exception.ErrorCode);
    }

    [Fact]
    public async Task DeleteContentRef_SoftDeletesEntity()
    {
        using var context = TestContext.Create();
        var contentRef = await TestDataFactory.SeedContentRefAsync(
            context.DbContext,
            context.SessionService.TenantId,
            ContentRefKind.Web,
            "https://www.example.com/content/delete-ref");

        var handler = new ContentRefsDeleteContentRefCommandHandler(context.DbContext);
        await handler.Handle(new ContentRefsDeleteContentRefCommand { Id = contentRef.Id }, CancellationToken.None);

        context.DbContext.SoftDeleteFiltersEnabled = false;
        var deleted = await context.DbContext.ContentRefs.FindAsync(contentRef.Id);
        Assert.NotNull(deleted);
        Assert.True(deleted!.IsDeleted);
    }

    [Fact]
    public async Task GetContentRef_ReturnsDto()
    {
        using var context = TestContext.Create();
        var contentRef = await TestDataFactory.SeedContentRefAsync(
            context.DbContext,
            context.SessionService.TenantId,
            ContentRefKind.Repository,
            "repo://faq");

        var handler = new ContentRefsGetContentRefQueryHandler(context.DbContext);
        var result = await handler.Handle(
            new ContentRefsGetContentRefQuery { Id = contentRef.Id },
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(contentRef.Id, result!.Id);
        Assert.Equal(ContentRefKind.Repository, result.Kind);
        Assert.Equal("repo://faq", result.Locator);
    }

    [Fact]
    public async Task GetContentRef_ReturnsNullWhenMissing()
    {
        using var context = TestContext.Create();
        var handler = new ContentRefsGetContentRefQueryHandler(context.DbContext);

        var result = await handler.Handle(
            new ContentRefsGetContentRefQuery { Id = Guid.NewGuid() },
            CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetContentRefList_ReturnsPagedItems()
    {
        using var context = TestContext.Create();
        await TestDataFactory.SeedContentRefAsync(context.DbContext, context.SessionService.TenantId);
        await TestDataFactory.SeedContentRefAsync(context.DbContext, context.SessionService.TenantId,
            ContentRefKind.Video, "https://www.example.com/videos/1");

        var handler = new ContentRefsGetContentRefListQueryHandler(context.DbContext);
        var request = new ContentRefsGetContentRefListQuery
        {
            Request = new ContentRefGetAllRequestDto { SkipCount = 0, MaxResultCount = 10 }
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task GetContentRefList_SortsByExplicitField()
    {
        using var context = TestContext.Create();
        await TestDataFactory.SeedContentRefAsync(context.DbContext, context.SessionService.TenantId,
            ContentRefKind.Web, "https://www.example.com/content/b-locator");
        await TestDataFactory.SeedContentRefAsync(context.DbContext, context.SessionService.TenantId,
            ContentRefKind.Web, "https://www.example.com/content/a-locator");

        var handler = new ContentRefsGetContentRefListQueryHandler(context.DbContext);
        var request = new ContentRefsGetContentRefListQuery
        {
            Request = new ContentRefGetAllRequestDto
            {
                SkipCount = 0,
                MaxResultCount = 10,
                Sorting = "locator DESC"
            }
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal("https://www.example.com/content/b-locator", result.Items[0].Locator);
        Assert.Equal("https://www.example.com/content/a-locator", result.Items[1].Locator);
    }

    [Fact]
    public async Task GetContentRefList_FallsBackToUpdatedDateWhenSortingInvalid()
    {
        using var context = TestContext.Create();
        var first = await TestDataFactory.SeedContentRefAsync(context.DbContext, context.SessionService.TenantId,
            ContentRefKind.Web, "https://www.example.com/content/z-locator");
        await TestDataFactory.SeedContentRefAsync(context.DbContext, context.SessionService.TenantId,
            ContentRefKind.Web, "https://www.example.com/content/a-locator");
        first.Label = "Updated";
        await context.DbContext.SaveChangesAsync();

        var handler = new ContentRefsGetContentRefListQueryHandler(context.DbContext);
        var request = new ContentRefsGetContentRefListQuery
        {
            Request = new ContentRefGetAllRequestDto
            {
                SkipCount = 0,
                MaxResultCount = 10,
                Sorting = "unknown DESC"
            }
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal("https://www.example.com/content/z-locator", result.Items[0].Locator);
        Assert.Equal("https://www.example.com/content/a-locator", result.Items[1].Locator);
    }

    [Fact]
    public async Task GetContentRefList_SortsByMultipleFields()
    {
        using var context = TestContext.Create();
        await TestDataFactory.SeedContentRefAsync(context.DbContext, context.SessionService.TenantId,
            ContentRefKind.Manual, "manual");
        await TestDataFactory.SeedContentRefAsync(context.DbContext, context.SessionService.TenantId,
            ContentRefKind.Web, "https://www.example.com/content/b-web");
        await TestDataFactory.SeedContentRefAsync(context.DbContext, context.SessionService.TenantId,
            ContentRefKind.Web, "https://www.example.com/content/a-web");

        var handler = new ContentRefsGetContentRefListQueryHandler(context.DbContext);
        var request = new ContentRefsGetContentRefListQuery
        {
            Request = new ContentRefGetAllRequestDto
            {
                SkipCount = 0,
                MaxResultCount = 10,
                Sorting = "kind ASC, locator DESC"
            }
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(ContentRefKind.Manual, result.Items[0].Kind);
        Assert.Equal("https://www.example.com/content/b-web", result.Items[1].Locator);
        Assert.Equal("https://www.example.com/content/a-web", result.Items[2].Locator);
    }

    [Fact]
    public async Task GetContentRefList_AppliesPaginationWindow()
    {
        using var context = TestContext.Create();
        await TestDataFactory.SeedContentRefAsync(
            context.DbContext,
            context.SessionService.TenantId,
            ContentRefKind.Web,
            "https://www.example.com/content/c-locator");
        await TestDataFactory.SeedContentRefAsync(
            context.DbContext,
            context.SessionService.TenantId,
            ContentRefKind.Web,
            "https://www.example.com/content/a-locator");
        await TestDataFactory.SeedContentRefAsync(
            context.DbContext,
            context.SessionService.TenantId,
            ContentRefKind.Web,
            "https://www.example.com/content/b-locator");

        var handler = new ContentRefsGetContentRefListQueryHandler(context.DbContext);
        var request = new ContentRefsGetContentRefListQuery
        {
            Request = new ContentRefGetAllRequestDto
            {
                SkipCount = 1,
                MaxResultCount = 1,
                Sorting = "locator ASC"
            }
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(3, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal("https://www.example.com/content/b-locator", result.Items[0].Locator);
    }

    [Fact]
    public async Task GetContentRefList_FiltersByFaqKindAndSearchText()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var matching = await TestDataFactory.SeedContentRefAsync(
            context.DbContext,
            context.SessionService.TenantId,
            ContentRefKind.Web,
            "https://www.example.com/billing");
        matching.Label = "Billing docs";
        var nonMatching = await TestDataFactory.SeedContentRefAsync(
            context.DbContext,
            context.SessionService.TenantId,
            ContentRefKind.Video,
            "https://www.example.com/support");
        nonMatching.Label = "Support video";
        await context.DbContext.SaveChangesAsync();
        await TestDataFactory.SeedFaqContentRefAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faq.Id,
            matching.Id);

        var handler = new ContentRefsGetContentRefListQueryHandler(context.DbContext);
        var request = new ContentRefsGetContentRefListQuery
        {
            Request = new ContentRefGetAllRequestDto
            {
                SkipCount = 0,
                MaxResultCount = 10,
                SearchText = "billing",
                Kind = ContentRefKind.Web,
                FaqId = faq.Id
            }
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal(matching.Id, result.Items[0].Id);
    }
}
