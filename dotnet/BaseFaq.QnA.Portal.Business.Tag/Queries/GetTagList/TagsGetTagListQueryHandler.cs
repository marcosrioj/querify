using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.Tag;
using BaseFaq.QnA.Common.Persistence.QnADb;
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

        var tenantId = sessionService.GetTenantId(AppEnum.QnA);
        var query = dbContext.Tags.Where(tag => tag.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(request.Request.SearchText))
            query = query.Where(tag => EF.Functions.ILike(tag.Name, $"%{request.Request.SearchText}%"));

        query = request.Request.Sorting?.Trim().ToLowerInvariant() switch
        {
            "name desc" => query.OrderByDescending(tag => tag.Name),
            _ => query.OrderBy(tag => tag.Name)
        };

        return GetPagedResultAsync(query.AsNoTracking(), request.Request, cancellationToken);
    }

    private static async Task<PagedResultDto<TagDto>> GetPagedResultAsync(
        IQueryable<Common.Persistence.QnADb.Entities.Tag> query,
        TagGetAllRequestDto request,
        CancellationToken cancellationToken)
    {
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip(request.SkipCount)
            .Take(request.MaxResultCount)
            .ToListAsync(cancellationToken);

        return new PagedResultDto<TagDto>(
            totalCount,
            items.Select(tag => new TagDto
                {
                    Id = tag.Id,
                    TenantId = tag.TenantId,
                    Name = tag.Name
                })
                .ToList());
    }
}