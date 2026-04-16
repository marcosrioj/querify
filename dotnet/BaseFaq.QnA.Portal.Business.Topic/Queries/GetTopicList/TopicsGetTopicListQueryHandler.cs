using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.Topic;
using BaseFaq.QnA.Common.Persistence.QnADb;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.Topic.Queries;

public sealed class TopicsGetTopicListQueryHandler(QnADbContext dbContext, ISessionService sessionService)
    : IRequestHandler<TopicsGetTopicListQuery, PagedResultDto<TopicDto>>
{
    public Task<PagedResultDto<TopicDto>> Handle(TopicsGetTopicListQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Request);

        var tenantId = sessionService.GetTenantId(AppEnum.QnA);
        IQueryable<Common.Persistence.QnADb.Entities.Topic> query = dbContext.Topics.Where(topic => topic.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(request.Request.SearchText))
        {
            query = query.Where(topic =>
                EF.Functions.ILike(topic.Name, $"%{request.Request.SearchText}%") ||
                EF.Functions.ILike(topic.Category ?? string.Empty, $"%{request.Request.SearchText}%"));
        }

        if (!string.IsNullOrWhiteSpace(request.Request.Category))
        {
            query = query.Where(topic => topic.Category == request.Request.Category);
        }

        query = request.Request.Sorting?.Trim().ToLowerInvariant() switch
        {
            "category" => query.OrderBy(topic => topic.Category).ThenBy(topic => topic.Name),
            "category desc" => query.OrderByDescending(topic => topic.Category).ThenBy(topic => topic.Name),
            "name desc" => query.OrderByDescending(topic => topic.Name),
            _ => query.OrderBy(topic => topic.Name)
        };

        return GetPagedResultAsync(query.AsNoTracking(), request.Request, cancellationToken);
    }

    private static async Task<PagedResultDto<TopicDto>> GetPagedResultAsync(
        IQueryable<Common.Persistence.QnADb.Entities.Topic> query,
        TopicGetAllRequestDto request,
        CancellationToken cancellationToken)
    {
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip(request.SkipCount)
            .Take(request.MaxResultCount)
            .ToListAsync(cancellationToken);

        return new PagedResultDto<TopicDto>(
            totalCount,
            items.Select(
                    topic => new TopicDto
                    {
                        Id = topic.Id,
                        TenantId = topic.TenantId,
                        Name = topic.Name,
                        Category = topic.Category,
                        Description = topic.Description
                    })
                .ToList());
    }
}
