using System.Net;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Models.Common.Enums;
using Querify.Models.QnA.Dtos.Source;
using Querify.Models.QnA.Dtos.Space;
using Querify.Models.QnA.Dtos.Tag;
using Querify.QnA.Common.Persistence.QnADb.DbContext;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Querify.QnA.Portal.Business.Space.Queries.GetSpace;

public sealed class SpacesGetSpaceQueryHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<SpacesGetSpaceQuery, SpaceDetailDto>
{
    public async Task<SpaceDetailDto> Handle(SpacesGetSpaceQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
        var entity = await dbContext.Spaces.AsNoTracking()
            .Where(space => space.TenantId == tenantId && space.Id == request.Id)
            .Select(space => new SpaceDetailDto
            {
                Id = space.Id,
                TenantId = space.TenantId,
                Name = space.Name,
                Slug = space.Slug,
                Summary = space.Summary,
                Language = space.Language,
                Status = space.Status,
                Visibility = space.Visibility,
                AcceptsQuestions = space.AcceptsQuestions,
                AcceptsAnswers = space.AcceptsAnswers,
                QuestionCount = space.Questions.Count,
                LastUpdatedAtUtc = space.UpdatedDate ?? space.CreatedDate,
                Tags = space.Tags
                    .OrderBy(link => link.Tag.Name)
                    .Select(link => new TagDto
                    {
                        Id = link.Tag.Id,
                        TenantId = link.Tag.TenantId,
                        Name = link.Tag.Name,
                        SpaceUsageCount = link.Tag.Spaces.Count,
                        QuestionUsageCount = link.Tag.Questions.Count,
                        LastUpdatedAtUtc = link.Tag.UpdatedDate ?? link.Tag.CreatedDate
                    })
                    .ToList(),
                CuratedSources = space.Sources
                    .OrderBy(link => link.Source.Label ?? link.Source.Locator)
                    .Select(link => new SourceDto
                    {
                        Id = link.Source.Id,
                        TenantId = link.Source.TenantId,
                        Locator = link.Source.Locator,
                        StorageKey = link.Source.StorageKey,
                        Label = link.Source.Label,
                        ContextNote = link.Source.ContextNote,
                        ExternalId = link.Source.ExternalId,
                        Language = link.Source.Language,
                        MediaType = link.Source.MediaType,
                        SizeBytes = link.Source.SizeBytes,
                        Checksum = link.Source.Checksum,
                        MetadataJson = link.Source.MetadataJson,
                        UploadStatus = link.Source.UploadStatus,
                        LastUpdatedAtUtc = link.Source.UpdatedDate ?? link.Source.CreatedDate,
                        SpaceUsageCount = link.Source.Spaces.Count,
                        QuestionUsageCount = link.Source.Questions.Count,
                        AnswerUsageCount = link.Source.Answers.Count
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (entity is null)
            throw new ApiErrorException($"Space '{request.Id}' was not found.", (int)HttpStatusCode.NotFound);

        return entity;
    }
}
