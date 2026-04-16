using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.Activity;
using BaseFaq.QnA.Common.Persistence.QnADb;
using BaseFaq.QnA.Common.Persistence.QnADb.Projections;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.Activity.Queries.GetActivity;

public sealed class ActivitiesGetActivityQueryHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<ActivitiesGetActivityQuery, ActivityDto>
{
    public async Task<ActivityDto> Handle(ActivitiesGetActivityQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(AppEnum.QnA);
        var entity = await dbContext.Activities.AsNoTracking()
            .SingleOrDefaultAsync(activity => activity.TenantId == tenantId && activity.Id == request.Id,
                cancellationToken);

        return entity is null
            ? throw new ApiErrorException(
                $"Activity '{request.Id}' was not found.",
                (int)HttpStatusCode.NotFound)
            : entity.ToActivityDto();
    }
}
