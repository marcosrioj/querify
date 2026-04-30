using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.Tag;
using BaseFaq.QnA.Common.Persistence.QnADb.DbContext;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.Tag.Queries.GetTagList;

public sealed class TagsGetTagListQueryHandler(QnADbContext dbContext, ISessionService sessionService)
    : IRequestHandler<TagsGetTagListQuery, PagedResultDto<TagDto>>
{
    public Task<PagedResultDto<TagDto>> Handle(TagsGetTagListQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Request);

        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
        var query = dbContext.Tags.Where(tag => tag.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(request.Request.SearchText))
            query = query.Where(tag => EF.Functions.ILike(tag.Name, $"%{request.Request.SearchText}%"));

        query = request.Request.Sorting?.Trim().ToLowerInvariant() switch
        {
            "lastupdatedatutc" or "lastupdatedatutc desc" or "updateddate" or "updateddate desc" =>
                query.OrderByDescending(tag => tag.UpdatedDate ?? tag.CreatedDate),
            "lastupdatedatutc asc" or "updateddate asc" => query.OrderBy(tag => tag.UpdatedDate ?? tag.CreatedDate),
            "name" or "name asc" => query.OrderBy(tag => tag.Name),
            "name desc" => query.OrderByDescending(tag => tag.Name),
            "spaceusagecount" or "spaceusagecount asc" => query.OrderBy(tag => tag.Spaces.Count),
            "spaceusagecount desc" => query.OrderByDescending(tag => tag.Spaces.Count),
            "questionusagecount" or "questionusagecount asc" => query.OrderBy(tag => tag.Questions.Count),
            "questionusagecount desc" => query.OrderByDescending(tag => tag.Questions.Count),
            "linkedrecordcount" or "linkedrecordcount asc" => query.OrderBy(tag =>
                tag.Spaces.Count + tag.Questions.Count),
            "linkedrecordcount desc" => query.OrderByDescending(tag => tag.Spaces.Count + tag.Questions.Count),
            _ => query.OrderByDescending(tag => tag.UpdatedDate ?? tag.CreatedDate).ThenBy(tag => tag.Name)
        };

        return GetPagedResultAsync(query.AsNoTracking(), request.Request, cancellationToken);
    }

    private static async Task<PagedResultDto<TagDto>> GetPagedResultAsync(
        IQueryable<Common.Domain.Entities.Tag> query,
        TagGetAllRequestDto request,
        CancellationToken cancellationToken)
    {
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip(request.SkipCount)
            .Take(request.MaxResultCount)
            .Select(tag => new TagDto
            {
                Id = tag.Id,
                TenantId = tag.TenantId,
                Name = tag.Name,
                SpaceUsageCount = tag.Spaces.Count,
                QuestionUsageCount = tag.Questions.Count,
                LastUpdatedAtUtc = tag.UpdatedDate ?? tag.CreatedDate
            })
            .ToListAsync(cancellationToken);

        return new PagedResultDto<TagDto>(
            totalCount,
            items);
    }
}
