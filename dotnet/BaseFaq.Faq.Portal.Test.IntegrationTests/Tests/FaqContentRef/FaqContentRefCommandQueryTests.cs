using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Faq.Portal.Business.Faq.Commands.CreateFaqContentRef;
using BaseFaq.Faq.Portal.Business.Faq.Commands.DeleteFaqContentRef;
using BaseFaq.Faq.Portal.Business.Faq.Commands.UpdateFaqContentRef;
using BaseFaq.Faq.Portal.Business.Faq.Queries.GetFaqContentRef;
using BaseFaq.Faq.Portal.Business.Faq.Queries.GetFaqContentRefList;
using BaseFaq.Faq.Portal.Test.IntegrationTests.Helpers;
using BaseFaq.Models.Faq.Dtos.FaqContentRef;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BaseFaq.Faq.Portal.Test.IntegrationTests.Tests.FaqContentRef;

public class FaqContentRefCommandQueryTests
{
    [Fact]
    public async Task CreateFaqContentRef_PersistsEntityAndReturnsId()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var contentRef = await TestDataFactory.SeedContentRefAsync(context.DbContext, context.SessionService.TenantId);

        var handler = new FaqContentRefsCreateFaqContentRefCommandHandler(
            context.DbContext,
            context.SessionService);
        var request = new FaqContentRefsCreateFaqContentRefCommand
        {
            FaqId = faq.Id,
            ContentRefId = contentRef.Id
        };

        var id = await handler.Handle(request, CancellationToken.None);

        var faqContentRef = await context.DbContext.FaqContentRefs.FindAsync(id);
        Assert.NotNull(faqContentRef);
        Assert.Equal(faq.Id, faqContentRef!.FaqId);
        Assert.Equal(contentRef.Id, faqContentRef.ContentRefId);
        Assert.Equal(context.SessionService.TenantId, faqContentRef.TenantId);
    }

    [Fact]
    public async Task CreateFaqContentRef_ThrowsWhenContentRefMissing()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);

        var handler = new FaqContentRefsCreateFaqContentRefCommandHandler(
            context.DbContext,
            context.SessionService);
        var request = new FaqContentRefsCreateFaqContentRefCommand
        {
            FaqId = faq.Id,
            ContentRefId = Guid.NewGuid()
        };

        var exception =
            await Assert.ThrowsAsync<ApiErrorException>(() => handler.Handle(request, CancellationToken.None));

        Assert.Equal(404, exception.ErrorCode);
    }

    [Fact]
    public async Task CreateFaqContentRef_ThrowsWhenFaqMissing()
    {
        using var context = TestContext.Create();
        var contentRef = await TestDataFactory.SeedContentRefAsync(context.DbContext, context.SessionService.TenantId);

        var handler = new FaqContentRefsCreateFaqContentRefCommandHandler(
            context.DbContext,
            context.SessionService);
        var request = new FaqContentRefsCreateFaqContentRefCommand
        {
            FaqId = Guid.NewGuid(),
            ContentRefId = contentRef.Id
        };

        var exception =
            await Assert.ThrowsAsync<ApiErrorException>(() => handler.Handle(request, CancellationToken.None));

        Assert.Equal(404, exception.ErrorCode);
    }

    [Fact]
    public async Task UpdateFaqContentRef_UpdatesExistingFaqContentRef()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var otherFaq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId, "Other");
        var contentRef = await TestDataFactory.SeedContentRefAsync(context.DbContext, context.SessionService.TenantId);
        var otherContentRef = await TestDataFactory.SeedContentRefAsync(
            context.DbContext,
            context.SessionService.TenantId,
            locator: "https://www.example.com/content/other");
        var faqContentRef = await TestDataFactory.SeedFaqContentRefAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faq.Id,
            contentRef.Id);

        var handler = new FaqContentRefsUpdateFaqContentRefCommandHandler(context.DbContext);
        var request = new FaqContentRefsUpdateFaqContentRefCommand
        {
            Id = faqContentRef.Id,
            FaqId = otherFaq.Id,
            ContentRefId = otherContentRef.Id
        };

        await handler.Handle(request, CancellationToken.None);

        var updated = await context.DbContext.FaqContentRefs.FindAsync(faqContentRef.Id);
        Assert.NotNull(updated);
        Assert.Equal(otherFaq.Id, updated!.FaqId);
        Assert.Equal(otherContentRef.Id, updated.ContentRefId);
    }

    [Fact]
    public async Task UpdateFaqContentRef_ThrowsWhenMissing()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var contentRef = await TestDataFactory.SeedContentRefAsync(context.DbContext, context.SessionService.TenantId);

        var handler = new FaqContentRefsUpdateFaqContentRefCommandHandler(context.DbContext);
        var request = new FaqContentRefsUpdateFaqContentRefCommand
        {
            Id = Guid.NewGuid(),
            FaqId = faq.Id,
            ContentRefId = contentRef.Id
        };

        var exception =
            await Assert.ThrowsAsync<ApiErrorException>(() => handler.Handle(request, CancellationToken.None));

        Assert.Equal(404, exception.ErrorCode);
    }

    [Fact]
    public async Task UpdateFaqContentRef_ThrowsWhenFaqMissing()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var contentRef = await TestDataFactory.SeedContentRefAsync(context.DbContext, context.SessionService.TenantId);
        var faqContentRef = await TestDataFactory.SeedFaqContentRefAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faq.Id,
            contentRef.Id);

        var handler = new FaqContentRefsUpdateFaqContentRefCommandHandler(context.DbContext);
        var request = new FaqContentRefsUpdateFaqContentRefCommand
        {
            Id = faqContentRef.Id,
            FaqId = Guid.NewGuid(),
            ContentRefId = contentRef.Id
        };

        var exception =
            await Assert.ThrowsAsync<ApiErrorException>(() => handler.Handle(request, CancellationToken.None));

        Assert.Equal(404, exception.ErrorCode);
    }

    [Fact]
    public async Task UpdateFaqContentRef_ThrowsWhenContentRefMissing()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var contentRef = await TestDataFactory.SeedContentRefAsync(context.DbContext, context.SessionService.TenantId);
        var faqContentRef = await TestDataFactory.SeedFaqContentRefAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faq.Id,
            contentRef.Id);

        var handler = new FaqContentRefsUpdateFaqContentRefCommandHandler(context.DbContext);
        var request = new FaqContentRefsUpdateFaqContentRefCommand
        {
            Id = faqContentRef.Id,
            FaqId = faq.Id,
            ContentRefId = Guid.NewGuid()
        };

        var exception =
            await Assert.ThrowsAsync<ApiErrorException>(() => handler.Handle(request, CancellationToken.None));

        Assert.Equal(404, exception.ErrorCode);
    }

    [Fact]
    public async Task DeleteFaqContentRef_SoftDeletesEntity()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var contentRef = await TestDataFactory.SeedContentRefAsync(context.DbContext, context.SessionService.TenantId);
        var faqContentRef = await TestDataFactory.SeedFaqContentRefAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faq.Id,
            contentRef.Id);

        var handler = new FaqContentRefsDeleteFaqContentRefCommandHandler(context.DbContext);
        await handler.Handle(
            new FaqContentRefsDeleteFaqContentRefCommand { Id = faqContentRef.Id },
            CancellationToken.None);

        context.DbContext.SoftDeleteFiltersEnabled = false;
        var deleted = await context.DbContext.FaqContentRefs.FindAsync(faqContentRef.Id);
        Assert.NotNull(deleted);
        Assert.True(deleted!.IsDeleted);
    }

    [Fact]
    public async Task GetFaqContentRef_ReturnsDto()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var contentRef = await TestDataFactory.SeedContentRefAsync(context.DbContext, context.SessionService.TenantId);
        var faqContentRef = await TestDataFactory.SeedFaqContentRefAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faq.Id,
            contentRef.Id);

        var handler = new FaqContentRefsGetFaqContentRefQueryHandler(context.DbContext);
        var result = await handler.Handle(
            new FaqContentRefsGetFaqContentRefQuery { Id = faqContentRef.Id },
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(faqContentRef.Id, result!.Id);
        Assert.Equal(faq.Id, result.FaqId);
        Assert.Equal(contentRef.Id, result.ContentRefId);
    }

    [Fact]
    public async Task GetFaqContentRef_ReturnsNullWhenMissing()
    {
        using var context = TestContext.Create();
        var handler = new FaqContentRefsGetFaqContentRefQueryHandler(context.DbContext);

        var result = await handler.Handle(
            new FaqContentRefsGetFaqContentRefQuery { Id = Guid.NewGuid() },
            CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetFaqContentRefList_ReturnsPagedItems()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var contentRef1 =
            await TestDataFactory.SeedContentRefAsync(context.DbContext, context.SessionService.TenantId,
                locator: "https://www.example.com/content/one");
        var contentRef2 =
            await TestDataFactory.SeedContentRefAsync(context.DbContext, context.SessionService.TenantId,
                locator: "https://www.example.com/content/two");
        await TestDataFactory.SeedFaqContentRefAsync(context.DbContext, context.SessionService.TenantId, faq.Id,
            contentRef1.Id);
        await TestDataFactory.SeedFaqContentRefAsync(context.DbContext, context.SessionService.TenantId, faq.Id,
            contentRef2.Id);

        var handler = new FaqContentRefsGetFaqContentRefListQueryHandler(context.DbContext);
        var request = new FaqContentRefsGetFaqContentRefListQuery
        {
            Request = new FaqContentRefGetAllRequestDto { SkipCount = 0, MaxResultCount = 10 }
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task CreateFaqContentRef_ThrowsWhenDuplicatePair()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var contentRef = await TestDataFactory.SeedContentRefAsync(context.DbContext, context.SessionService.TenantId);

        var handler = new FaqContentRefsCreateFaqContentRefCommandHandler(
            context.DbContext,
            context.SessionService);
        var request = new FaqContentRefsCreateFaqContentRefCommand
        {
            FaqId = faq.Id,
            ContentRefId = contentRef.Id
        };

        await handler.Handle(request, CancellationToken.None);

        await Assert.ThrowsAsync<DbUpdateException>(() => handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task GetFaqContentRefList_SortsByMultipleFields()
    {
        using var context = TestContext.Create();
        var tenantId = context.SessionService.TenantId;

        var faqA = new Common.Persistence.FaqDb.Entities.Faq
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000110"),
            Name = "FAQ A",
            Language = "en-US",
            Status = BaseFaq.Models.Faq.Enums.FaqStatus.Draft,
            TenantId = tenantId
        };
        var faqB = new Common.Persistence.FaqDb.Entities.Faq
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000120"),
            Name = "FAQ B",
            Language = "en-US",
            Status = BaseFaq.Models.Faq.Enums.FaqStatus.Draft,
            TenantId = tenantId
        };
        var contentRefA = new Common.Persistence.FaqDb.Entities.ContentRef
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000130"),
            Kind = BaseFaq.Models.Faq.Enums.ContentRefKind.Web,
            Locator = "https://www.example.com/content/ref-a",
            Label = "A",
            Scope = "Scope",
            TenantId = tenantId
        };
        var contentRefB = new Common.Persistence.FaqDb.Entities.ContentRef
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000140"),
            Kind = BaseFaq.Models.Faq.Enums.ContentRefKind.Web,
            Locator = "https://www.example.com/content/ref-b",
            Label = "B",
            Scope = "Scope",
            TenantId = tenantId
        };

        context.DbContext.Faqs.AddRange(faqA, faqB);
        context.DbContext.ContentRefs.AddRange(contentRefA, contentRefB);
        await context.DbContext.SaveChangesAsync();

        var ref1 = new Common.Persistence.FaqDb.Entities.FaqContentRef
        {
            FaqId = faqA.Id,
            ContentRefId = contentRefA.Id,
            TenantId = tenantId
        };
        var ref2 = new Common.Persistence.FaqDb.Entities.FaqContentRef
        {
            FaqId = faqA.Id,
            ContentRefId = contentRefB.Id,
            TenantId = tenantId
        };
        var ref3 = new Common.Persistence.FaqDb.Entities.FaqContentRef
        {
            FaqId = faqB.Id,
            ContentRefId = contentRefA.Id,
            TenantId = tenantId
        };

        context.DbContext.FaqContentRefs.AddRange(ref1, ref2, ref3);
        await context.DbContext.SaveChangesAsync();

        var handler = new FaqContentRefsGetFaqContentRefListQueryHandler(context.DbContext);
        var request = new FaqContentRefsGetFaqContentRefListQuery
        {
            Request = new FaqContentRefGetAllRequestDto
            {
                SkipCount = 0,
                MaxResultCount = 10,
                Sorting = "faqid ASC, contentrefid DESC"
            }
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(ref2.Id, result.Items[0].Id);
        Assert.Equal(ref1.Id, result.Items[1].Id);
        Assert.Equal(ref3.Id, result.Items[2].Id);
    }

    [Fact]
    public async Task GetFaqContentRefList_FallsBackToUpdatedDateWhenSortingInvalid()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var otherFaq =
            await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId, "Other FAQ");
        var contentRefA = await TestDataFactory.SeedContentRefAsync(
            context.DbContext,
            context.SessionService.TenantId,
            locator: "https://www.example.com/content/ref-a");
        var contentRefB = await TestDataFactory.SeedContentRefAsync(
            context.DbContext,
            context.SessionService.TenantId,
            locator: "https://www.example.com/content/ref-b");

        var first = await TestDataFactory.SeedFaqContentRefAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faq.Id,
            contentRefA.Id);
        await TestDataFactory.SeedFaqContentRefAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faq.Id,
            contentRefB.Id);

        first.FaqId = otherFaq.Id;
        await context.DbContext.SaveChangesAsync();

        var handler = new FaqContentRefsGetFaqContentRefListQueryHandler(context.DbContext);
        var request = new FaqContentRefsGetFaqContentRefListQuery
        {
            Request = new FaqContentRefGetAllRequestDto
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

    [Fact]
    public async Task GetFaqContentRefList_AppliesPaginationWindow()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var contentRefA = await TestDataFactory.SeedContentRefAsync(
            context.DbContext,
            context.SessionService.TenantId,
            locator: "https://www.example.com/content/ref-a");
        var contentRefB = await TestDataFactory.SeedContentRefAsync(
            context.DbContext,
            context.SessionService.TenantId,
            locator: "https://www.example.com/content/ref-b");
        var contentRefC = await TestDataFactory.SeedContentRefAsync(
            context.DbContext,
            context.SessionService.TenantId,
            locator: "https://www.example.com/content/ref-c");

        await TestDataFactory.SeedFaqContentRefAsync(context.DbContext, context.SessionService.TenantId, faq.Id,
            contentRefA.Id);
        await TestDataFactory.SeedFaqContentRefAsync(context.DbContext, context.SessionService.TenantId, faq.Id,
            contentRefB.Id);
        await TestDataFactory.SeedFaqContentRefAsync(context.DbContext, context.SessionService.TenantId, faq.Id,
            contentRefC.Id);

        var handler = new FaqContentRefsGetFaqContentRefListQueryHandler(context.DbContext);
        var request = new FaqContentRefsGetFaqContentRefListQuery
        {
            Request = new FaqContentRefGetAllRequestDto
            {
                SkipCount = 1,
                MaxResultCount = 1,
                Sorting = "contentrefid ASC"
            }
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(3, result.TotalCount);
        Assert.Single(result.Items);
    }
}
