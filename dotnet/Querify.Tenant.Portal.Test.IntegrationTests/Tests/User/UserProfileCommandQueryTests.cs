using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Models.User.Enums;
using Querify.Tenant.Portal.Business.User.Commands.UpdateUserProfile;
using Querify.Tenant.Portal.Business.User.Queries.GetUserProfile;
using Querify.Tenant.Portal.Test.IntegrationTests.Helpers;
using Xunit;

namespace Querify.Tenant.Portal.Test.IntegrationTests.Tests.User;

public class UserProfileCommandQueryTests
{
    [Fact]
    public async Task GetUserProfile_ReturnsCurrentUserProfile()
    {
        var currentUserId = Guid.NewGuid();
        using var context = TestContext.Create(userId: currentUserId);

        await TestDataFactory.SeedUserAsync(
            context.DbContext,
            id: currentUserId,
            givenName: "Casey",
            surName: "Doe",
            email: "casey@example.test",
            externalId: "ext-casey",
            phoneNumber: "555-0100",
            role: UserRoleType.Admin);

        var handler = new UsersGetUserProfileQueryHandler(context.DbContext, context.SessionService);
        var result = await handler.Handle(new UsersGetUserProfileQuery(), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Casey", result!.GivenName);
        Assert.Equal("Doe", result.SurName);
        Assert.Equal("casey@example.test", result.Email);
        Assert.Equal("555-0100", result.PhoneNumber);
    }

    [Fact]
    public async Task GetUserProfile_ReturnsNullWhenCurrentUserMissing()
    {
        using var context = TestContext.Create(userId: Guid.NewGuid());

        var handler = new UsersGetUserProfileQueryHandler(context.DbContext, context.SessionService);
        var result = await handler.Handle(new UsersGetUserProfileQuery(), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateUserProfile_UpdatesOnlyAllowedFields()
    {
        var currentUserId = Guid.NewGuid();
        using var context = TestContext.Create(userId: currentUserId);

        await TestDataFactory.SeedUserAsync(
            context.DbContext,
            id: currentUserId,
            givenName: "Before",
            surName: "BeforeSur",
            email: "before@example.test",
            externalId: "ext-before",
            phoneNumber: "555-0000",
            role: UserRoleType.Admin);

        var handler = new UsersUpdateUserProfileCommandHandler(context.DbContext, context.SessionService);
        var request = new UsersUpdateUserProfileCommand
        {
            GivenName = "After",
            SurName = "AfterSur",
            PhoneNumber = "555-0200"
        };

        await handler.Handle(request, CancellationToken.None);

        var updated = await context.DbContext.Users.FindAsync(currentUserId);

        Assert.NotNull(updated);
        Assert.Equal("After", updated!.GivenName);
        Assert.Equal("AfterSur", updated.SurName);
        Assert.Equal("555-0200", updated.PhoneNumber);
        Assert.Equal("before@example.test", updated.Email);
        Assert.Equal("ext-before", updated.ExternalId);
        Assert.Equal(UserRoleType.Admin, updated.Role);
    }

    [Fact]
    public async Task UpdateUserProfile_ThrowsWhenCurrentUserMissing()
    {
        using var context = TestContext.Create(userId: Guid.NewGuid());

        var handler = new UsersUpdateUserProfileCommandHandler(context.DbContext, context.SessionService);
        var request = new UsersUpdateUserProfileCommand
        {
            GivenName = "Missing",
            SurName = "User",
            PhoneNumber = "555-0300"
        };

        var exception =
            await Assert.ThrowsAsync<ApiErrorException>(() => handler.Handle(request, CancellationToken.None));

        Assert.Equal(404, exception.ErrorCode);
    }

    [Fact]
    public async Task UpdateUserProfile_NormalizesNullPhoneNumberToEmpty()
    {
        var currentUserId = Guid.NewGuid();
        using var context = TestContext.Create(userId: currentUserId);

        await TestDataFactory.SeedUserAsync(
            context.DbContext,
            id: currentUserId,
            givenName: "Before",
            surName: "BeforeSur",
            email: "before@example.test",
            externalId: "ext-before",
            phoneNumber: "555-0000",
            role: UserRoleType.Member);

        var handler = new UsersUpdateUserProfileCommandHandler(context.DbContext, context.SessionService);
        await handler.Handle(new UsersUpdateUserProfileCommand
        {
            GivenName = "After",
            SurName = "AfterSur",
            PhoneNumber = null
        }, CancellationToken.None);

        var updated = await context.DbContext.Users.FindAsync(currentUserId);
        Assert.NotNull(updated);
        Assert.Equal(string.Empty, updated!.PhoneNumber);
    }

    [Fact]
    public async Task UpdateUserProfile_TrimsAndPersistsSupportedTimeZone()
    {
        var currentUserId = Guid.NewGuid();
        using var context = TestContext.Create(userId: currentUserId);

        await TestDataFactory.SeedUserAsync(
            context.DbContext,
            id: currentUserId,
            givenName: "Before",
            surName: "BeforeSur",
            email: "before@example.test",
            externalId: "ext-before",
            phoneNumber: "555-0000",
            role: UserRoleType.Member);

        var handler = new UsersUpdateUserProfileCommandHandler(context.DbContext, context.SessionService);
        await handler.Handle(new UsersUpdateUserProfileCommand
        {
            GivenName = "After",
            SurName = "AfterSur",
            PhoneNumber = "555-0400",
            TimeZone = "  America/Vancouver  "
        }, CancellationToken.None);

        var updated = await context.DbContext.Users.FindAsync(currentUserId);
        Assert.NotNull(updated);
        Assert.Equal("America/Vancouver", updated!.TimeZone);
    }

    [Fact]
    public async Task UpdateUserProfile_RejectsUnsupportedTimeZone()
    {
        var currentUserId = Guid.NewGuid();
        using var context = TestContext.Create(userId: currentUserId);

        await TestDataFactory.SeedUserAsync(
            context.DbContext,
            id: currentUserId,
            givenName: "Before",
            surName: "BeforeSur",
            email: "before@example.test",
            externalId: "ext-before",
            phoneNumber: "555-0000",
            role: UserRoleType.Member);

        var handler = new UsersUpdateUserProfileCommandHandler(context.DbContext, context.SessionService);
        var exception = await Assert.ThrowsAsync<ApiErrorException>(() => handler.Handle(
            new UsersUpdateUserProfileCommand
            {
                GivenName = "After",
                SurName = "AfterSur",
                PhoneNumber = "555-0500",
                TimeZone = "Not/A_Time_Zone"
            },
            CancellationToken.None));

        Assert.Equal(422, exception.ErrorCode);
    }
}
