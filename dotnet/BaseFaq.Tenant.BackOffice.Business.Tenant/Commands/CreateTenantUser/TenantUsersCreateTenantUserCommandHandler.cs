using System.Net;
using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Common.EntityFramework.Tenant.Entities;
using BaseFaq.Common.EntityFramework.Tenant.Helpers;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Common.Infrastructure.Core.Helpers;
using BaseFaq.Models.Tenant.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Tenant.BackOffice.Business.Tenant.Commands.CreateTenantUser;

public class TenantUsersCreateTenantUserCommandHandler(
    TenantDbContext dbContext,
    IAllowedTenantStore allowedTenantStore)
    : IRequestHandler<TenantUsersCreateTenantUserCommand, Guid>
{
    public async Task<Guid> Handle(TenantUsersCreateTenantUserCommand request, CancellationToken cancellationToken)
    {
        var user = await ResolveUserByEmailAsync(request.Email, cancellationToken);
        var tenantExists = await dbContext.Tenants
            .AnyAsync(entity => entity.Id == request.TenantId, cancellationToken);

        if (!tenantExists)
        {
            throw new ApiErrorException(
                $"Tenant '{request.TenantId}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        var tenantUserExists = await dbContext.TenantUsers
            .AnyAsync(
                entity => entity.TenantId == request.TenantId && entity.UserId == user.Id,
                cancellationToken);

        if (tenantUserExists)
        {
            throw new ApiErrorException(
                $"User '{user.Email}' already belongs to tenant '{request.TenantId}'.",
                errorCode: (int)HttpStatusCode.Conflict);
        }

        var previousOwnerUserId = await dbContext.TenantUsers
            .AsNoTracking()
            .Where(entity => entity.TenantId == request.TenantId)
            .Select(entity => entity.Role == TenantUserRoleType.Owner ? (Guid?)entity.UserId : null)
            .FirstOrDefaultAsync(entity => entity.HasValue, cancellationToken);

        var tenantUser = await CreateTenantUserAsync(request, user.Id, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        await AllowedTenantCacheHelper.RemoveUserEntries(
            allowedTenantStore,
            [user.Id, previousOwnerUserId ?? Guid.Empty],
            cancellationToken);

        return tenantUser.Id;
    }

    private async Task<TenantUser> CreateTenantUserAsync(
        TenantUsersCreateTenantUserCommand request,
        Guid userId,
        CancellationToken cancellationToken)
    {
        if (request.Role != TenantUserRoleType.Owner)
        {
            var tenantUser = new TenantUser
            {
                Id = Guid.NewGuid(),
                TenantId = request.TenantId,
                UserId = userId,
                Role = request.Role
            };

            await dbContext.TenantUsers.AddAsync(tenantUser, cancellationToken);
            return tenantUser;
        }

        var tenantUsers = await dbContext.TenantUsers
            .Where(entity => entity.TenantId == request.TenantId)
            .ToListAsync(cancellationToken);

        foreach (var tenantUser in tenantUsers)
        {
            tenantUser.Role = tenantUser.UserId == userId
                ? TenantUserRoleType.Owner
                : TenantUserRoleType.Member;
        }

        var ownerTenantUser = tenantUsers.FirstOrDefault(entity => entity.UserId == userId);
        if (ownerTenantUser is not null)
        {
            return ownerTenantUser;
        }

        ownerTenantUser = new TenantUser
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            UserId = userId,
            Role = TenantUserRoleType.Owner
        };

        await dbContext.TenantUsers.AddAsync(ownerTenantUser, cancellationToken);
        return ownerTenantUser;
    }

    private async Task<Common.EntityFramework.Tenant.Entities.User> ResolveUserByEmailAsync(string email, CancellationToken cancellationToken)
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
