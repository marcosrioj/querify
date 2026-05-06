using Querify.Common.EntityFramework.Tenant;
using Querify.Models.User.Dtos.User;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Querify.Tenant.BackOffice.Business.User.Queries.GetUser;

public class UsersGetUserQueryHandler(TenantDbContext dbContext)
    : IRequestHandler<UsersGetUserQuery, UserDto?>
{
    public async Task<UserDto?> Handle(UsersGetUserQuery request, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);

        if (user is null)
        {
            return null;
        }

        return new UserDto
        {
            Id = user.Id,
            GivenName = user.GivenName,
            SurName = user.SurName,
            Email = user.Email,
            ExternalId = user.ExternalId,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role
        };
    }
}