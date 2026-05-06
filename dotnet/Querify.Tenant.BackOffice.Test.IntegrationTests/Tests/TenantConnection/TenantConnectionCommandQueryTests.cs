using Querify.Common.Architecture.Test.IntegrationTest.Shared.Tenancy;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Models.Common.Enums;
using Querify.Models.Tenant.Dtos.TenantConnection;
using Querify.Tenant.BackOffice.Business.Tenant.Commands.CreateTenantConnection;
using Querify.Tenant.BackOffice.Business.Tenant.Commands.DeleteTenantConnection;
using Querify.Tenant.BackOffice.Business.Tenant.Commands.UpdateTenantConnection;
using Querify.Tenant.BackOffice.Business.Tenant.Queries.GetTenantConnection;
using Querify.Tenant.BackOffice.Business.Tenant.Queries.GetTenantConnectionList;
using Querify.Tenant.BackOffice.Test.IntegrationTests.Helpers;
using Xunit;

namespace Querify.Tenant.BackOffice.Test.IntegrationTests.Tests.TenantConnection;

public class TenantConnectionCommandQueryTests
{
    [Fact]
    public async Task CreateTenantConnection_PersistsEntityAndReturnsId()
    {
        using var context = TestContext.Create();

        var handler = new TenantConnectionsCreateTenantConnectionCommandHandler(context.DbContext);
        var request = new TenantConnectionsCreateTenantConnectionCommand
        {
            Module = ModuleEnum.Tenant,
            ConnectionString = IntegrationTestConnectionStrings.Tenant,
            IsCurrent = true
        };

        var id = await handler.Handle(request, CancellationToken.None);

        var connection = await context.DbContext.TenantConnections.FindAsync(id);
        Assert.NotNull(connection);
        Assert.Equal(ModuleEnum.Tenant, connection!.Module);
        Assert.Equal(request.ConnectionString, connection.ConnectionString);
        Assert.True(connection.IsCurrent);
    }

    [Fact]
    public async Task UpdateTenantConnection_UpdatesExistingConnection()
    {
        using var context = TestContext.Create();
        var connection = await TestDataFactory.SeedTenantConnectionAsync(context.DbContext, module: ModuleEnum.QnA);

        var handler = new TenantConnectionsUpdateTenantConnectionCommandHandler(context.DbContext);
        var request = new TenantConnectionsUpdateTenantConnectionCommand
        {
            Id = connection.Id,
            Module = ModuleEnum.Tenant,
            ConnectionString = IntegrationTestConnectionStrings.CreateNamed("updated"),
            IsCurrent = false
        };

        await handler.Handle(request, CancellationToken.None);

        var updated = await context.DbContext.TenantConnections.FindAsync(connection.Id);
        Assert.NotNull(updated);
        Assert.Equal(ModuleEnum.Tenant, updated!.Module);
        Assert.Equal(request.ConnectionString, updated.ConnectionString);
        Assert.False(updated.IsCurrent);
    }

    [Fact]
    public async Task UpdateTenantConnection_ThrowsWhenMissing()
    {
        using var context = TestContext.Create();
        var handler = new TenantConnectionsUpdateTenantConnectionCommandHandler(context.DbContext);
        var request = new TenantConnectionsUpdateTenantConnectionCommand
        {
            Id = Guid.NewGuid(),
            Module = ModuleEnum.QnA,
            ConnectionString = IntegrationTestConnectionStrings.CreateNamed("missing"),
            IsCurrent = true
        };

        var exception =
            await Assert.ThrowsAsync<ApiErrorException>(() => handler.Handle(request, CancellationToken.None));

        Assert.Equal(404, exception.ErrorCode);
    }

    [Fact]
    public async Task DeleteTenantConnection_SoftDeletesEntity()
    {
        using var context = TestContext.Create();
        var connection = await TestDataFactory.SeedTenantConnectionAsync(context.DbContext);

        var handler = new TenantConnectionsDeleteTenantConnectionCommandHandler(context.DbContext);
        await handler.Handle(new TenantConnectionsDeleteTenantConnectionCommand { Id = connection.Id },
            CancellationToken.None);

        context.DbContext.SoftDeleteFiltersEnabled = false;
        var deleted = await context.DbContext.TenantConnections.FindAsync(connection.Id);
        Assert.NotNull(deleted);
        Assert.True(deleted!.IsDeleted);
    }

    [Fact]
    public async Task GetTenantConnection_ReturnsDto()
    {
        using var context = TestContext.Create();
        var connection = await TestDataFactory.SeedTenantConnectionAsync(context.DbContext, module: ModuleEnum.QnA);

        var handler = new TenantConnectionsGetTenantConnectionQueryHandler(context.DbContext);
        var result =
            await handler.Handle(new TenantConnectionsGetTenantConnectionQuery { Id = connection.Id },
                CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(connection.Id, result!.Id);
        Assert.Equal(connection.Module, result.Module);
        Assert.Equal(string.Empty, result.ConnectionString);
        Assert.Equal(connection.IsCurrent, result.IsCurrent);
    }

    [Fact]
    public async Task GetTenantConnection_ReturnsNullWhenMissing()
    {
        using var context = TestContext.Create();
        var handler = new TenantConnectionsGetTenantConnectionQueryHandler(context.DbContext);

        var result =
            await handler.Handle(new TenantConnectionsGetTenantConnectionQuery { Id = Guid.NewGuid() },
                CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetTenantConnectionList_ReturnsPagedItems()
    {
        using var context = TestContext.Create();
        await TestDataFactory.SeedTenantConnectionAsync(context.DbContext, module: ModuleEnum.QnA);
        await TestDataFactory.SeedTenantConnectionAsync(context.DbContext, module: ModuleEnum.Tenant);

        var handler = new TenantConnectionsGetTenantConnectionListQueryHandler(context.DbContext);
        var request = new TenantConnectionsGetTenantConnectionListQuery
        {
            Request = new TenantConnectionGetAllRequestDto { SkipCount = 0, MaxResultCount = 10 }
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task GetTenantConnectionList_SortsByExplicitField()
    {
        using var context = TestContext.Create();
        await TestDataFactory.SeedTenantConnectionAsync(context.DbContext, module: ModuleEnum.QnA, isCurrent: false);
        await TestDataFactory.SeedTenantConnectionAsync(context.DbContext, module: ModuleEnum.Tenant, isCurrent: true);

        var handler = new TenantConnectionsGetTenantConnectionListQueryHandler(context.DbContext);
        var request = new TenantConnectionsGetTenantConnectionListQuery
        {
            Request = new TenantConnectionGetAllRequestDto
            {
                SkipCount = 0,
                MaxResultCount = 10,
                Sorting = "isCurrent DESC"
            }
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.True(result.Items[0].IsCurrent);
        Assert.False(result.Items[1].IsCurrent);
    }

    [Fact]
    public async Task GetTenantConnectionList_FallsBackToUpdatedDateWhenSortingInvalid()
    {
        using var context = TestContext.Create();
        var first = await TestDataFactory.SeedTenantConnectionAsync(context.DbContext, module: ModuleEnum.Tenant);
        await TestDataFactory.SeedTenantConnectionAsync(context.DbContext, module: ModuleEnum.QnA);
        first.IsCurrent = !first.IsCurrent;
        await context.DbContext.SaveChangesAsync();

        var handler = new TenantConnectionsGetTenantConnectionListQueryHandler(context.DbContext);
        var request = new TenantConnectionsGetTenantConnectionListQuery
        {
            Request = new TenantConnectionGetAllRequestDto
            {
                SkipCount = 0,
                MaxResultCount = 10,
                Sorting = "unknown DESC"
            }
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(ModuleEnum.Tenant, result.Items[0].Module);
        Assert.Equal(ModuleEnum.QnA, result.Items[1].Module);
    }

    [Fact]
    public async Task GetTenantConnectionList_SortsByMultipleFields()
    {
        using var context = TestContext.Create();

        var connectionA = new Querify.Common.EntityFramework.Tenant.Entities.TenantConnection
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000021"),
            Module = ModuleEnum.QnA,
            ConnectionString = IntegrationTestConnectionStrings.CreateNamed("a"),
            IsCurrent = false
        };
        var connectionB = new Querify.Common.EntityFramework.Tenant.Entities.TenantConnection
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000022"),
            Module = ModuleEnum.QnA,
            ConnectionString = IntegrationTestConnectionStrings.CreateNamed("b"),
            IsCurrent = true
        };

        context.DbContext.TenantConnections.AddRange(connectionA, connectionB);
        await context.DbContext.SaveChangesAsync();

        var handler = new TenantConnectionsGetTenantConnectionListQueryHandler(context.DbContext);
        var request = new TenantConnectionsGetTenantConnectionListQuery
        {
            Request = new TenantConnectionGetAllRequestDto
            {
                SkipCount = 0,
                MaxResultCount = 10,
                Sorting = "module ASC, isCurrent DESC"
            }
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(connectionB.Id, result.Items[0].Id);
        Assert.Equal(connectionA.Id, result.Items[1].Id);
    }

    [Fact]
    public async Task GetTenantConnectionList_AppliesPaginationWindow()
    {
        using var context = TestContext.Create();
        await TestDataFactory.SeedTenantConnectionAsync(context.DbContext, module: ModuleEnum.QnA, isCurrent: false);
        await TestDataFactory.SeedTenantConnectionAsync(context.DbContext, module: ModuleEnum.Tenant, isCurrent: true);
        await TestDataFactory.SeedTenantConnectionAsync(context.DbContext, module: ModuleEnum.QnA, isCurrent: true);

        var handler = new TenantConnectionsGetTenantConnectionListQueryHandler(context.DbContext);
        var request = new TenantConnectionsGetTenantConnectionListQuery
        {
            Request = new TenantConnectionGetAllRequestDto
            {
                SkipCount = 1,
                MaxResultCount = 1,
                Sorting = "isCurrent DESC, module ASC"
            }
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(3, result.TotalCount);
        Assert.Single(result.Items);
    }
}
