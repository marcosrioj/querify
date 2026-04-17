using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.Source;
using BaseFaq.QnA.Common.Persistence.QnADb;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.Source.Queries.GetSourceList;

public sealed class SourcesGetSourceListQueryHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<SourcesGetSourceListQuery, PagedResultDto<SourceDto>>
{
    public Task<PagedResultDto<SourceDto>> Handle(
        SourcesGetSourceListQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Request);

        var tenantId = sessionService.GetTenantId(AppEnum.QnA);
        var query = dbContext.Sources
            .Where(source => source.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(request.Request.SearchText))
            query = query.Where(source =>
                EF.Functions.ILike(source.Locator, $"%{request.Request.SearchText}%") ||
                EF.Functions.ILike(source.Label ?? string.Empty, $"%{request.Request.SearchText}%"));

        if (request.Request.Kind is not null) query = query.Where(source => source.Kind == request.Request.Kind);

        if (request.Request.Visibility is not null)
            query = query.Where(source => source.Visibility == request.Request.Visibility);

        if (request.Request.IsAuthoritative is not null)
            query = query.Where(source => source.IsAuthoritative == request.Request.IsAuthoritative);

        if (!string.IsNullOrWhiteSpace(request.Request.SystemName))
            query = query.Where(source => source.SystemName == request.Request.SystemName);

        query = request.Request.Sorting?.Trim().ToLowerInvariant() switch
        {
            "kind" => query.OrderBy(source => source.Kind).ThenBy(source => source.Label),
            "kind desc" => query.OrderByDescending(source => source.Kind).ThenBy(source => source.Label),
            "label desc" => query.OrderByDescending(source => source.Label),
            _ => query.OrderBy(source => source.Label).ThenBy(source => source.Locator)
        };

        return GetPagedResultAsync(query.AsNoTracking(), request.Request, cancellationToken);
    }

    private static async Task<PagedResultDto<SourceDto>> GetPagedResultAsync(
        IQueryable<Common.Persistence.QnADb.Entities.Source> query,
        SourceGetAllRequestDto request,
        CancellationToken cancellationToken)
    {
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip(request.SkipCount)
            .Take(request.MaxResultCount)
            .ToListAsync(cancellationToken);

        return new PagedResultDto<SourceDto>(
            totalCount,
            items.Select(entity => new SourceDto
                {
                    Id = entity.Id,
                    TenantId = entity.TenantId,
                    Kind = entity.Kind,
                    Locator = entity.Locator,
                    Label = entity.Label,
                    Scope = entity.Scope,
                    SystemName = entity.SystemName,
                    ExternalId = entity.ExternalId,
                    Language = entity.Language,
                    MediaType = entity.MediaType,
                    Checksum = entity.Checksum,
                    MetadataJson = entity.MetadataJson,
                    Visibility = entity.Visibility,
                    AllowsPublicCitation = entity.AllowsPublicCitation,
                    AllowsPublicExcerpt = entity.AllowsPublicExcerpt,
                    IsAuthoritative = entity.IsAuthoritative,
                    CapturedAtUtc = entity.CapturedAtUtc,
                    LastVerifiedAtUtc = entity.LastVerifiedAtUtc
                })
                .ToList());
    }
}