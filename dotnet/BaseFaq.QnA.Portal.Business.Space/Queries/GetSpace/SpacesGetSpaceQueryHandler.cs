using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.Source;
using BaseFaq.Models.QnA.Dtos.Space;
using BaseFaq.Models.QnA.Dtos.Tag;
using BaseFaq.QnA.Common.Persistence.QnADb.DbContext;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.Space.Queries.GetSpace;

public sealed class SpacesGetSpaceQueryHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<SpacesGetSpaceQuery, SpaceDetailDto>
{
    public async Task<SpaceDetailDto> Handle(SpacesGetSpaceQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
        var entity = await dbContext.Spaces
            .Include(space => space.Questions)
            .Include(space => space.Tags)
            .ThenInclude(link => link.Tag)
            .Include(space => space.Sources)
            .ThenInclude(link => link.Source)
            .AsNoTracking()
            .SingleOrDefaultAsync(space => space.TenantId == tenantId && space.Id == request.Id, cancellationToken);

        if (entity is null)
            throw new ApiErrorException($"Space '{request.Id}' was not found.", (int)HttpStatusCode.NotFound);

        return new SpaceDetailDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            Name = entity.Name,
            Slug = entity.Slug,
            Summary = entity.Summary,
            Language = entity.Language,
            Status = entity.Status,
            Visibility = entity.Visibility,
            AcceptsQuestions = entity.AcceptsQuestions,
            AcceptsAnswers = entity.AcceptsAnswers,
            QuestionCount = entity.Questions.Count,
            LastUpdatedAtUtc = entity.UpdatedDate ?? entity.CreatedDate,
            Tags = entity.Tags
                .Select(link => link.Tag)
                .Select(tag => new TagDto
                {
                    Id = tag.Id,
                    TenantId = tag.TenantId,
                    Name = tag.Name,
                    SpaceUsageCount = tag.Spaces.Count,
                    QuestionUsageCount = tag.Questions.Count,
                    LastUpdatedAtUtc = tag.UpdatedDate ?? tag.CreatedDate
                })
                .ToList(),
            CuratedSources = entity.Sources
                .Select(link => link.Source)
                .Select(source => new SourceDto
                {
                    Id = source.Id,
                    TenantId = source.TenantId,
                    Kind = source.Kind,
                    Locator = source.Locator,
                    Label = source.Label,
                    ContextNote = source.ContextNote,
                    ExternalId = source.ExternalId,
                    Language = source.Language,
                    MediaType = source.MediaType,
                    Checksum = source.Checksum,
                    MetadataJson = source.MetadataJson,
                    Visibility = source.Visibility,
                    LastVerifiedAtUtc = source.LastVerifiedAtUtc,
                    LastUpdatedAtUtc = source.UpdatedDate ?? source.CreatedDate,
                    SpaceUsageCount = source.Spaces.Count,
                    QuestionUsageCount = source.Questions.Count,
                    AnswerUsageCount = source.Answers.Count
                })
                .ToList()
        };
    }
}
