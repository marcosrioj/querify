using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.Answer;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb;
using MediatR;
using Microsoft.EntityFrameworkCore;
using AnswerEntity = BaseFaq.QnA.Common.Persistence.QnADb.Entities.Answer;
using ActivityEntity = BaseFaq.QnA.Common.Persistence.QnADb.Entities.Activity;

namespace BaseFaq.QnA.Portal.Business.Answer.Commands.UpdateAnswer;

public sealed class AnswersUpdateAnswerCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<AnswersUpdateAnswerCommand, Guid>
{
    public async Task<Guid> Handle(AnswersUpdateAnswerCommand request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(AppEnum.QnA);
        var userId = sessionService.GetUserId().ToString();
        var entity = await dbContext.Answers
            .Include(answer => answer.Question)
            .ThenInclude(question => question.Activities)
            .Include(answer => answer.Sources)
            .ThenInclude(link => link.Source)
            .SingleOrDefaultAsync(answer => answer.TenantId == tenantId && answer.Id == request.Id, cancellationToken);

        if (entity is null)
            throw new ApiErrorException($"Answer '{request.Id}' was not found.", (int)HttpStatusCode.NotFound);

        Apply(entity, request.Request, userId);
        var activity = new ActivityEntity
        {
            TenantId = entity.TenantId,
            QuestionId = entity.QuestionId,
            Question = entity.Question,
            AnswerId = entity.Id,
            Answer = entity,
            Kind = ActivityKind.AnswerUpdated,
            ActorKind = ActorKind.Moderator,
            ActorLabel = userId,
            UserPrint = string.Empty,
            Ip = string.Empty,
            UserAgent = string.Empty,
            OccurredAtUtc = DateTime.UtcNow,
            CreatedBy = userId,
            UpdatedBy = userId
        };
        entity.Question.Activities.Add(activity);
        entity.Question.LastActivityAtUtc = activity.OccurredAtUtc;
        dbContext.Activities.Add(activity);

        await dbContext.SaveChangesAsync(cancellationToken);
        return request.Id;
    }

    private static void Apply(AnswerEntity entity, AnswerUpdateRequestDto request, string userId)
    {
        entity.Headline = request.Headline;
        entity.Body = request.Body;
        entity.AuthorLabel = request.AuthorLabel;
        entity.Language = request.Language;
        entity.ContextKey = request.ContextKey;
        entity.ApplicabilityRulesJson = request.ApplicabilityRulesJson;
        entity.ConfidenceScore = request.ConfidenceScore;
        entity.TrustNote = request.TrustNote;
        entity.EvidenceSummary = request.EvidenceSummary;
        entity.Rank = request.Rank;
        entity.Kind = request.Kind;

        switch (request.Status)
        {
            case AnswerStatus.Published:
                entity.Status = AnswerStatus.Published;
                entity.PublishedAtUtc = DateTime.UtcNow;
                entity.RevisionNumber++;
                break;
            case AnswerStatus.Validated:
                entity.Status = AnswerStatus.Validated;
                entity.ValidatedAtUtc = DateTime.UtcNow;
                entity.RevisionNumber++;
                break;
            case AnswerStatus.Rejected:
                entity.Status = AnswerStatus.Rejected;
                entity.Visibility = VisibilityScope.Internal;
                break;
            default:
                entity.Status = request.Status;
                break;
        }

        EnsureVisibilityAllowed(entity, request.Visibility);
        entity.Visibility = request.Visibility;
        entity.UpdatedBy = userId;
    }

    private static void EnsureVisibilityAllowed(AnswerEntity entity, VisibilityScope visibility)
    {
        if (visibility is not VisibilityScope.Public and not VisibilityScope.PublicIndexed) return;

        if (entity.Status is not AnswerStatus.Published and not AnswerStatus.Validated)
            throw new InvalidOperationException("Only published or validated answers can be exposed publicly.");

        if (entity.Kind == AnswerKind.AiDraft && entity.Status != AnswerStatus.Validated)
            throw new InvalidOperationException("AI draft answers must be validated before public exposure.");

        foreach (var sourceLink in entity.Sources)
        {
            if (sourceLink.Role is SourceRole.Citation or SourceRole.CanonicalReference &&
                (sourceLink.Source.Visibility is not VisibilityScope.Public and not VisibilityScope.PublicIndexed ||
                 !sourceLink.Source.AllowsPublicCitation))
                throw new InvalidOperationException(
                    "Public citations require a publicly visible source that explicitly allows citation.");
        }
    }
}
