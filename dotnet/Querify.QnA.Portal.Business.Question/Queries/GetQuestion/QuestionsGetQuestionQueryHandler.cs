using System.Net;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Models.Common.Enums;
using Querify.Models.QnA.Dtos.Answer;
using Querify.Models.QnA.Dtos.Question;
using Querify.Models.QnA.Dtos.Source;
using Querify.Models.QnA.Dtos.Tag;
using Querify.Models.QnA.Enums;
using Querify.QnA.Common.Domain.BusinessRules.Activities;
using Querify.QnA.Common.Persistence.QnADb.DbContext;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Querify.QnA.Portal.Business.Question.Queries.GetQuestion;

public sealed class QuestionsGetQuestionQueryHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<QuestionsGetQuestionQuery, QuestionDetailDto>
{
    public async Task<QuestionDetailDto> Handle(
        QuestionsGetQuestionQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
        var entity = await dbContext.Questions
            .AsNoTracking()
            .Where(question => question.TenantId == tenantId && question.Id == request.Id)
            .Select(question => new QuestionDetailDto
            {
                Id = question.Id,
                TenantId = question.TenantId,
                SpaceId = question.SpaceId,
                SpaceSlug = question.Space.Slug,
                Title = question.Title,
                Summary = question.Summary,
                ContextNote = question.ContextNote,
                Status = question.Status,
                Visibility = question.Visibility,
                OriginChannel = question.OriginChannel,
                AiConfidenceScore = question.AiConfidenceScore,
                FeedbackScore = question.FeedbackScore,
                Sort = question.Sort,
                AcceptedAnswerId = question.AcceptedAnswerId,
                LastActivityAtUtc = question.LastActivityAtUtc,
                LastUpdatedAtUtc = question.UpdatedDate ?? question.CreatedDate
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (entity is null)
            throw new ApiErrorException(
                $"Question '{request.Id}' was not found.",
                (int)HttpStatusCode.NotFound);

        entity.AcceptedAnswer = await GetAcceptedAnswerAsync(
            tenantId,
            entity.AcceptedAnswerId,
            cancellationToken);
        entity.Tags = await GetTagsAsync(tenantId, request.Id, cancellationToken);
        entity.Sources = await GetSourcesAsync(tenantId, request.Id, cancellationToken);

        await PopulateAcceptedAnswerVoteScoreAsync(tenantId, entity, cancellationToken);
        return entity;
    }

    private Task<AnswerDto?> GetAcceptedAnswerAsync(
        Guid tenantId,
        Guid? acceptedAnswerId,
        CancellationToken cancellationToken)
    {
        if (acceptedAnswerId is null)
            return Task.FromResult<AnswerDto?>(null);

        return dbContext.Answers
            .AsNoTracking()
            .Where(answer => answer.TenantId == tenantId && answer.Id == acceptedAnswerId.Value)
            .Select(answer => new AnswerDto
            {
                Id = answer.Id,
                TenantId = answer.TenantId,
                QuestionId = answer.QuestionId,
                Headline = answer.Headline,
                Body = answer.Body,
                Kind = answer.Kind,
                Status = answer.Status,
                Visibility = answer.Visibility,
                ContextNote = answer.ContextNote,
                AuthorLabel = answer.AuthorLabel,
                AiConfidenceScore = answer.AiConfidenceScore,
                Score = answer.Score,
                Sort = answer.Sort,
                IsAccepted = true,
                IsOfficial = answer.Kind == AnswerKind.Official,
                LastUpdatedAtUtc = answer.UpdatedDate ?? answer.CreatedDate,
                VoteScore = 0
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    private Task<List<TagDto>> GetTagsAsync(
        Guid tenantId,
        Guid questionId,
        CancellationToken cancellationToken)
    {
        return dbContext.QuestionTags
            .AsNoTracking()
            .Where(link => link.TenantId == tenantId && link.QuestionId == questionId)
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
            .ToListAsync(cancellationToken);
    }

    private Task<List<QuestionSourceLinkDto>> GetSourcesAsync(
        Guid tenantId,
        Guid questionId,
        CancellationToken cancellationToken)
    {
        return dbContext.QuestionSourceLinks
            .AsNoTracking()
            .Where(link => link.TenantId == tenantId && link.QuestionId == questionId)
            .OrderBy(link => link.Order)
            .Select(link => new QuestionSourceLinkDto
            {
                Id = link.Id,
                QuestionId = link.QuestionId,
                SourceId = link.SourceId,
                Role = link.Role,
                Order = link.Order,
                Source = new SourceDto
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
                    Visibility = link.Source.Visibility,
                    LastVerifiedAtUtc = link.Source.LastVerifiedAtUtc,
                    LastUpdatedAtUtc = link.Source.UpdatedDate ?? link.Source.CreatedDate,
                    SpaceUsageCount = link.Source.Spaces.Count,
                    QuestionUsageCount = link.Source.Questions.Count,
                    AnswerUsageCount = link.Source.Answers.Count
                }
            })
            .ToListAsync(cancellationToken);
    }

    private async Task PopulateAcceptedAnswerVoteScoreAsync(
        Guid tenantId,
        QuestionDetailDto entity,
        CancellationToken cancellationToken)
    {
        if (entity.AcceptedAnswer is null)
            return;

        var signals = await dbContext.Activities
            .AsNoTracking()
            .Where(activity =>
                activity.TenantId == tenantId &&
                activity.QuestionId == entity.Id &&
                activity.AnswerId == entity.AcceptedAnswer.Id &&
                activity.Kind == ActivityKind.VoteReceived)
            .Select(activity => new ActivitySignalEntry(
                activity.Kind,
                activity.AnswerId,
                activity.OccurredAtUtc,
                activity.UserPrint,
                activity.MetadataJson))
            .ToListAsync(cancellationToken);

        entity.AcceptedAnswer.VoteScore = ActivitySignals.ComputeVoteScore(
            signals,
            entity.AcceptedAnswer.Id);
    }
}
