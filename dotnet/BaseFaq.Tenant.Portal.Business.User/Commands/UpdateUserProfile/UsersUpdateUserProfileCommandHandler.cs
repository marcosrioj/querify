using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace BaseFaq.Tenant.Portal.Business.User.Commands.UpdateUserProfile;

public class UsersUpdateUserProfileCommandHandler(TenantDbContext dbContext, ISessionService sessionService)
    : IRequestHandler<UsersUpdateUserProfileCommand>
{
    public async Task Handle(UsersUpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        var userId = sessionService.GetUserId();

        var user = await dbContext.Users.FirstOrDefaultAsync(entity => entity.Id == userId, cancellationToken);
        if (user is null)
        {
            throw new ApiErrorException(
                $"User '{userId}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        user.GivenName = request.GivenName;
        user.SurName = request.SurName;
        user.PhoneNumber = request.PhoneNumber ?? string.Empty;
        user.Language = string.IsNullOrWhiteSpace(request.Language)
            ? null
            : request.Language.Trim();
        user.TimeZone = string.IsNullOrWhiteSpace(request.TimeZone)
            ? null
            : request.TimeZone.Trim();

        dbContext.Users.Update(user);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
