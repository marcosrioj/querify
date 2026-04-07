using BaseFaq.Models.Tenant.Enums;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Tenant.Portal.Business.Tenant.Commands.CreateTenantUser;
using BaseFaq.Tenant.Portal.Business.Tenant.Commands.DeleteTenantUser;
using BaseFaq.Tenant.Portal.Business.Tenant.Commands.UpdateTenantUser;
using BaseFaq.Tenant.Portal.Business.Tenant.Queries.GetTenantUserList;
using BaseFaq.Tenant.Portal.Business.Tenant.Service;
using BaseFaq.Tenant.Portal.Test.IntegrationTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BaseFaq.Tenant.Portal.Test.IntegrationTests.Tests.TenantUser;

public class TenantUserCommandQueryTests
{
    [Fact]
    public async Task GetTenantUserList_ReturnsOwnerAndMembersForSelectedTenant()
    {
        var tenantId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        using var context = TestContext.Create(tenantId: tenantId, userId: currentUserId);

        await TestDataFactory.SeedTenantAsync(
            context.DbContext,
            id: tenantId,
            userId: currentUserId);

        var member = await TestDataFactory.SeedUserAsync(
            context.DbContext,
            email: "member@example.test");
        await TestDataFactory.SeedTenantUserAsync(
            context.DbContext,
            tenantId,
            userId: member.Id,
            role: TenantUserRoleType.Member);

        var handler = new TenantUsersGetTenantUserListQueryHandler(
            context.DbContext,
            context.SessionService,
            new TenantPortalAccessService(context.DbContext, context.SessionService));
        var result = await handler.Handle(
            new TenantUsersGetTenantUserListQuery { TenantId = tenantId },
            CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal(TenantUserRoleType.Owner, result[0].Role);
        Assert.True(result[0].IsCurrentUser);
        Assert.Equal("member@example.test", result[1].Email);
        Assert.Equal(TenantUserRoleType.Member, result[1].Role);
    }

    [Fact]
    public async Task CreateTenantUser_AddsExistingUserByEmail()
    {
        var tenantId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        using var context = TestContext.Create(tenantId: tenantId, userId: currentUserId);

        await TestDataFactory.SeedTenantAsync(
            context.DbContext,
            id: tenantId,
            userId: currentUserId);
        var user = await TestDataFactory.SeedUserAsync(
            context.DbContext,
            email: "invitee@example.test");

        var handler = new TenantUsersCreateTenantUserCommandHandler(
            context.DbContext,
            new TestAllowedTenantStore(),
            new TenantPortalAccessService(context.DbContext, context.SessionService));

        var id = await handler.Handle(
            new TenantUsersCreateTenantUserCommand
            {
                TenantId = tenantId,
                Email = "invitee@example.test",
                Role = TenantUserRoleType.Member
            },
            CancellationToken.None);

        var tenantUser = await context.DbContext.TenantUsers.FindAsync(id);
        Assert.NotNull(tenantUser);
        Assert.Equal(tenantId, tenantUser!.TenantId);
        Assert.Equal(user.Id, tenantUser.UserId);
        Assert.Equal(TenantUserRoleType.Member, tenantUser.Role);
    }

    [Fact]
    public async Task CreateTenantUser_ThrowsWhenCurrentUserIsNotOwner()
    {
        var tenantId = Guid.NewGuid();
        var ownerUserId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        using var context = TestContext.Create(tenantId: tenantId, userId: currentUserId);

        await TestDataFactory.SeedTenantAsync(
            context.DbContext,
            id: tenantId,
            userId: ownerUserId);
        await TestDataFactory.SeedTenantUserAsync(
            context.DbContext,
            tenantId,
            userId: currentUserId,
            role: TenantUserRoleType.Member);
        await TestDataFactory.SeedUserAsync(
            context.DbContext,
            email: "invitee@example.test");

        var handler = new TenantUsersCreateTenantUserCommandHandler(
            context.DbContext,
            new TestAllowedTenantStore(),
            new TenantPortalAccessService(context.DbContext, context.SessionService));

        var exception = await Assert.ThrowsAsync<ApiErrorException>(() => handler.Handle(
            new TenantUsersCreateTenantUserCommand
            {
                TenantId = tenantId,
                Email = "invitee@example.test",
                Role = TenantUserRoleType.Member
            },
            CancellationToken.None));

        Assert.Equal(403, exception.ErrorCode);
    }

    [Fact]
    public async Task CreateTenantUser_ThrowsWhenRequestRoleIsOwner()
    {
        var tenantId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        using var context = TestContext.Create(tenantId: tenantId, userId: currentUserId);

        await TestDataFactory.SeedTenantAsync(
            context.DbContext,
            id: tenantId,
            userId: currentUserId);
        await TestDataFactory.SeedUserAsync(
            context.DbContext,
            email: "invitee@example.test");

        var handler = new TenantUsersCreateTenantUserCommandHandler(
            context.DbContext,
            new TestAllowedTenantStore(),
            new TenantPortalAccessService(context.DbContext, context.SessionService));

        var exception = await Assert.ThrowsAsync<ApiErrorException>(() => handler.Handle(
            new TenantUsersCreateTenantUserCommand
            {
                TenantId = tenantId,
                Email = "invitee@example.test",
                Role = TenantUserRoleType.Owner
            },
            CancellationToken.None));

        Assert.Equal(400, exception.ErrorCode);
    }

    [Fact]
    public async Task UpdateTenantUser_KeepsMemberRole_WhenCurrentUserIsOwner()
    {
        var tenantId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        using var context = TestContext.Create(tenantId: tenantId, userId: currentUserId);

        await TestDataFactory.SeedTenantAsync(
            context.DbContext,
            id: tenantId,
            userId: currentUserId);
        var member = await TestDataFactory.SeedUserAsync(
            context.DbContext,
            email: "member@example.test");
        var memberLink = await TestDataFactory.SeedTenantUserAsync(
            context.DbContext,
            tenantId,
            userId: member.Id,
            role: TenantUserRoleType.Member);

        var handler = new TenantUsersUpdateTenantUserCommandHandler(
            context.DbContext,
            new TestAllowedTenantStore(),
            new TenantPortalAccessService(context.DbContext, context.SessionService));

        await handler.Handle(
            new TenantUsersUpdateTenantUserCommand
            {
                TenantId = tenantId,
                Id = memberLink.Id,
                Role = TenantUserRoleType.Member
            },
            CancellationToken.None);

        var tenantUsers = await context.DbContext.TenantUsers
            .Where(entity => entity.TenantId == tenantId)
            .OrderBy(entity => entity.UserId)
            .ToListAsync();

        Assert.Contains(tenantUsers, entity => entity.UserId == member.Id && entity.Role == TenantUserRoleType.Member);
        Assert.Contains(
            tenantUsers,
            entity => entity.UserId == currentUserId && entity.Role == TenantUserRoleType.Owner);
    }

    [Fact]
    public async Task UpdateTenantUser_ThrowsWhenCurrentUserIsNotOwner()
    {
        var tenantId = Guid.NewGuid();
        var ownerUserId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        using var context = TestContext.Create(tenantId: tenantId, userId: currentUserId);

        await TestDataFactory.SeedTenantAsync(
            context.DbContext,
            id: tenantId,
            userId: ownerUserId);
        await TestDataFactory.SeedTenantUserAsync(
            context.DbContext,
            tenantId,
            userId: currentUserId,
            role: TenantUserRoleType.Member);
        var member = await TestDataFactory.SeedUserAsync(
            context.DbContext,
            email: "member@example.test");
        var memberLink = await TestDataFactory.SeedTenantUserAsync(
            context.DbContext,
            tenantId,
            userId: member.Id,
            role: TenantUserRoleType.Member);

        var handler = new TenantUsersUpdateTenantUserCommandHandler(
            context.DbContext,
            new TestAllowedTenantStore(),
            new TenantPortalAccessService(context.DbContext, context.SessionService));

        var exception = await Assert.ThrowsAsync<ApiErrorException>(() => handler.Handle(
            new TenantUsersUpdateTenantUserCommand
            {
                TenantId = tenantId,
                Id = memberLink.Id,
                Role = TenantUserRoleType.Member
            },
            CancellationToken.None));

        Assert.Equal(403, exception.ErrorCode);
    }

    [Fact]
    public async Task UpdateTenantUser_ThrowsWhenRequestRoleIsOwner()
    {
        var tenantId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        using var context = TestContext.Create(tenantId: tenantId, userId: currentUserId);

        await TestDataFactory.SeedTenantAsync(
            context.DbContext,
            id: tenantId,
            userId: currentUserId);
        var member = await TestDataFactory.SeedUserAsync(
            context.DbContext,
            email: "member@example.test");
        var memberLink = await TestDataFactory.SeedTenantUserAsync(
            context.DbContext,
            tenantId,
            userId: member.Id,
            role: TenantUserRoleType.Member);

        var handler = new TenantUsersUpdateTenantUserCommandHandler(
            context.DbContext,
            new TestAllowedTenantStore(),
            new TenantPortalAccessService(context.DbContext, context.SessionService));

        var exception = await Assert.ThrowsAsync<ApiErrorException>(() => handler.Handle(
            new TenantUsersUpdateTenantUserCommand
            {
                TenantId = tenantId,
                Id = memberLink.Id,
                Role = TenantUserRoleType.Owner
            },
            CancellationToken.None));

        Assert.Equal(400, exception.ErrorCode);
    }

    [Fact]
    public async Task DeleteTenantUser_RemovesMemberButNotOwner()
    {
        var tenantId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        using var context = TestContext.Create(tenantId: tenantId, userId: currentUserId);

        await TestDataFactory.SeedTenantAsync(
            context.DbContext,
            id: tenantId,
            userId: currentUserId);
        var member = await TestDataFactory.SeedUserAsync(
            context.DbContext,
            email: "member@example.test");
        var memberLink = await TestDataFactory.SeedTenantUserAsync(
            context.DbContext,
            tenantId,
            userId: member.Id,
            role: TenantUserRoleType.Member);

        var handler = new TenantUsersDeleteTenantUserCommandHandler(
            context.DbContext,
            new TestAllowedTenantStore(),
            new TenantPortalAccessService(context.DbContext, context.SessionService));

        await handler.Handle(
            new TenantUsersDeleteTenantUserCommand { TenantId = tenantId, Id = memberLink.Id },
            CancellationToken.None);

        var visibleTenantUser = await context.DbContext.TenantUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.Id == memberLink.Id);
        Assert.Null(visibleTenantUser);

        context.DbContext.SoftDeleteFiltersEnabled = false;
        var deletedTenantUser = await context.DbContext.TenantUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.Id == memberLink.Id);
        Assert.NotNull(deletedTenantUser);
        Assert.True(deletedTenantUser!.IsDeleted);
    }

    [Fact]
    public async Task DeleteTenantUser_ThrowsWhenCurrentUserIsNotOwner()
    {
        var tenantId = Guid.NewGuid();
        var ownerUserId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        using var context = TestContext.Create(tenantId: tenantId, userId: currentUserId);

        await TestDataFactory.SeedTenantAsync(
            context.DbContext,
            id: tenantId,
            userId: ownerUserId);
        await TestDataFactory.SeedTenantUserAsync(
            context.DbContext,
            tenantId,
            userId: currentUserId,
            role: TenantUserRoleType.Member);
        var member = await TestDataFactory.SeedUserAsync(
            context.DbContext,
            email: "member@example.test");
        var memberLink = await TestDataFactory.SeedTenantUserAsync(
            context.DbContext,
            tenantId,
            userId: member.Id,
            role: TenantUserRoleType.Member);

        var handler = new TenantUsersDeleteTenantUserCommandHandler(
            context.DbContext,
            new TestAllowedTenantStore(),
            new TenantPortalAccessService(context.DbContext, context.SessionService));

        var exception = await Assert.ThrowsAsync<ApiErrorException>(() => handler.Handle(
            new TenantUsersDeleteTenantUserCommand { TenantId = tenantId, Id = memberLink.Id },
            CancellationToken.None));

        Assert.Equal(403, exception.ErrorCode);
    }
}
