using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.QuestionSpace;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Public.Business.QuestionSpace.Queries;

public sealed class QuestionSpacesGetQuestionSpaceQueryHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<QuestionSpacesGetQuestionSpaceQuery, QuestionSpaceDto>
{
    public async Task<QuestionSpaceDto> Handle(QuestionSpacesGetQuestionSpaceQuery request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(AppEnum.QnA);
        var entity = await dbContext.QuestionSpaces
            .Include(space => space.Questions)
            .AsNoTracking()
            .SingleOrDefaultAsync(
                space =>
                    space.TenantId == tenantId &&
                    space.Id == request.Id &&
                    (space.Visibility == VisibilityScope.Public || space.Visibility == VisibilityScope.PublicIndexed),
                cancellationToken);

        return entity is null
            ? throw new ApiErrorException($"Question space '{request.Id}' was not found.", errorCode: (int)HttpStatusCode.NotFound)
            : new QuestionSpaceDto
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
                QuestionCount = entity.Questions.Count
            };
    }
}
