using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.User.Dtos.User;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Tenant.Portal.Business.User.Queries.GetUserProfile;

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
                TimeZone = user.TimeZone
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
