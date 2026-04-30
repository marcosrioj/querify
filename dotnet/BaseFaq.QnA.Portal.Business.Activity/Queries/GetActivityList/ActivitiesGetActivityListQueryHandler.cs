using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.Activity;
using BaseFaq.QnA.Common.Persistence.QnADb.DbContext;
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

        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
        var query = dbContext.Activities
            .Where(activity => activity.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(request.Request.SearchText))
            query = query.Where(activity =>
                EF.Functions.ILike(activity.ActorLabel ?? string.Empty, $"%{request.Request.SearchText}%") ||
                EF.Functions.ILike(activity.UserPrint, $"%{request.Request.SearchText}%") ||
                EF.Functions.ILike(activity.Notes ?? string.Empty, $"%{request.Request.SearchText}%") ||
                EF.Functions.ILike(activity.MetadataJson ?? string.Empty, $"%{request.Request.SearchText}%") ||
                EF.Functions.ILike(activity.Question.Title, $"%{request.Request.SearchText}%") ||
                (activity.Answer != null &&
                 EF.Functions.ILike(activity.Answer.Headline, $"%{request.Request.SearchText}%")));

        if (request.Request.SpaceId is not null)
            query = query.Where(activity => activity.Question.SpaceId == request.Request.SpaceId.Value);

        if (request.Request.QuestionId is not null)
            query = query.Where(activity => activity.QuestionId == request.Request.QuestionId);

        if (request.Request.AnswerId is not null)
            query = query.Where(activity => activity.AnswerId == request.Request.AnswerId);

        if (request.Request.Kind is not null) query = query.Where(activity => activity.Kind == request.Request.Kind);

        if (request.Request.ActorKind is not null)
            query = query.Where(activity => activity.ActorKind == request.Request.ActorKind);

        query = request.Request.Sorting?.Trim().ToLowerInvariant() switch
        {
            "occurredatutc" or "occurredatutc asc" => query.OrderBy(activity => activity.OccurredAtUtc),
            "occurredatutc desc" => query.OrderByDescending(activity => activity.OccurredAtUtc),
            "kind" or "kind asc" => query.OrderBy(activity => activity.Kind),
            "kind desc" => query.OrderByDescending(activity => activity.Kind),
            "actorkind" or "actorkind asc" => query.OrderBy(activity => activity.ActorKind),
            "actorkind desc" => query.OrderByDescending(activity => activity.ActorKind),
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
            .Select(activity => new ActivityDto
            {
                Id = activity.Id,
                TenantId = activity.TenantId,
                QuestionId = activity.QuestionId,
                AnswerId = activity.AnswerId,
                Kind = activity.Kind,
                ActorKind = activity.ActorKind,
                ActorLabel = activity.ActorLabel,
                UserPrint = activity.UserPrint,
                Ip = activity.Ip,
                UserAgent = activity.UserAgent,
                Notes = activity.Notes,
                MetadataJson = activity.MetadataJson,
                OccurredAtUtc = activity.OccurredAtUtc
            })
            .ToListAsync(cancellationToken);

        return new PagedResultDto<ActivityDto>(
            totalCount,
            items);
    }
}
