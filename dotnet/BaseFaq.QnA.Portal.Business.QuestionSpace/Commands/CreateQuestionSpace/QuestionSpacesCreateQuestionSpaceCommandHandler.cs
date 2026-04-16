using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.QuestionSpace;
using BaseFaq.QnA.Common.Persistence.QnADb;
using MediatR;
using QuestionSpaceEntity = BaseFaq.QnA.Common.Persistence.QnADb.Entities.QuestionSpace;

namespace BaseFaq.QnA.Portal.Business.QuestionSpace.Commands;

public sealed class QuestionSpacesCreateQuestionSpaceCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<QuestionSpacesCreateQuestionSpaceCommand, Guid>
{
    public async Task<Guid> Handle(QuestionSpacesCreateQuestionSpaceCommand request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(AppEnum.QnA);
        var userId = sessionService.GetUserId().ToString();

        var entity = new QuestionSpaceEntity
        {
            TenantId = tenantId,
            Name = request.Request.Name,
            Key = request.Request.Key,
            DefaultLanguage = request.Request.DefaultLanguage,
            CreatedBy = userId,
            UpdatedBy = userId
        };
        dbContext.QuestionSpaces.Add(entity);

        Apply(entity, request.Request, userId);

        await dbContext.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    private static void Apply(
        QuestionSpaceEntity entity,
        QuestionSpaceCreateRequestDto request,
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
            request.Visibility is BaseFaq.Models.QnA.Enums.VisibilityScope.Public or BaseFaq.Models.QnA.Enums.VisibilityScope.PublicIndexed
                ? DateTime.UtcNow
                : null;

        if (request.MarkValidated)
        {
            entity.LastValidatedAtUtc = DateTime.UtcNow;
        }

        entity.UpdatedBy = userId;
    }
}
