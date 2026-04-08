using BaseFaq.Faq.Portal.Business.Faq.Queries.GetFaq;
using BaseFaq.Faq.Portal.Business.Faq.Queries.GetFaqList;
using BaseFaq.Faq.Portal.Test.IntegrationTests.Helpers;
using BaseFaq.Faq.Portal.Test.IntegrationTests.Helpers.Infrastructure;
using BaseFaq.Models.Faq.Dtos.Faq;
using BaseFaq.Models.Faq.Enums;
using Xunit;

namespace BaseFaq.Faq.Portal.Test.IntegrationTests.Tests.Faq;

public class FaqBusinessRulesTests
{
    [Fact]
    public async Task TenantFilters_HideOtherTenantData()
    {
        using var database = TestDatabase.Create();
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        using var contextA = TestContext.CreateForDatabase(
            database.ConnectionString,
            database.AdminConnectionString,
            database.DatabaseName,
            tenantId: tenantA);
        var faq = await TestDataFactory.SeedFaqAsync(contextA.DbContext, tenantA, "Tenant A FAQ");

        using var contextB = TestContext.CreateForDatabase(
            database.ConnectionString,
            database.AdminConnectionString,
            database.DatabaseName,
            tenantId: tenantB);
        var handler = new FaqsGetFaqQueryHandler(contextB.DbContext);

        var result = await handler.Handle(new FaqsGetFaqQuery { Id = faq.Id }, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task TenantFilters_CanBeSkippedWithEndpointMetadata()
    {
        using var database = TestDatabase.Create();
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        using var contextA = TestContext.CreateForDatabase(
            database.ConnectionString,
            database.AdminConnectionString,
            database.DatabaseName,
            tenantId: tenantA);
        var faq = await TestDataFactory.SeedFaqAsync(contextA.DbContext, tenantA, "Tenant A FAQ");

        var httpContext = TestHttpContextFactory.CreateWithTenantValidationSkipped();
        using var contextB = TestContext.CreateForDatabase(
            database.ConnectionString,
            database.AdminConnectionString,
            database.DatabaseName,
            tenantId: tenantB,
            httpContext: httpContext);

        var handler = new FaqsGetFaqQueryHandler(contextB.DbContext);
        var result = await handler.Handle(new FaqsGetFaqQuery { Id = faq.Id }, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(faq.Id, result!.Id);
    }

    [Fact]
    public async Task SoftDelete_HidesRecordsByDefault()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId, "Soft delete");

        context.DbContext.Faqs.Remove(faq);
        await context.DbContext.SaveChangesAsync();

        var handler = new FaqsGetFaqQueryHandler(context.DbContext);
        var result = await handler.Handle(new FaqsGetFaqQuery { Id = faq.Id }, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task SoftDelete_CanBeBypassedWhenDisabled()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId, "Soft delete");

        context.DbContext.Faqs.Remove(faq);
        await context.DbContext.SaveChangesAsync();

        context.DbContext.SoftDeleteFiltersEnabled = false;

        var handler = new FaqsGetFaqQueryHandler(context.DbContext);
        var result = await handler.Handle(new FaqsGetFaqQuery { Id = faq.Id }, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(faq.Id, result!.Id);
    }

    [Fact]
    public async Task Sorting_UsesExplicitFields()
    {
        using var context = TestContext.Create();
        await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId, "Alpha", "en-ZA");
        await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId, "Bravo", "en-US");

        var handler = new FaqsGetFaqListQueryHandler(context.DbContext);
        var request = new FaqsGetFaqListQuery
        {
            Request = new FaqGetAllRequestDto
            {
                SkipCount = 0,
                MaxResultCount = 10,
                Sorting = "language DESC, name ASC"
            }
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal("en-ZA", result.Items[0].Language);
        Assert.Equal("Alpha", result.Items[0].Name);
    }

    [Fact]
    public async Task Sorting_FallsBackToUpdatedDateWhenInvalidField()
    {
        using var context = TestContext.Create();
        var first = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId, "Zulu");
        await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId, "Alpha");
        first.Status = first.Status == FaqStatus.Draft ? FaqStatus.Published : FaqStatus.Draft;
        await context.DbContext.SaveChangesAsync();

        var handler = new FaqsGetFaqListQueryHandler(context.DbContext);
        var request = new FaqsGetFaqListQuery
        {
            Request = new FaqGetAllRequestDto
            {
                SkipCount = 0,
                MaxResultCount = 10,
                Sorting = "unknown DESC"
            }
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal("Zulu", result.Items[0].Name);
        Assert.Equal("Alpha", result.Items[1].Name);
    }
}
