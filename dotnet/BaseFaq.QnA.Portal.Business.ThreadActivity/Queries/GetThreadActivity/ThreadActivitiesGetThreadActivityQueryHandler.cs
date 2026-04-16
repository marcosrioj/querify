using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.ThreadActivity;
using BaseFaq.QnA.Common.Persistence.QnADb;
using BaseFaq.QnA.Common.Persistence.QnADb.Projections;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.ThreadActivity.Queries.GetThreadActivity;

public sealed class ThreadActivitiesGetThreadActivityQueryHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<ThreadActivitiesGetThreadActivityQuery, ThreadActivityDto>
{
    public async Task<ThreadActivityDto> Handle(ThreadActivitiesGetThreadActivityQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(AppEnum.QnA);
        var entity = await dbContext.ThreadActivities.AsNoTracking()
            .SingleOrDefaultAsync(activity => activity.TenantId == tenantId && activity.Id == request.Id,
                cancellationToken);

        return entity is null
            ? throw new ApiErrorException(
                $"Thread activity '{request.Id}' was not found.",
                (int)HttpStatusCode.NotFound)
            : entity.ToThreadActivityDto();
    }
}
