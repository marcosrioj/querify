using System.Net;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Common.Infrastructure.Core.Constants;
using Querify.Models.QnA.Dtos.Activity;
using Querify.Models.QnA.Dtos.Answer;
using Querify.Models.QnA.Dtos.Question;
using Querify.Models.QnA.Dtos.Source;
using Querify.Models.QnA.Dtos.Tag;
using Querify.Models.QnA.Enums;
using Querify.QnA.Common.Domain.BusinessRules.Activities;
using Querify.QnA.Common.Persistence.QnADb.DbContext;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Querify.QnA.Public.Business.Question.Queries.GetQuestion;

public sealed class QuestionsGetQuestionQueryHandler(
    QnADbContext dbContext,
    IClientKeyContextService clientKeyContextService,
    ITenantClientKeyResolver tenantClientKeyResolver,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<QuestionsGetQuestionQuery, QuestionDetailDto>
{
    public async Task<QuestionDetailDto> Handle(QuestionsGetQuestionQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Request);

        var tenantId = await ResolveTenantIdAndSetContextAsync(cancellationToken);
        var entity = await dbContext.Questions
            .AsNoTracking()
            .Where(question =>
                question.TenantId == tenantId &&
                question.Id == request.Id &&
                question.Visibility == VisibilityScope.Public &&
                question.Status == QuestionStatus.Active)
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
                AcceptedAnswerId = question.AcceptedAnswer != null &&
                                   question.AcceptedAnswer.Visibility == VisibilityScope.Public &&
                                   question.AcceptedAnswer.Status == AnswerStatus.Active
                    ? question.AcceptedAnswerId
                    : null,
                LastActivityAtUtc = question.LastActivityAtUtc,
                LastUpdatedAtUtc = question.UpdatedDate ?? question.CreatedDate
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (entity is null)
            throw new ApiErrorException($"Question '{request.Id}' was not found.", (int)HttpStatusCode.NotFound);

        if (request.Request.IncludeAnswers)
            await PopulateAnswersAsync(tenantId, entity, cancellationToken);

        if (request.Request.IncludeTags)
            entity.Tags = await GetTagsAsync(tenantId, request.Id, cancellationToken);

        if (request.Request.IncludeSources)
            entity.Sources = await GetSourcesAsync(tenantId, request.Id, cancellationToken);

        if (request.Request.IncludeActivity)
            entity.Activity = await GetActivityAsync(tenantId, request.Id, cancellationToken);

        return entity;
    }

    private async Task<Guid> ResolveTenantIdAndSetContextAsync(CancellationToken cancellationToken)
    {
        var clientKey = clientKeyContextService.GetRequiredClientKey();
        var tenantId = await tenantClientKeyResolver.ResolveTenantId(clientKey, cancellationToken);
        httpContextAccessor.HttpContext?.Items[TenantContextKeys.TenantIdItemKey] = tenantId;
        return tenantId;
    }

    private async Task PopulateAnswersAsync(
        Guid tenantId,
        QuestionDetailDto entity,
        CancellationToken cancellationToken)
    {
        var answers = await dbContext.Answers
            .AsNoTracking()
            .Where(answer =>
                answer.TenantId == tenantId &&
                answer.QuestionId == entity.Id &&
                answer.Visibility == VisibilityScope.Public &&
                answer.Status == AnswerStatus.Active)
            .OrderByDescending(answer => answer.Id == entity.AcceptedAnswerId)
            .ThenBy(answer => answer.Sort)
            .ThenByDescending(answer => answer.Score)
            .ThenBy(answer => answer.Headline)
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
                IsAccepted = answer.Id == entity.AcceptedAnswerId,
                IsOfficial = answer.Kind == AnswerKind.Official,
                LastUpdatedAtUtc = answer.UpdatedDate ?? answer.CreatedDate,
                VoteScore = 0,
                Sources = answer.Sources
                    .Where(source => source.Source.Visibility == VisibilityScope.Public)
                    .OrderBy(source => source.Order)
                    .Select(source => new AnswerSourceLinkDto
                    {
                        Id = source.Id,
                        AnswerId = source.AnswerId,
                        SourceId = source.SourceId,
                        Role = source.Role,
                        Order = source.Order,
                        Source = new SourceDto
                        {
                            Id = source.Source.Id,
                            TenantId = source.Source.TenantId,
                            Kind = source.Source.Kind,
                            Locator = source.Source.Locator,
                            Label = source.Source.Label,
                            ContextNote = source.Source.ContextNote,
                            ExternalId = source.Source.ExternalId,
                            Language = source.Source.Language,
                            MediaType = source.Source.MediaType,
                            Checksum = source.Source.Checksum,
                            MetadataJson = source.Source.MetadataJson,
                            Visibility = source.Source.Visibility,
                            LastVerifiedAtUtc = source.Source.LastVerifiedAtUtc,
                            LastUpdatedAtUtc = source.Source.UpdatedDate ?? source.Source.CreatedDate,
                            SpaceUsageCount = source.Source.Spaces.Count,
                            QuestionUsageCount = source.Source.Questions.Count,
                            AnswerUsageCount = source.Source.Answers.Count
                        }
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        entity.Answers = answers;
        entity.AcceptedAnswer = answers.FirstOrDefault(answer => answer.Id == entity.AcceptedAnswerId);
        entity.AcceptedAnswerId = entity.AcceptedAnswer?.Id;

        await PopulateVoteScoresAsync(tenantId, answers, cancellationToken);
    }

    private Task<List<TagDto>> GetTagsAsync(Guid tenantId, Guid questionId, CancellationToken cancellationToken)
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
            .Where(link =>
                link.TenantId == tenantId &&
                link.QuestionId == questionId &&
                link.Source.Visibility == VisibilityScope.Public)
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
                    Kind = link.Source.Kind,
                    Locator = link.Source.Locator,
                    Label = link.Source.Label,
                    ContextNote = link.Source.ContextNote,
                    ExternalId = link.Source.ExternalId,
                    Language = link.Source.Language,
                    MediaType = link.Source.MediaType,
                    Checksum = link.Source.Checksum,
                    MetadataJson = link.Source.MetadataJson,
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

    private Task<List<ActivityDto>> GetActivityAsync(
        Guid tenantId,
        Guid questionId,
        CancellationToken cancellationToken)
    {
        return dbContext.Activities
            .AsNoTracking()
            .Where(activity => activity.TenantId == tenantId && activity.QuestionId == questionId)
            .OrderByDescending(activity => activity.OccurredAtUtc)
            .Select(activity => new ActivityDto
            {
                Id = activity.Id,
                TenantId = activity.TenantId,
                QuestionId = activity.QuestionId,
                QuestionTitle = activity.Question.Title,
                AnswerId = activity.AnswerId,
                AnswerHeadline = activity.Answer == null ? null : activity.Answer.Headline,
                Kind = activity.Kind,
                ActorKind = activity.ActorKind,
                ActorLabel = activity.ActorLabel,
                UserPrint = activity.UserPrint,
                Ip = activity.Ip,
                UserAgent = activity.UserAgent,
                Notes = activity.Notes,
                MetadataJson = activity.MetadataJson,
                OccurredAtUtc = activity.OccurredAtUtc
            })
            .ToListAsync(cancellationToken);
    }

    private async Task PopulateVoteScoresAsync(
        Guid tenantId,
        IReadOnlyCollection<AnswerDto> answers,
        CancellationToken cancellationToken)
    {
        if (answers.Count == 0)
            return;

        var answerIds = answers.Select(answer => answer.Id).ToHashSet();
        var questionId = answers.First().QuestionId;
        var voteSignals = await dbContext.Activities
            .AsNoTracking()
            .Where(activity =>
                activity.TenantId == tenantId &&
                activity.QuestionId == questionId &&
                activity.Kind == ActivityKind.VoteReceived &&
                activity.AnswerId.HasValue &&
                answerIds.Contains(activity.AnswerId.Value))
            .Select(activity => new ActivitySignalEntry(
                activity.Kind,
                activity.AnswerId,
                activity.OccurredAtUtc,
                activity.UserPrint,
                activity.MetadataJson))
            .ToListAsync(cancellationToken);

        foreach (var answer in answers)
            answer.VoteScore = ActivitySignals.ComputeVoteScore(voteSignals, answer.Id);
    }
}
