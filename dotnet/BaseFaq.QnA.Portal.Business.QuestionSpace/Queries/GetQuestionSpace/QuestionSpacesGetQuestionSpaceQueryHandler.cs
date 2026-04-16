using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.KnowledgeSource;
using BaseFaq.Models.QnA.Dtos.QuestionSpace;
using BaseFaq.Models.QnA.Dtos.Topic;
using BaseFaq.QnA.Common.Persistence.QnADb;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.QuestionSpace.Queries.GetQuestionSpace;

public sealed class QuestionSpacesGetQuestionSpaceQueryHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<QuestionSpacesGetQuestionSpaceQuery, QuestionSpaceDetailDto>
{
    public async Task<QuestionSpaceDetailDto> Handle(QuestionSpacesGetQuestionSpaceQuery request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(AppEnum.QnA);
        var entity = await dbContext.QuestionSpaces
            .Include(space => space.Questions)
            .Include(space => space.QuestionSpaceTopics)
            .ThenInclude(link => link.Topic)
            .Include(space => space.QuestionSpaceSources)
            .ThenInclude(link => link.KnowledgeSource)
            .AsNoTracking()
            .SingleOrDefaultAsync(space => space.TenantId == tenantId && space.Id == request.Id, cancellationToken);

        if (entity is null)
        {
            throw new ApiErrorException($"Question space '{request.Id}' was not found.", errorCode: (int)HttpStatusCode.NotFound);
        }

        return new QuestionSpaceDetailDto
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
            Topics = entity.QuestionSpaceTopics
                .Select(link => link.Topic)
                .Select(topic => new TopicDto
                {
                    Id = topic.Id,
                    TenantId = topic.TenantId,
                    Name = topic.Name,
                    Category = topic.Category,
                    Description = topic.Description
                })
                .ToList(),
            CuratedSources = entity.QuestionSpaceSources
                .Select(link => link.KnowledgeSource)
                .Select(source => new KnowledgeSourceDto
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
