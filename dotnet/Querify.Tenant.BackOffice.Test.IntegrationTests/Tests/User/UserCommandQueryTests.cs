using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Models.User.Dtos.User;
using Querify.Models.User.Enums;
using Querify.Tenant.BackOffice.Business.User.Commands.CreateUser;
using Querify.Tenant.BackOffice.Business.User.Commands.DeleteUser;
using Querify.Tenant.BackOffice.Business.User.Commands.UpdateUser;
using Querify.Tenant.BackOffice.Business.User.Queries.GetUser;
using Querify.Tenant.BackOffice.Business.User.Queries.GetUserList;
using Querify.Tenant.BackOffice.Test.IntegrationTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Querify.Tenant.BackOffice.Test.IntegrationTests.Tests.User;

public class UserCommandQueryTests
{
    [Fact]
    public async Task CreateUser_PersistsEntityAndReturnsId()
    {
        using var context = TestContext.Create();

        var handler = new UsersCreateUserCommandHandler(context.DbContext);
        var request = new UsersCreateUserCommand
        {
            GivenName = "Casey",
            SurName = "Doe",
            Email = "casey@example.test",
            ExternalId = "ext-casey",
            PhoneNumber = "555-0100",
            Role = UserRoleType.Admin
        };

        var id = await handler.Handle(request, CancellationToken.None);

        var user = await context.DbContext.Users.FindAsync(id);
        Assert.NotNull(user);
        Assert.Equal("Casey", user!.GivenName);
        Assert.Equal("Doe", user.SurName);
        Assert.Equal("casey@example.test", user.Email);
        Assert.Equal("ext-casey", user.ExternalId);
        Assert.Equal("555-0100", user.PhoneNumber);
        Assert.Equal(UserRoleType.Admin, user.Role);
    }

    [Fact]
    public async Task UpdateUser_UpdatesExistingUser()
    {
        using var context = TestContext.Create();
        var user = await TestDataFactory.SeedUserAsync(
            context.DbContext,
            givenName: "Alex",
            email: "alex@example.test",
            externalId: "ext-alex");

        var handler = new UsersUpdateUserCommandHandler(context.DbContext);
        var request = new UsersUpdateUserCommand
        {
            Id = user.Id,
            GivenName = "Jordan",
            SurName = "Updated",
            Email = "jordan@example.test",
            ExternalId = "ext-jordan",
            PhoneNumber = "555-0200",
            Role = UserRoleType.Member
        };

        await handler.Handle(request, CancellationToken.None);

        var updated = await context.DbContext.Users.FindAsync(user.Id);
        Assert.NotNull(updated);
        Assert.Equal("Jordan", updated!.GivenName);
        Assert.Equal("Updated", updated.SurName);
        Assert.Equal("jordan@example.test", updated.Email);
        Assert.Equal("ext-jordan", updated.ExternalId);
        Assert.Equal("555-0200", updated.PhoneNumber);
        Assert.Equal(UserRoleType.Member, updated.Role);
    }

    [Fact]
    public async Task UpdateUser_ThrowsWhenMissing()
    {
        using var context = TestContext.Create();
        var handler = new UsersUpdateUserCommandHandler(context.DbContext);
        var request = new UsersUpdateUserCommand
        {
            Id = Guid.NewGuid(),
            GivenName = "Missing",
            SurName = "User",
            Email = "missing@example.test",
            ExternalId = "ext-missing",
            PhoneNumber = "555-0300",
            Role = UserRoleType.Member
        };

        var exception =
            await Assert.ThrowsAsync<ApiErrorException>(() => handler.Handle(request, CancellationToken.None));

        Assert.Equal(404, exception.ErrorCode);
    }

    [Fact]
    public async Task DeleteUser_SoftDeletesEntity()
    {
        using var context = TestContext.Create();
        var user = await TestDataFactory.SeedUserAsync(context.DbContext, givenName: "Delete");

        var handler = new UsersDeleteUserCommandHandler(context.DbContext);
        await handler.Handle(new UsersDeleteUserCommand { Id = user.Id }, CancellationToken.None);

        context.DbContext.SoftDeleteFiltersEnabled = false;
        var deleted = await context.DbContext.Users.FindAsync(user.Id);
        Assert.NotNull(deleted);
        Assert.True(deleted!.IsDeleted);
    }

    [Fact]
    public async Task GetUser_ReturnsDto()
    {
        using var context = TestContext.Create();
        var user = await TestDataFactory.SeedUserAsync(
            context.DbContext,
            givenName: "Riley",
            surName: "Smith",
            email: "riley@example.test",
            externalId: "ext-riley",
            phoneNumber: "555-0400",
            role: UserRoleType.Admin);

        var handler = new UsersGetUserQueryHandler(context.DbContext);
        var result = await handler.Handle(new UsersGetUserQuery { Id = user.Id }, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(user.Id, result!.Id);
        Assert.Equal("Riley", result.GivenName);
        Assert.Equal("Smith", result.SurName);
        Assert.Equal("riley@example.test", result.Email);
        Assert.Equal("ext-riley", result.ExternalId);
        Assert.Equal("555-0400", result.PhoneNumber);
        Assert.Equal(UserRoleType.Admin, result.Role);
    }

    [Fact]
    public async Task GetUser_ReturnsNullWhenMissing()
    {
        using var context = TestContext.Create();
        var handler = new UsersGetUserQueryHandler(context.DbContext);

        var result = await handler.Handle(new UsersGetUserQuery { Id = Guid.NewGuid() }, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserList_ReturnsPagedItems()
    {
        using var context = TestContext.Create();
        await TestDataFactory.SeedUserAsync(context.DbContext, givenName: "Alpha");
        await TestDataFactory.SeedUserAsync(context.DbContext, givenName: "Bravo");

        var handler = new UsersGetUserListQueryHandler(context.DbContext);
        var request = new UsersGetUserListQuery
        {
            Request = new UserGetAllRequestDto { SkipCount = 0, MaxResultCount = 10 }
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task GetUserList_SortsByExplicitField()
    {
        using var context = TestContext.Create();
        await TestDataFactory.SeedUserAsync(context.DbContext, email: "b@example.test");
        await TestDataFactory.SeedUserAsync(context.DbContext, email: "a@example.test");

        var handler = new UsersGetUserListQueryHandler(context.DbContext);
        var request = new UsersGetUserListQuery
        {
            Request = new UserGetAllRequestDto
            {
                SkipCount = 0,
                MaxResultCount = 10,
                Sorting = "email DESC"
            }
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal("b@example.test", result.Items[0].Email);
        Assert.Equal("a@example.test", result.Items[1].Email);
    }

    [Fact]
    public async Task GetUserList_FallsBackToUpdatedDateWhenSortingInvalid()
    {
        using var context = TestContext.Create();
        var first = await TestDataFactory.SeedUserAsync(context.DbContext, givenName: "Zulu");
        await TestDataFactory.SeedUserAsync(context.DbContext, givenName: "Alpha");
        first.PhoneNumber = "555-0200";
        await context.DbContext.SaveChangesAsync();

        var handler = new UsersGetUserListQueryHandler(context.DbContext);
        var request = new UsersGetUserListQuery
        {
            Request = new UserGetAllRequestDto
            {
                SkipCount = 0,
                MaxResultCount = 10,
                Sorting = "unknown DESC"
            }
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal("Zulu", result.Items[0].GivenName);
        Assert.Equal("Alpha", result.Items[1].GivenName);
    }

    [Fact]
    public async Task GetUserList_SortsByMultipleFields()
    {
        using var context = TestContext.Create();

        var userA = new Querify.Common.EntityFramework.Tenant.Entities.User
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000031"),
            GivenName = "Same",
            SurName = "Bravo",
            Email = "b@example.test",
            ExternalId = "ext-b",
            PhoneNumber = "555-1111",
            Role = UserRoleType.Member
        };
        var userB = new Querify.Common.EntityFramework.Tenant.Entities.User
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000032"),
            GivenName = "Same",
            SurName = "Alpha",
            Email = "a@example.test",
            ExternalId = "ext-a",
            PhoneNumber = "555-2222",
            Role = UserRoleType.Member
        };

        context.DbContext.Users.AddRange(userA, userB);
        await context.DbContext.SaveChangesAsync();

        var handler = new UsersGetUserListQueryHandler(context.DbContext);
        var request = new UsersGetUserListQuery
        {
            Request = new UserGetAllRequestDto
            {
                SkipCount = 0,
                MaxResultCount = 10,
                Sorting = "givenName ASC, surName ASC"
            }
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(userB.Id, result.Items[0].Id);
        Assert.Equal(userA.Id, result.Items[1].Id);
    }

    [Fact]
    public async Task GetUserList_AppliesPaginationWindow()
    {
        using var context = TestContext.Create();
        await TestDataFactory.SeedUserAsync(context.DbContext, givenName: "Charlie", email: "charlie@example.test");
        await TestDataFactory.SeedUserAsync(context.DbContext, givenName: "Alpha", email: "alpha@example.test");
        await TestDataFactory.SeedUserAsync(context.DbContext, givenName: "Bravo", email: "bravo@example.test");

        var handler = new UsersGetUserListQueryHandler(context.DbContext);
        var request = new UsersGetUserListQuery
        {
            Request = new UserGetAllRequestDto
            {
                SkipCount = 1,
                MaxResultCount = 1,
                Sorting = "givenName ASC"
            }
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(3, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal("Bravo", result.Items[0].GivenName);
    }

    [Fact]
    public async Task CreateUser_ThrowsWhenExternalIdDuplicated()
    {
        using var context = TestContext.Create();
        await TestDataFactory.SeedUserAsync(context.DbContext, externalId: "dup-ext");

        var handler = new UsersCreateUserCommandHandler(context.DbContext);
        var request = new UsersCreateUserCommand
        {
            GivenName = "Dup",
            SurName = "User",
            Email = "dup@example.test",
            ExternalId = "dup-ext",
            PhoneNumber = "555-9999",
            Role = UserRoleType.Member
        };

        await Assert.ThrowsAsync<DbUpdateException>(() => handler.Handle(request, CancellationToken.None));
    }
}