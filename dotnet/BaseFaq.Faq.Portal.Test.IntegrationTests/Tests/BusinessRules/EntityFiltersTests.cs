using BaseFaq.Faq.Portal.Business.ContentRef.Queries.GetContentRef;
using BaseFaq.Faq.Portal.Business.FaqItem.Queries.GetFaqItem;
using BaseFaq.Faq.Portal.Business.Tag.Queries.GetTag;
using BaseFaq.Faq.Portal.Business.Feedback.Queries.GetFeedback;
using BaseFaq.Faq.Portal.Test.IntegrationTests.Helpers;
using BaseFaq.Faq.Portal.Test.IntegrationTests.Helpers.Infrastructure;
using Xunit;

namespace BaseFaq.Faq.Portal.Test.IntegrationTests.Tests.BusinessRules;

public class EntityFiltersTests
{
    [Fact]
    public async Task TenantFilters_HideOtherTenantTags()
    {
        using var database = TestDatabase.Create();
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        using var contextA = TestContext.CreateForDatabase(
            database.ConnectionString,
            database.AdminConnectionString,
            database.DatabaseName,
            tenantId: tenantA);
        var tag = await TestDataFactory.SeedTagAsync(contextA.DbContext, tenantA, "Tenant A Tag");

        using var contextB = TestContext.CreateForDatabase(
            database.ConnectionString,
            database.AdminConnectionString,
            database.DatabaseName,
            tenantId: tenantB);
        var handler = new TagsGetTagQueryHandler(contextB.DbContext);

        var result = await handler.Handle(new TagsGetTagQuery { Id = tag.Id }, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task TenantFilters_HideOtherTenantContentRefs()
    {
        using var database = TestDatabase.Create();
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        using var contextA = TestContext.CreateForDatabase(
            database.ConnectionString,
            database.AdminConnectionString,
            database.DatabaseName,
            tenantId: tenantA);
        var contentRef = await TestDataFactory.SeedContentRefAsync(contextA.DbContext, tenantA);

        using var contextB = TestContext.CreateForDatabase(
            database.ConnectionString,
            database.AdminConnectionString,
            database.DatabaseName,
            tenantId: tenantB);
        var handler = new ContentRefsGetContentRefQueryHandler(contextB.DbContext);

        var result = await handler.Handle(
            new ContentRefsGetContentRefQuery { Id = contentRef.Id },
            CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task TenantFilters_HideOtherTenantFaqItems()
    {
        using var database = TestDatabase.Create();
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        using var contextA = TestContext.CreateForDatabase(
            database.ConnectionString,
            database.AdminConnectionString,
            database.DatabaseName,
            tenantId: tenantA);
        var faq = await TestDataFactory.SeedFaqAsync(contextA.DbContext, tenantA);
        var faqItem = await TestDataFactory.SeedFaqItemAsync(contextA.DbContext, tenantA, faq.Id);

        using var contextB = TestContext.CreateForDatabase(
            database.ConnectionString,
            database.AdminConnectionString,
            database.DatabaseName,
            tenantId: tenantB);
        var handler = new FaqItemsGetFaqItemQueryHandler(contextB.DbContext);

        var result = await handler.Handle(new FaqItemsGetFaqItemQuery { Id = faqItem.Id }, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task TenantFilters_HideOtherTenantFeedbacks()
    {
        using var database = TestDatabase.Create();
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        using var contextA = TestContext.CreateForDatabase(
            database.ConnectionString,
            database.AdminConnectionString,
            database.DatabaseName,
            tenantId: tenantA);
        var faq = await TestDataFactory.SeedFaqAsync(contextA.DbContext, tenantA);
        var faqItem = await TestDataFactory.SeedFaqItemAsync(contextA.DbContext, tenantA, faq.Id);
        var feedback = await TestDataFactory.SeedFeedbackAsync(contextA.DbContext, tenantA, faqItem.Id);

        using var contextB = TestContext.CreateForDatabase(
            database.ConnectionString,
            database.AdminConnectionString,
            database.DatabaseName,
            tenantId: tenantB);
        var handler = new FeedbacksGetFeedbackQueryHandler(contextB.DbContext);

        var result = await handler.Handle(new FeedbacksGetFeedbackQuery { Id = feedback.Id }, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task TenantFilters_CanBeSkippedForTags()
    {
        using var database = TestDatabase.Create();
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        using var contextA = TestContext.CreateForDatabase(
            database.ConnectionString,
            database.AdminConnectionString,
            database.DatabaseName,
            tenantId: tenantA);
        var tag = await TestDataFactory.SeedTagAsync(contextA.DbContext, tenantA, "Tenant A Tag");

        var httpContext = TestHttpContextFactory.CreateWithTenantValidationSkipped();
        using var contextB = TestContext.CreateForDatabase(
            database.ConnectionString,
            database.AdminConnectionString,
            database.DatabaseName,
            tenantId: tenantB,
            httpContext: httpContext);
        var handler = new TagsGetTagQueryHandler(contextB.DbContext);

        var result = await handler.Handle(new TagsGetTagQuery { Id = tag.Id }, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(tag.Id, result!.Id);
    }

    [Fact]
    public async Task TenantFilters_CanBeSkippedForContentRefs()
    {
        using var database = TestDatabase.Create();
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        using var contextA = TestContext.CreateForDatabase(
            database.ConnectionString,
            database.AdminConnectionString,
            database.DatabaseName,
            tenantId: tenantA);
        var contentRef = await TestDataFactory.SeedContentRefAsync(contextA.DbContext, tenantA);

        var httpContext = TestHttpContextFactory.CreateWithTenantValidationSkipped();
        using var contextB = TestContext.CreateForDatabase(
            database.ConnectionString,
            database.AdminConnectionString,
            database.DatabaseName,
            tenantId: tenantB,
            httpContext: httpContext);
        var handler = new ContentRefsGetContentRefQueryHandler(contextB.DbContext);

        var result = await handler.Handle(
            new ContentRefsGetContentRefQuery { Id = contentRef.Id },
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(contentRef.Id, result!.Id);
    }

    [Fact]
    public async Task TenantFilters_CanBeSkippedForFaqItems()
    {
        using var database = TestDatabase.Create();
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        using var contextA = TestContext.CreateForDatabase(
            database.ConnectionString,
            database.AdminConnectionString,
            database.DatabaseName,
            tenantId: tenantA);
        var faq = await TestDataFactory.SeedFaqAsync(contextA.DbContext, tenantA);
        var faqItem = await TestDataFactory.SeedFaqItemAsync(contextA.DbContext, tenantA, faq.Id);

        var httpContext = TestHttpContextFactory.CreateWithTenantValidationSkipped();
        using var contextB = TestContext.CreateForDatabase(
            database.ConnectionString,
            database.AdminConnectionString,
            database.DatabaseName,
            tenantId: tenantB,
            httpContext: httpContext);
        var handler = new FaqItemsGetFaqItemQueryHandler(contextB.DbContext);

        var result = await handler.Handle(new FaqItemsGetFaqItemQuery { Id = faqItem.Id }, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(faqItem.Id, result!.Id);
    }

    [Fact]
    public async Task TenantFilters_CanBeSkippedForFeedbacks()
    {
        using var database = TestDatabase.Create();
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        using var contextA = TestContext.CreateForDatabase(
            database.ConnectionString,
            database.AdminConnectionString,
            database.DatabaseName,
            tenantId: tenantA);
        var faq = await TestDataFactory.SeedFaqAsync(contextA.DbContext, tenantA);
        var faqItem = await TestDataFactory.SeedFaqItemAsync(contextA.DbContext, tenantA, faq.Id);
        var feedback = await TestDataFactory.SeedFeedbackAsync(contextA.DbContext, tenantA, faqItem.Id);

        var httpContext = TestHttpContextFactory.CreateWithTenantValidationSkipped();
        using var contextB = TestContext.CreateForDatabase(
            database.ConnectionString,
            database.AdminConnectionString,
            database.DatabaseName,
            tenantId: tenantB,
            httpContext: httpContext);
        var handler = new FeedbacksGetFeedbackQueryHandler(contextB.DbContext);

        var result = await handler.Handle(new FeedbacksGetFeedbackQuery { Id = feedback.Id }, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(feedback.Id, result!.Id);
    }

    [Fact]
    public async Task SoftDelete_HidesTagRecordsByDefault()
    {
        using var context = TestContext.Create();
        var tag = await TestDataFactory.SeedTagAsync(context.DbContext, context.SessionService.TenantId, "soft-delete");

        context.DbContext.Tags.Remove(tag);
        await context.DbContext.SaveChangesAsync();

        var handler = new TagsGetTagQueryHandler(context.DbContext);
        var result = await handler.Handle(new TagsGetTagQuery { Id = tag.Id }, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task SoftDelete_CanBeBypassedForFaqItemsWhenDisabled()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var faqItem = await TestDataFactory.SeedFaqItemAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faq.Id);

        context.DbContext.FaqItems.Remove(faqItem);
        await context.DbContext.SaveChangesAsync();

        context.DbContext.SoftDeleteFiltersEnabled = false;

        var handler = new FaqItemsGetFaqItemQueryHandler(context.DbContext);
        var result = await handler.Handle(new FaqItemsGetFaqItemQuery { Id = faqItem.Id }, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(faqItem.Id, result!.Id);
    }
}