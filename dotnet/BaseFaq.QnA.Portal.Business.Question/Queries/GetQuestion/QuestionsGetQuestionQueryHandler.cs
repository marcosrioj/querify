using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.Activity;
using BaseFaq.Models.QnA.Dtos.Answer;
using BaseFaq.Models.QnA.Dtos.Question;
using BaseFaq.Models.QnA.Dtos.Source;
using BaseFaq.Models.QnA.Dtos.Tag;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Domain.BusinessRules.Activities;
using BaseFaq.QnA.Common.Persistence.QnADb.DbContext;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.Question.Queries.GetQuestion;

public sealed class QuestionsGetQuestionQueryHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<QuestionsGetQuestionQuery, QuestionDetailDto>
{
    public async Task<QuestionDetailDto> Handle(QuestionsGetQuestionQuery request, CancellationToken cancellationToken)
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
            throw new ApiErrorException($"Question '{request.Id}' was not found.", (int)HttpStatusCode.NotFound);

        entity.Answers = await GetAnswersAsync(tenantId, entity, cancellationToken);
        entity.AcceptedAnswer = entity.Answers.FirstOrDefault(answer => answer.Id == entity.AcceptedAnswerId);
        entity.Tags = await GetTagsAsync(tenantId, request.Id, cancellationToken);
        entity.Sources = await GetSourcesAsync(tenantId, request.Id, cancellationToken);
        entity.Activity = await GetActivityAsync(tenantId, request.Id, cancellationToken);

        PopulateVoteScores(entity);
        return entity;
    }

    private async Task<List<AnswerDto>> GetAnswersAsync(
        Guid tenantId,
        QuestionDetailDto question,
        CancellationToken cancellationToken)
    {
        var answers = await dbContext.Answers
            .AsNoTracking()
            .Where(answer => answer.TenantId == tenantId && answer.QuestionId == question.Id)
            .OrderByDescending(answer => answer.Id == question.AcceptedAnswerId)
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
                IsAccepted = answer.Id == question.AcceptedAnswerId,
                IsOfficial = answer.Kind == AnswerKind.Official,
                LastUpdatedAtUtc = answer.UpdatedDate ?? answer.CreatedDate,
                VoteScore = 0
            })
            .ToListAsync(cancellationToken);

        await PopulateAnswerSourcesAsync(tenantId, answers, cancellationToken);
        return answers;
    }

    private async Task PopulateAnswerSourcesAsync(
        Guid tenantId,
        IReadOnlyCollection<AnswerDto> answers,
        CancellationToken cancellationToken)
    {
        if (answers.Count == 0)
            return;

        var answerIds = answers.Select(answer => answer.Id).ToHashSet();
        var sourceLinksQuery = dbContext.AnswerSourceLinks
            .AsNoTracking()
            .Where(link =>
                link.TenantId == tenantId &&
                answerIds.Contains(link.AnswerId));

        var sourceLinks = await sourceLinksQuery
            .OrderBy(link => link.Order)
            .Select(link => new
            {
                link.AnswerId,
                Source = new AnswerSourceLinkDto
                {
                    Id = link.Id,
                    AnswerId = link.AnswerId,
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
                }
            })
            .ToListAsync(cancellationToken);

        var sourceLookup = sourceLinks
            .GroupBy(link => link.AnswerId)
            .ToDictionary(group => group.Key, group => group.Select(link => link.Source).ToList());

        foreach (var answer in answers)
            answer.Sources = sourceLookup.GetValueOrDefault(answer.Id) ?? [];
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

    private static void PopulateVoteScores(QuestionDetailDto entity)
    {
        var signals = entity.Activity
            .Select(activity => new ActivitySignalEntry(
                activity.Kind,
                activity.AnswerId,
                activity.OccurredAtUtc,
                activity.UserPrint,
                activity.MetadataJson))
            .ToList();

        if (entity.AcceptedAnswer is not null)
            entity.AcceptedAnswer.VoteScore = ActivitySignals.ComputeVoteScore(signals, entity.AcceptedAnswer.Id);

        foreach (var answer in entity.Answers)
            answer.VoteScore = ActivitySignals.ComputeVoteScore(signals, answer.Id);
    }
}
