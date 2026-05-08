using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Models.Common.Dtos;
using Querify.Models.Common.Enums;
using Querify.Models.QnA.Dtos.Source;
using Querify.QnA.Common.Persistence.QnADb.DbContext;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Querify.QnA.Portal.Business.Source.Queries.GetSourceList;

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

        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
        var query = dbContext.Sources
            .Where(source => source.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(request.Request.SearchText))
            query = query.Where(source =>
                EF.Functions.ILike(source.Locator, $"%{request.Request.SearchText}%") ||
                EF.Functions.ILike(source.Label ?? string.Empty, $"%{request.Request.SearchText}%") ||
                EF.Functions.ILike(source.ContextNote ?? string.Empty, $"%{request.Request.SearchText}%") ||
                EF.Functions.ILike(source.ExternalId ?? string.Empty, $"%{request.Request.SearchText}%") ||
                EF.Functions.ILike(source.Language, $"%{request.Request.SearchText}%") ||
                EF.Functions.ILike(source.MediaType ?? string.Empty, $"%{request.Request.SearchText}%"));

        if (request.Request.Visibility is not null)
            query = query.Where(source => source.Visibility == request.Request.Visibility);

        query = request.Request.Sorting?.Trim().ToLowerInvariant() switch
        {
            "lastupdatedatutc" or "lastupdatedatutc desc" or "updateddate" or "updateddate desc" =>
                query.OrderByDescending(source => source.UpdatedDate ?? source.CreatedDate),
            "lastupdatedatutc asc" or "updateddate asc" => query.OrderBy(source =>
                source.UpdatedDate ?? source.CreatedDate),
            "label" or "label asc" => query.OrderBy(source => source.Label).ThenBy(source => source.Locator),
            "label desc" => query.OrderByDescending(source => source.Label),
            "locator" or "locator asc" => query.OrderBy(source => source.Locator),
            "locator desc" => query.OrderByDescending(source => source.Locator),
            "lastverifiedatutc" or "lastverifiedatutc asc" =>
                query.OrderBy(source => source.LastVerifiedAtUtc),
            "lastverifiedatutc desc" => query.OrderByDescending(source => source.LastVerifiedAtUtc),
            "spaceusagecount" or "spaceusagecount asc" => query.OrderBy(source => source.Spaces.Count),
            "spaceusagecount desc" => query.OrderByDescending(source => source.Spaces.Count),
            "questionusagecount" or "questionusagecount asc" => query.OrderBy(source => source.Questions.Count),
            "questionusagecount desc" => query.OrderByDescending(source => source.Questions.Count),
            "answerusagecount" or "answerusagecount asc" => query.OrderBy(source => source.Answers.Count),
            "answerusagecount desc" => query.OrderByDescending(source => source.Answers.Count),
            "linkedrecordcount" or "linkedrecordcount asc" => query.OrderBy(source =>
                source.Spaces.Count + source.Questions.Count + source.Answers.Count),
            "linkedrecordcount desc" => query.OrderByDescending(source =>
                source.Spaces.Count + source.Questions.Count + source.Answers.Count),
            _ => query.OrderByDescending(source => source.UpdatedDate ?? source.CreatedDate)
                .ThenBy(source => source.Label)
        };

        return GetPagedResultAsync(query.AsNoTracking(), request.Request, cancellationToken);
    }

    private static async Task<PagedResultDto<SourceDto>> GetPagedResultAsync(
        IQueryable<Common.Domain.Entities.Source> query,
        SourceGetAllRequestDto request,
        CancellationToken cancellationToken)
    {
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip(request.SkipCount)
            .Take(request.MaxResultCount)
            .Select(entity => new SourceDto
            {
                Id = entity.Id,
                TenantId = entity.TenantId,
                Locator = entity.Locator,
                StorageKey = entity.StorageKey,
                Label = entity.Label,
                ContextNote = entity.ContextNote,
                ExternalId = entity.ExternalId,
                Language = entity.Language,
                MediaType = entity.MediaType,
                SizeBytes = entity.SizeBytes,
                Checksum = entity.Checksum,
                MetadataJson = entity.MetadataJson,
                UploadStatus = entity.UploadStatus,
                Visibility = entity.Visibility,
                LastVerifiedAtUtc = entity.LastVerifiedAtUtc,
                LastUpdatedAtUtc = entity.UpdatedDate ?? entity.CreatedDate,
                SpaceUsageCount = entity.Spaces.Count,
                QuestionUsageCount = entity.Questions.Count,
                AnswerUsageCount = entity.Answers.Count
            })
            .ToListAsync(cancellationToken);

        return new PagedResultDto<SourceDto>(
            totalCount,
            items);
    }
}
