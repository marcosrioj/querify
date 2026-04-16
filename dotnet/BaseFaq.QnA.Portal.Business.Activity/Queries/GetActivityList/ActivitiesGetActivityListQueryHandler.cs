using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.Activity;
using BaseFaq.QnA.Common.Persistence.QnADb;
using BaseFaq.QnA.Common.Persistence.QnADb.Projections;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.Activity.Queries.GetActivityList;

public sealed class ActivitiesGetActivityListQueryHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<ActivitiesGetActivityListQuery, PagedResultDto<ActivityDto>>
{
    public Task<PagedResultDto<ActivityDto>> Handle(
        ActivitiesGetActivityListQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Request);

        var tenantId = sessionService.GetTenantId(AppEnum.QnA);
        var query = dbContext.Activities
            .Where(activity => activity.TenantId == tenantId);

        if (request.Request.QuestionId is not null)
            query = query.Where(activity => activity.QuestionId == request.Request.QuestionId);

        if (request.Request.AnswerId is not null)
            query = query.Where(activity => activity.AnswerId == request.Request.AnswerId);

        if (request.Request.Kind is not null) query = query.Where(activity => activity.Kind == request.Request.Kind);

        if (request.Request.ActorKind is not null)
            query = query.Where(activity => activity.ActorKind == request.Request.ActorKind);

        query = request.Request.Sorting?.Trim().ToLowerInvariant() switch
        {
            "occurredatutc" => query.OrderBy(activity => activity.OccurredAtUtc),
            _ => query.OrderByDescending(activity => activity.OccurredAtUtc)
        };

        return GetPagedResultAsync(query.AsNoTracking(), request.Request, cancellationToken);
    }

    private static async Task<PagedResultDto<ActivityDto>> GetPagedResultAsync(
        IQueryable<Common.Persistence.QnADb.Entities.Activity> query,
        ActivityGetAllRequestDto request,
        CancellationToken cancellationToken)
    {
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip(request.SkipCount)
            .Take(request.MaxResultCount)
            .ToListAsync(cancellationToken);

        return new PagedResultDto<ActivityDto>(
            totalCount,
            items.Select(activity => activity.ToActivityDto())
                .ToList());
    }
}
