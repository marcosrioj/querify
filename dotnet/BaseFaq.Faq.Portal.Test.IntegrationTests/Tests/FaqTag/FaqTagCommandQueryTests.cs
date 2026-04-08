using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Faq.Portal.Business.Faq.Commands.CreateFaqTag;
using BaseFaq.Faq.Portal.Business.Faq.Commands.DeleteFaqTag;
using BaseFaq.Faq.Portal.Business.Faq.Commands.UpdateFaqTag;
using BaseFaq.Faq.Portal.Business.Faq.Queries.GetFaqTag;
using BaseFaq.Faq.Portal.Business.Faq.Queries.GetFaqTagList;
using BaseFaq.Faq.Portal.Test.IntegrationTests.Helpers;
using BaseFaq.Models.Faq.Dtos.FaqTag;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BaseFaq.Faq.Portal.Test.IntegrationTests.Tests.FaqTag;

public class FaqTagCommandQueryTests
{
    [Fact]
    public async Task CreateFaqTag_PersistsEntityAndReturnsId()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var tag = await TestDataFactory.SeedTagAsync(context.DbContext, context.SessionService.TenantId);

        var handler = new FaqTagsCreateFaqTagCommandHandler(context.DbContext, context.SessionService);
        var request = new FaqTagsCreateFaqTagCommand { FaqId = faq.Id, TagId = tag.Id };

        var id = await handler.Handle(request, CancellationToken.None);

        var faqTag = await context.DbContext.FaqTags.FindAsync(id);
        Assert.NotNull(faqTag);
        Assert.Equal(faq.Id, faqTag!.FaqId);
        Assert.Equal(tag.Id, faqTag.TagId);
        Assert.Equal(context.SessionService.TenantId, faqTag.TenantId);
    }

    [Fact]
    public async Task CreateFaqTag_ThrowsWhenFaqMissing()
    {
        using var context = TestContext.Create();
        var tag = await TestDataFactory.SeedTagAsync(context.DbContext, context.SessionService.TenantId);

        var handler = new FaqTagsCreateFaqTagCommandHandler(context.DbContext, context.SessionService);
        var request = new FaqTagsCreateFaqTagCommand { FaqId = Guid.NewGuid(), TagId = tag.Id };

        var exception =
            await Assert.ThrowsAsync<ApiErrorException>(() => handler.Handle(request, CancellationToken.None));

        Assert.Equal(404, exception.ErrorCode);
    }

    [Fact]
    public async Task CreateFaqTag_ThrowsWhenTagMissing()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);

        var handler = new FaqTagsCreateFaqTagCommandHandler(context.DbContext, context.SessionService);
        var request = new FaqTagsCreateFaqTagCommand { FaqId = faq.Id, TagId = Guid.NewGuid() };

        var exception =
            await Assert.ThrowsAsync<ApiErrorException>(() => handler.Handle(request, CancellationToken.None));

        Assert.Equal(404, exception.ErrorCode);
    }

    [Fact]
    public async Task UpdateFaqTag_UpdatesExistingFaqTag()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var tag = await TestDataFactory.SeedTagAsync(context.DbContext, context.SessionService.TenantId, "first");
        var otherTag = await TestDataFactory.SeedTagAsync(context.DbContext, context.SessionService.TenantId, "second");
        var faqTag = await TestDataFactory.SeedFaqTagAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faq.Id,
            tag.Id);

        var handler = new FaqTagsUpdateFaqTagCommandHandler(context.DbContext);
        var request = new FaqTagsUpdateFaqTagCommand
        {
            Id = faqTag.Id,
            FaqId = faq.Id,
            TagId = otherTag.Id
        };

        await handler.Handle(request, CancellationToken.None);

        var updated = await context.DbContext.FaqTags.FindAsync(faqTag.Id);
        Assert.NotNull(updated);
        Assert.Equal(otherTag.Id, updated!.TagId);
    }

    [Fact]
    public async Task UpdateFaqTag_ThrowsWhenMissing()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var tag = await TestDataFactory.SeedTagAsync(context.DbContext, context.SessionService.TenantId);

        var handler = new FaqTagsUpdateFaqTagCommandHandler(context.DbContext);
        var request = new FaqTagsUpdateFaqTagCommand
        {
            Id = Guid.NewGuid(),
            FaqId = faq.Id,
            TagId = tag.Id
        };

        var exception =
            await Assert.ThrowsAsync<ApiErrorException>(() => handler.Handle(request, CancellationToken.None));

        Assert.Equal(404, exception.ErrorCode);
    }

    [Fact]
    public async Task UpdateFaqTag_ThrowsWhenFaqMissing()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var tag = await TestDataFactory.SeedTagAsync(context.DbContext, context.SessionService.TenantId);
        var faqTag = await TestDataFactory.SeedFaqTagAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faq.Id,
            tag.Id);

        var handler = new FaqTagsUpdateFaqTagCommandHandler(context.DbContext);
        var request = new FaqTagsUpdateFaqTagCommand
        {
            Id = faqTag.Id,
            FaqId = Guid.NewGuid(),
            TagId = tag.Id
        };

        var exception =
            await Assert.ThrowsAsync<ApiErrorException>(() => handler.Handle(request, CancellationToken.None));

        Assert.Equal(404, exception.ErrorCode);
    }

    [Fact]
    public async Task UpdateFaqTag_ThrowsWhenTagMissing()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var tag = await TestDataFactory.SeedTagAsync(context.DbContext, context.SessionService.TenantId);
        var faqTag = await TestDataFactory.SeedFaqTagAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faq.Id,
            tag.Id);

        var handler = new FaqTagsUpdateFaqTagCommandHandler(context.DbContext);
        var request = new FaqTagsUpdateFaqTagCommand
        {
            Id = faqTag.Id,
            FaqId = faq.Id,
            TagId = Guid.NewGuid()
        };

        var exception =
            await Assert.ThrowsAsync<ApiErrorException>(() => handler.Handle(request, CancellationToken.None));

        Assert.Equal(404, exception.ErrorCode);
    }

    [Fact]
    public async Task DeleteFaqTag_SoftDeletesEntity()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var tag = await TestDataFactory.SeedTagAsync(context.DbContext, context.SessionService.TenantId);
        var faqTag = await TestDataFactory.SeedFaqTagAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faq.Id,
            tag.Id);

        var handler = new FaqTagsDeleteFaqTagCommandHandler(context.DbContext);
        await handler.Handle(new FaqTagsDeleteFaqTagCommand { Id = faqTag.Id }, CancellationToken.None);

        context.DbContext.SoftDeleteFiltersEnabled = false;
        var deleted = await context.DbContext.FaqTags.FindAsync(faqTag.Id);
        Assert.NotNull(deleted);
        Assert.True(deleted!.IsDeleted);
    }

    [Fact]
    public async Task GetFaqTag_ReturnsDto()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var tag = await TestDataFactory.SeedTagAsync(context.DbContext, context.SessionService.TenantId);
        var faqTag = await TestDataFactory.SeedFaqTagAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faq.Id,
            tag.Id);

        var handler = new FaqTagsGetFaqTagQueryHandler(context.DbContext);
        var result = await handler.Handle(new FaqTagsGetFaqTagQuery { Id = faqTag.Id }, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(faqTag.Id, result!.Id);
        Assert.Equal(faq.Id, result.FaqId);
        Assert.Equal(tag.Id, result.TagId);
    }

    [Fact]
    public async Task GetFaqTag_ReturnsNullWhenMissing()
    {
        using var context = TestContext.Create();
        var handler = new FaqTagsGetFaqTagQueryHandler(context.DbContext);

        var result = await handler.Handle(new FaqTagsGetFaqTagQuery { Id = Guid.NewGuid() }, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetFaqTagList_ReturnsPagedItems()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var tag1 = await TestDataFactory.SeedTagAsync(context.DbContext, context.SessionService.TenantId, "one");
        var tag2 = await TestDataFactory.SeedTagAsync(context.DbContext, context.SessionService.TenantId, "two");
        await TestDataFactory.SeedFaqTagAsync(context.DbContext, context.SessionService.TenantId, faq.Id, tag1.Id);
        await TestDataFactory.SeedFaqTagAsync(context.DbContext, context.SessionService.TenantId, faq.Id, tag2.Id);

        var handler = new FaqTagsGetFaqTagListQueryHandler(context.DbContext);
        var request = new FaqTagsGetFaqTagListQuery
        {
            Request = new FaqTagGetAllRequestDto { SkipCount = 0, MaxResultCount = 10 }
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task CreateFaqTag_ThrowsWhenDuplicatePair()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var tag = await TestDataFactory.SeedTagAsync(context.DbContext, context.SessionService.TenantId);

        var handler = new FaqTagsCreateFaqTagCommandHandler(context.DbContext, context.SessionService);
        var request = new FaqTagsCreateFaqTagCommand { FaqId = faq.Id, TagId = tag.Id };

        await handler.Handle(request, CancellationToken.None);

        await Assert.ThrowsAsync<DbUpdateException>(() => handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task GetFaqTagList_SortsByMultipleFields()
    {
        using var context = TestContext.Create();
        var tenantId = context.SessionService.TenantId;

        var faqA = new Common.Persistence.FaqDb.Entities.Faq
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000010"),
            Name = "FAQ A",
            Language = "en-US",
            Status = BaseFaq.Models.Faq.Enums.FaqStatus.Draft,
            TenantId = tenantId
        };
        var faqB = new Common.Persistence.FaqDb.Entities.Faq
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000020"),
            Name = "FAQ B",
            Language = "en-US",
            Status = BaseFaq.Models.Faq.Enums.FaqStatus.Draft,
            TenantId = tenantId
        };
        var tagA = new Common.Persistence.FaqDb.Entities.Tag
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000030"),
            Value = "tag-a",
            TenantId = tenantId
        };
        var tagB = new Common.Persistence.FaqDb.Entities.Tag
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000040"),
            Value = "tag-b",
            TenantId = tenantId
        };

        context.DbContext.Faqs.AddRange(faqA, faqB);
        context.DbContext.Tags.AddRange(tagA, tagB);
        await context.DbContext.SaveChangesAsync();

        var faqTag1 = new Common.Persistence.FaqDb.Entities.FaqTag
        {
            FaqId = faqA.Id,
            TagId = tagA.Id,
            TenantId = tenantId
        };
        var faqTag2 = new Common.Persistence.FaqDb.Entities.FaqTag
        {
            FaqId = faqA.Id,
            TagId = tagB.Id,
            TenantId = tenantId
        };
        var faqTag3 = new Common.Persistence.FaqDb.Entities.FaqTag
        {
            FaqId = faqB.Id,
            TagId = tagA.Id,
            TenantId = tenantId
        };

        context.DbContext.FaqTags.AddRange(faqTag1, faqTag2, faqTag3);
        await context.DbContext.SaveChangesAsync();

        var handler = new FaqTagsGetFaqTagListQueryHandler(context.DbContext);
        var request = new FaqTagsGetFaqTagListQuery
        {
            Request = new FaqTagGetAllRequestDto
            {
                SkipCount = 0,
                MaxResultCount = 10,
                Sorting = "faqid ASC, tagid DESC"
            }
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(faqTag2.Id, result.Items[0].Id);
        Assert.Equal(faqTag1.Id, result.Items[1].Id);
        Assert.Equal(faqTag3.Id, result.Items[2].Id);
    }

    [Fact]
    public async Task GetFaqTagList_FallsBackToUpdatedDateWhenSortingInvalid()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var otherFaq =
            await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId, "Other FAQ");
        var tagA = await TestDataFactory.SeedTagAsync(context.DbContext, context.SessionService.TenantId, "tag-a");
        var tagB = await TestDataFactory.SeedTagAsync(context.DbContext, context.SessionService.TenantId, "tag-b");
        var first = await TestDataFactory.SeedFaqTagAsync(context.DbContext, context.SessionService.TenantId, faq.Id,
            tagA.Id);
        await TestDataFactory.SeedFaqTagAsync(context.DbContext, context.SessionService.TenantId, faq.Id, tagB.Id);

        first.FaqId = otherFaq.Id;
        await context.DbContext.SaveChangesAsync();

        var handler = new FaqTagsGetFaqTagListQueryHandler(context.DbContext);
        var request = new FaqTagsGetFaqTagListQuery
        {
            Request = new FaqTagGetAllRequestDto
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
    public async Task GetFaqTagList_AppliesPaginationWindow()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var tagA = await TestDataFactory.SeedTagAsync(context.DbContext, context.SessionService.TenantId, "tag-a");
        var tagB = await TestDataFactory.SeedTagAsync(context.DbContext, context.SessionService.TenantId, "tag-b");
        var tagC = await TestDataFactory.SeedTagAsync(context.DbContext, context.SessionService.TenantId, "tag-c");

        await TestDataFactory.SeedFaqTagAsync(context.DbContext, context.SessionService.TenantId, faq.Id, tagA.Id);
        await TestDataFactory.SeedFaqTagAsync(context.DbContext, context.SessionService.TenantId, faq.Id, tagB.Id);
        await TestDataFactory.SeedFaqTagAsync(context.DbContext, context.SessionService.TenantId, faq.Id, tagC.Id);

        var handler = new FaqTagsGetFaqTagListQueryHandler(context.DbContext);
        var request = new FaqTagsGetFaqTagListQuery
        {
            Request = new FaqTagGetAllRequestDto
            {
                SkipCount = 1,
                MaxResultCount = 1,
                Sorting = "tagid ASC"
            }
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(3, result.TotalCount);
        Assert.Single(result.Items);
    }
}