using Querify.Common.EntityFramework.Tenant;
using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Models.User.Dtos.User;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Querify.Tenant.Portal.Business.User.Queries.GetUserProfile;

public class UsersGetUserProfileQueryHandler(TenantDbContext dbContext, ISessionService sessionService)
    : IRequestHandler<UsersGetUserProfileQuery, UserProfileDto?>
{
    public async Task<UserProfileDto?> Handle(UsersGetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var userId = sessionService.GetUserId();

        return await dbContext.Users
            .AsNoTracking()
            .Where(entity => entity.Id == userId)
            .Select(user => new UserProfileDto
            {
                GivenName = user.GivenName,
                SurName = user.SurName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Language = user.Language,
                TimeZone = user.TimeZone
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
