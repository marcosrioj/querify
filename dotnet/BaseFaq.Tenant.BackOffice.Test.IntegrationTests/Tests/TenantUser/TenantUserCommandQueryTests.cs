using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Models.Tenant.Enums;
using BaseFaq.Tenant.BackOffice.Business.Tenant.Commands.CreateTenantUser;
using BaseFaq.Tenant.BackOffice.Business.Tenant.Commands.DeleteTenantUser;
using BaseFaq.Tenant.BackOffice.Business.Tenant.Commands.UpdateTenantUser;
using BaseFaq.Tenant.BackOffice.Business.Tenant.Queries.GetTenantUserList;
using BaseFaq.Tenant.BackOffice.Test.IntegrationTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BaseFaq.Tenant.BackOffice.Test.IntegrationTests.Tests.TenantUser;

public class TenantUserCommandQueryTests
{
    [Fact]
    public async Task GetTenantUserList_ReturnsOwnerAndMembersForTenant()
    {
        using var context = TestContext.Create();

        var ownerId = Guid.NewGuid();
        var tenant = await TestDataFactory.SeedTenantAsync(
            context.DbContext,
            userId: ownerId);
        var member = await TestDataFactory.SeedUserAsync(
            context.DbContext,
            email: "member@example.test");
        await TestDataFactory.SeedTenantUserAsync(
            context.DbContext,
            tenant.Id,
            userId: member.Id,
            role: TenantUserRoleType.Member);

        var handler = new TenantUsersGetTenantUserListQueryHandler(context.DbContext);
        var result = await handler.Handle(
            new TenantUsersGetTenantUserListQuery { TenantId = tenant.Id },
            CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal(TenantUserRoleType.Owner, result[0].Role);
        Assert.Equal(TenantUserRoleType.Member, result[1].Role);
        Assert.False(result[0].IsCurrentUser);
    }

    [Fact]
    public async Task CreateTenantUser_AddsExistingUserByEmail()
    {
        using var context = TestContext.Create();

        var tenant = await TestDataFactory.SeedTenantAsync(context.DbContext);
        var member = await TestDataFactory.SeedUserAsync(
            context.DbContext,
            email: "member@example.test");

        var handler = new TenantUsersCreateTenantUserCommandHandler(
            context.DbContext,
            new TestAllowedTenantStore());

        var id = await handler.Handle(
            new TenantUsersCreateTenantUserCommand
            {
                TenantId = tenant.Id,
                Email = "member@example.test",
                Role = TenantUserRoleType.Member
            },
            CancellationToken.None);

        var tenantUser = await context.DbContext.TenantUsers.FindAsync(id);
        Assert.NotNull(tenantUser);
        Assert.Equal(member.Id, tenantUser!.UserId);
        Assert.Equal(TenantUserRoleType.Member, tenantUser.Role);
    }

    [Fact]
    public async Task UpdateTenantUser_PromotesMemberToOwner()
    {
        using var context = TestContext.Create();

        var ownerId = Guid.NewGuid();
        var tenant = await TestDataFactory.SeedTenantAsync(
            context.DbContext,
            userId: ownerId);
        var member = await TestDataFactory.SeedUserAsync(
            context.DbContext,
            email: "member@example.test");
        var memberLink = await TestDataFactory.SeedTenantUserAsync(
            context.DbContext,
            tenant.Id,
            userId: member.Id,
            role: TenantUserRoleType.Member);

        var handler = new TenantUsersUpdateTenantUserCommandHandler(
            context.DbContext,
            new TestAllowedTenantStore());

        await handler.Handle(
            new TenantUsersUpdateTenantUserCommand
            {
                TenantId = tenant.Id,
                Id = memberLink.Id,
                Role = TenantUserRoleType.Owner
            },
            CancellationToken.None);

        var tenantUsers = await context.DbContext.TenantUsers
            .Where(entity => entity.TenantId == tenant.Id)
            .OrderBy(entity => entity.UserId)
            .ToListAsync();

        Assert.Contains(tenantUsers, entity => entity.UserId == member.Id && entity.Role == TenantUserRoleType.Owner);
        Assert.Contains(
            tenantUsers,
            entity => entity.UserId == ownerId && entity.Role == TenantUserRoleType.Member);
    }

    [Fact]
    public async Task DeleteTenantUser_ThrowsWhenRemovingOwner()
    {
        using var context = TestContext.Create();

        var tenant = await TestDataFactory.SeedTenantAsync(context.DbContext);
        var ownerLink = tenant.TenantUsers.Single(entity => entity.Role == TenantUserRoleType.Owner);

        var handler = new TenantUsersDeleteTenantUserCommandHandler(
            context.DbContext,
            new TestAllowedTenantStore());

        var exception = await Assert.ThrowsAsync<ApiErrorException>(() =>
            handler.Handle(
                new TenantUsersDeleteTenantUserCommand
                {
                    TenantId = tenant.Id,
                    Id = ownerLink.Id
                },
                CancellationToken.None));

        Assert.Equal(400, exception.ErrorCode);
    }
}
