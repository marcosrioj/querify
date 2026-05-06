using System.Net;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Models.Common.Enums;
using Querify.Models.QnA.Dtos.Answer;
using Querify.Models.QnA.Dtos.Source;
using Querify.Models.QnA.Enums;
using Querify.QnA.Common.Domain.BusinessRules.Activities;
using Querify.QnA.Common.Persistence.QnADb.DbContext;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Querify.QnA.Portal.Business.Answer.Queries.GetAnswer;

public sealed class AnswersGetAnswerQueryHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<AnswersGetAnswerQuery, AnswerDto>
{
    public async Task<AnswerDto> Handle(AnswersGetAnswerQuery request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
        var entity = await dbContext.Answers
            .AsNoTracking()
            .Where(answer => answer.TenantId == tenantId && answer.Id == request.Id)
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
                IsAccepted = answer.Question.AcceptedAnswerId == answer.Id,
                IsOfficial = answer.Kind == AnswerKind.Official,
                LastUpdatedAtUtc = answer.UpdatedDate ?? answer.CreatedDate,
                VoteScore = 0,
                Sources = answer.Sources
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
            .FirstOrDefaultAsync(cancellationToken);

        if (entity is null)
            throw new ApiErrorException($"Answer '{request.Id}' was not found.", (int)HttpStatusCode.NotFound);

        var voteSignals = await dbContext.Activities
            .AsNoTracking()
            .Where(activity =>
                activity.TenantId == tenantId &&
                activity.QuestionId == entity.QuestionId &&
                activity.AnswerId == entity.Id &&
                activity.Kind == ActivityKind.VoteReceived)
            .Select(activity => new ActivitySignalEntry(
                activity.Kind,
                activity.AnswerId,
                activity.OccurredAtUtc,
                activity.UserPrint,
                activity.MetadataJson))
            .ToListAsync(cancellationToken);

        entity.VoteScore = ActivitySignals.ComputeVoteScore(voteSignals, entity.Id);
        return entity;
    }
}
