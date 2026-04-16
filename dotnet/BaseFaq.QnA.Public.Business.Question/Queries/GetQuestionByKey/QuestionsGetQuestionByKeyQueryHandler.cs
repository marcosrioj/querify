using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Common.Infrastructure.Core.Constants;
using BaseFaq.Models.QnA.Dtos.Question;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb;
using BaseFaq.QnA.Common.Persistence.QnADb.Projections;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuestionEntity = BaseFaq.QnA.Common.Persistence.QnADb.Entities.Question;

namespace BaseFaq.QnA.Public.Business.Question.Queries.GetQuestionByKey;

public sealed class QuestionsGetQuestionByKeyQueryHandler(
    QnADbContext dbContext,
    IClientKeyContextService clientKeyContextService,
    ITenantClientKeyResolver tenantClientKeyResolver,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<QuestionsGetQuestionByKeyQuery, QuestionDetailDto>
{
    public async Task<QuestionDetailDto> Handle(QuestionsGetQuestionByKeyQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Request);

        var tenantId = await ResolveTenantIdAndSetContextAsync(cancellationToken);
        var entity = await BuildVisibleQuestionDetailQuery(tenantId)
            .AsNoTracking()
            .SingleOrDefaultAsync(question => question.Key == request.Key, cancellationToken);

        return entity is null
            ? throw new ApiErrorException($"Question '{request.Key}' was not found.", (int)HttpStatusCode.NotFound)
            : entity.ToPublicQuestionDetailDto(request.Request);
    }

    private async Task<Guid> ResolveTenantIdAndSetContextAsync(CancellationToken cancellationToken)
    {
        var clientKey = clientKeyContextService.GetRequiredClientKey();
        var tenantId = await tenantClientKeyResolver.ResolveTenantId(clientKey, cancellationToken);
        httpContextAccessor.HttpContext?.Items[TenantContextKeys.TenantIdItemKey] = tenantId;
        return tenantId;
    }

    private IQueryable<QuestionEntity> BuildVisibleQuestionDetailQuery(Guid tenantId)
    {
        return dbContext.Questions
            .Include(question => question.Space)
            .Include(question => question.AcceptedAnswer)
            .ThenInclude(answer => answer!.Sources)
            .ThenInclude(link => link.Source)
            .Include(question => question.Answers)
            .ThenInclude(answer => answer.Sources)
            .ThenInclude(link => link.Source)
            .Include(question => question.Sources)
            .ThenInclude(link => link.Source)
            .Include(question => question.Topics)
            .ThenInclude(link => link.Topic)
            .Include(question => question.Activities)
            .Where(question =>
                question.TenantId == tenantId &&
                (question.Visibility == VisibilityScope.Public ||
                 question.Visibility == VisibilityScope.PublicIndexed) &&
                (question.Status == QuestionStatus.Open || question.Status == QuestionStatus.Answered ||
                 question.Status == QuestionStatus.Validated));
    }
}
