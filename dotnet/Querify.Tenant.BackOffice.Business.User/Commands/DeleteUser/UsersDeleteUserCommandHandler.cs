using Querify.Common.EntityFramework.Tenant;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Querify.Tenant.BackOffice.Business.User.Commands.DeleteUser;

public class UsersDeleteUserCommandHandler(TenantDbContext dbContext)
    : IRequestHandler<UsersDeleteUserCommand>
{
    public async Task Handle(UsersDeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);
        if (user is null)
        {
            throw new ApiErrorException(
                $"User '{request.Id}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        dbContext.Users.Remove(user);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}