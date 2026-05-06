using Querify.Common.EntityFramework.Tenant;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Querify.Tenant.BackOffice.Business.User.Commands.UpdateUser;

public class UsersUpdateUserCommandHandler(TenantDbContext dbContext)
    : IRequestHandler<UsersUpdateUserCommand>
{
    public async Task Handle(UsersUpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);
        if (user is null)
        {
            throw new ApiErrorException(
                $"User '{request.Id}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        user.GivenName = request.GivenName;
        user.SurName = request.SurName;
        user.Email = request.Email;
        user.ExternalId = request.ExternalId;
        user.PhoneNumber = request.PhoneNumber ?? string.Empty;
        user.Role = request.Role;

        dbContext.Users.Update(user);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}