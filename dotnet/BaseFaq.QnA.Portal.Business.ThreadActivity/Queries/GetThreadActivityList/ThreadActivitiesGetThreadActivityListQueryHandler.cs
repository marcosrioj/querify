using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.ThreadActivity;
using BaseFaq.QnA.Common.Persistence.QnADb;
using BaseFaq.QnA.Common.Persistence.QnADb.Projections;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.ThreadActivity.Queries.GetThreadActivityList;

public sealed class ThreadActivitiesGetThreadActivityListQueryHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<ThreadActivitiesGetThreadActivityListQuery, PagedResultDto<ThreadActivityDto>>
{
    public Task<PagedResultDto<ThreadActivityDto>> Handle(
        ThreadActivitiesGetThreadActivityListQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Request);

        var tenantId = sessionService.GetTenantId(AppEnum.QnA);
        var query = dbContext.ThreadActivities
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

    private static async Task<PagedResultDto<ThreadActivityDto>> GetPagedResultAsync(
        IQueryable<Common.Persistence.QnADb.Entities.ThreadActivity> query,
        ThreadActivityGetAllRequestDto request,
        CancellationToken cancellationToken)
    {
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip(request.SkipCount)
            .Take(request.MaxResultCount)
            .ToListAsync(cancellationToken);

        return new PagedResultDto<ThreadActivityDto>(
            totalCount,
            items.Select(activity => activity.ToThreadActivityDto())
                .ToList());
    }
}
