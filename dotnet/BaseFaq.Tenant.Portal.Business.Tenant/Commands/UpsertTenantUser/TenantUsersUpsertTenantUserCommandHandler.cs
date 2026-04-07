using System.Net;
using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Common.EntityFramework.Tenant.Entities;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Common.Infrastructure.Core.Helpers;
using BaseFaq.Models.Tenant.Enums;
using BaseFaq.Tenant.Portal.Business.Tenant.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Tenant.Portal.Business.Tenant.Commands.UpsertTenantUser;

public class TenantUsersUpsertTenantUserCommandHandler(
    TenantDbContext dbContext,
    IAllowedTenantStore allowedTenantStore,
    ITenantPortalAccessService tenantPortalAccessService,
    ISessionService sessionService)
    : IRequestHandler<TenantUsersUpsertTenantUserCommand, Guid>
{
    public async Task<Guid> Handle(TenantUsersUpsertTenantUserCommand request, CancellationToken cancellationToken)
    {
        var tenantId = request.TenantId;
        var currentUserId = sessionService.GetUserId();
        await tenantPortalAccessService.EnsureOwnerAccessAsync(tenantId, cancellationToken);

        if (request.Role != TenantUserRoleType.Member)
        {
            throw new ApiErrorException(
                "Portal users can only add members to the workspace.",
                errorCode: (int)HttpStatusCode.BadRequest);
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var trimmedName = request.Name.Trim();
        var existingTenantUser = await dbContext.TenantUsers
            .Include(entity => entity.User)
            .FirstOrDefaultAsync(
                entity => entity.TenantId == tenantId && entity.User.Email.ToLower() == normalizedEmail,
                cancellationToken);

        if (existingTenantUser is not null)
        {
            if (existingTenantUser.UserId != currentUserId)
            {
                throw CreateExistingTenantUserEmailException();
            }

            existingTenantUser.User.GivenName = trimmedName;

            await dbContext.SaveChangesAsync(cancellationToken);
            await AllowedTenantCacheHelper.RemoveUserEntries(
                allowedTenantStore,
                [existingTenantUser.UserId],
                cancellationToken);

            return existingTenantUser.Id;
        }

        var user = await ResolveOrEnsureUserByEmailAsync(normalizedEmail, trimmedName, cancellationToken);
        if (user.Id == currentUserId)
        {
            user.GivenName = trimmedName;
        }

        var existingMembershipForUser = await dbContext.TenantUsers
            .FirstOrDefaultAsync(
                entity => entity.TenantId == tenantId && entity.UserId == user.Id,
                cancellationToken);

        if (existingMembershipForUser is not null)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            await AllowedTenantCacheHelper.RemoveUserEntries(
                allowedTenantStore,
                [existingMembershipForUser.UserId],
                cancellationToken);

            return existingMembershipForUser.Id;
        }

        var tenantUser = await CreateTenantUserAsync(tenantId, request.Role, user.Id, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        await AllowedTenantCacheHelper.RemoveUserEntries(allowedTenantStore, [user.Id], cancellationToken);

        return tenantUser.Id;
    }

    private static ApiErrorException CreateExistingTenantUserEmailException()
    {
        return new ApiErrorException(
            "This email is already a member of the workspace.",
            errorCode: (int)HttpStatusCode.BadRequest);
    }

    private async Task<TenantUser> CreateTenantUserAsync(
        Guid tenantId,
        TenantUserRoleType role,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var tenantUser = new TenantUser
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = userId,
            Role = role
        };

        await dbContext.TenantUsers.AddAsync(tenantUser, cancellationToken);
        return tenantUser;
    }

    private async Task<User> ResolveOrEnsureUserByEmailAsync(
        string normalizedEmail,
        string trimmedName,
        CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .FirstOrDefaultAsync(entity => entity.Email.ToLower() == normalizedEmail, cancellationToken);

        if (user is not null)
        {
            return user;
        }

        var userId = await dbContext.EnsureUser(
            normalizedEmail,
            trimmedName,
            normalizedEmail,
            cancellationToken);

        return await dbContext.Users.FirstAsync(entity => entity.Id == userId, cancellationToken);
    }
}
