using Querify.Common.EntityFramework.Tenant;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Common.Infrastructure.Core.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Querify.Tenant.Portal.Business.User.Commands.UpdateUserProfile;

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
        user.Language = NormalizeOptionalText(request.Language);
        user.TimeZone = NormalizeTimeZone(request.TimeZone);

        dbContext.Users.Update(user);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string? NormalizeTimeZone(string? timeZone)
    {
        var normalizedTimeZone = NormalizeOptionalText(timeZone);
        if (normalizedTimeZone is null)
        {
            return null;
        }

        if (IsSupportedTimeZone(normalizedTimeZone))
        {
            return normalizedTimeZone;
        }

        throw new ApiErrorException("Time zone is invalid.", errorCode: (int)HttpStatusCode.UnprocessableEntity);
    }

    private static bool IsSupportedTimeZone(string timeZone)
    {
        try
        {
            _ = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            return true;
        }
        catch (TimeZoneNotFoundException)
        {
            return false;
        }
        catch (InvalidTimeZoneException)
        {
            return false;
        }
    }
}
