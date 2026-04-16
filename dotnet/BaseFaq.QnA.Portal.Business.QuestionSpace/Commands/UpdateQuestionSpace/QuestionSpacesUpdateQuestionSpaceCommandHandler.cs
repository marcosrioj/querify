using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.QuestionSpace;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestionSpaceEntity = BaseFaq.QnA.Common.Persistence.QnADb.Entities.QuestionSpace;

namespace BaseFaq.QnA.Portal.Business.QuestionSpace.Commands.UpdateQuestionSpace;

public sealed class QuestionSpacesUpdateQuestionSpaceCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<QuestionSpacesUpdateQuestionSpaceCommand, Guid>
{
    public async Task<Guid> Handle(QuestionSpacesUpdateQuestionSpaceCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(AppEnum.QnA);
        var userId = sessionService.GetUserId().ToString();
        var entity = await dbContext.QuestionSpaces
            .Include(space => space.Tags)
            .ThenInclude(link => link.Tag)
            .Include(space => space.Sources)
            .ThenInclude(link => link.KnowledgeSource)
            .SingleOrDefaultAsync(space => space.TenantId == tenantId && space.Id == request.Id, cancellationToken);

        if (entity is null)
            throw new ApiErrorException($"Question space '{request.Id}' was not found.", (int)HttpStatusCode.NotFound);

        Apply(entity, request.Request, userId);
        await dbContext.SaveChangesAsync(cancellationToken);
        return request.Id;
    }

    private static void Apply(
        QuestionSpaceEntity entity,
        QuestionSpaceUpdateRequestDto request,
        string userId)
    {
        entity.Name = request.Name;
        entity.Key = request.Key;
        entity.DefaultLanguage = request.DefaultLanguage;
        entity.Summary = request.Summary;
        entity.ProductScope = request.ProductScope;
        entity.JourneyScope = request.JourneyScope;
        entity.ModerationPolicy = request.ModerationPolicy;
        entity.AcceptsQuestions = request.AcceptsQuestions;
        entity.AcceptsAnswers = request.AcceptsAnswers;
        entity.RequiresQuestionReview = request.RequiresQuestionReview;
        entity.RequiresAnswerReview = request.RequiresAnswerReview;
        entity.Kind = request.Kind;
        entity.Visibility = request.Visibility;
        entity.SearchMarkupMode = request.SearchMarkupMode;
        entity.PublishedAtUtc =
            request.Visibility is VisibilityScope.Public or VisibilityScope.PublicIndexed
                ? entity.PublishedAtUtc ?? DateTime.UtcNow
                : null;

        if (request.MarkValidated) entity.LastValidatedAtUtc = DateTime.UtcNow;

        entity.UpdatedBy = userId;
    }
}
