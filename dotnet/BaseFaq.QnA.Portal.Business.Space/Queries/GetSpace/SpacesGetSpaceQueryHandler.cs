using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.Source;
using BaseFaq.Models.QnA.Dtos.Space;
using BaseFaq.Models.QnA.Dtos.Tag;
using BaseFaq.QnA.Common.Persistence.QnADb;
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
        var tenantId = sessionService.GetTenantId(AppEnum.QnA);
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
            Key = entity.Key,
            Summary = entity.Summary,
            DefaultLanguage = entity.DefaultLanguage,
            Kind = entity.Kind,
            Visibility = entity.Visibility,
            ModerationPolicy = entity.ModerationPolicy,
            SearchMarkupMode = entity.SearchMarkupMode,
            ProductScope = entity.ProductScope,
            JourneyScope = entity.JourneyScope,
            AcceptsQuestions = entity.AcceptsQuestions,
            AcceptsAnswers = entity.AcceptsAnswers,
            RequiresQuestionReview = entity.RequiresQuestionReview,
            RequiresAnswerReview = entity.RequiresAnswerReview,
            PublishedAtUtc = entity.PublishedAtUtc,
            LastValidatedAtUtc = entity.LastValidatedAtUtc,
            QuestionCount = entity.Questions.Count,
            Tags = entity.Tags
                .Select(link => link.Tag)
                .Select(tag => new TagDto
                {
                    Id = tag.Id,
                    TenantId = tag.TenantId,
                    Name = tag.Name
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
                    Scope = source.Scope,
                    SystemName = source.SystemName,
                    ExternalId = source.ExternalId,
                    Language = source.Language,
                    MediaType = source.MediaType,
                    Checksum = source.Checksum,
                    MetadataJson = source.MetadataJson,
                    Visibility = source.Visibility,
                    AllowsPublicCitation = source.AllowsPublicCitation,
                    AllowsPublicExcerpt = source.AllowsPublicExcerpt,
                    IsAuthoritative = source.IsAuthoritative,
                    CapturedAtUtc = source.CapturedAtUtc,
                    LastVerifiedAtUtc = source.LastVerifiedAtUtc
                })
                .ToList()
        };
    }
}