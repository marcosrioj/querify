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

namespace BaseFaq.Tenant.Portal.Business.Tenant.Commands.CreateTenantUser;

public class TenantUsersCreateTenantUserCommandHandler(
    TenantDbContext dbContext,
    IAllowedTenantStore allowedTenantStore,
    ITenantPortalAccessService tenantPortalAccessService)
    : IRequestHandler<TenantUsersCreateTenantUserCommand, Guid>
{
    public async Task<Guid> Handle(TenantUsersCreateTenantUserCommand request, CancellationToken cancellationToken)
    {
        var tenantId = request.TenantId;
        await tenantPortalAccessService.EnsureOwnerAccessAsync(tenantId, cancellationToken);

        if (request.Role != TenantUserRoleType.Member)
        {
            throw new ApiErrorException(
                "Portal users can only add members to the workspace.",
                errorCode: (int)HttpStatusCode.BadRequest);
        }

        var user = await ResolveUserByEmailAsync(request.Email, cancellationToken);

        var tenantUserExists = await dbContext.TenantUsers
            .AnyAsync(
                entity => entity.TenantId == tenantId && entity.UserId == user.Id,
                cancellationToken);

        if (tenantUserExists)
        {
            throw new ApiErrorException(
                $"User '{user.Email}' already belongs to tenant '{tenantId}'.",
                errorCode: (int)HttpStatusCode.Conflict);
        }

        var tenantUser = await CreateTenantUserAsync(tenantId, request.Role, user.Id, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        await AllowedTenantCacheHelper.RemoveUserEntries(allowedTenantStore, [user.Id], cancellationToken);

        return tenantUser.Id;
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

    private async Task<User> ResolveUserByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await dbContext.Users
            .FirstOrDefaultAsync(entity => entity.Email.ToLower() == normalizedEmail, cancellationToken);

        if (user is not null)
        {
            return user;
        }

        throw new ApiErrorException(
            $"User with email '{email}' was not found.",
            errorCode: (int)HttpStatusCode.NotFound);
    }
}
