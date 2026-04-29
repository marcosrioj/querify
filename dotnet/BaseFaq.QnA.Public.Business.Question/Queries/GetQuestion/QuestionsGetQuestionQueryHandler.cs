using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Common.Infrastructure.Core.Constants;
using BaseFaq.Models.QnA.Dtos.Question;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb.DbContext;
using BaseFaq.QnA.Common.Persistence.QnADb.Mappings;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Public.Business.Question.Queries.GetQuestion;

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
        var entity = await BuildVisibleQuestionDetailQuery(tenantId)
            .AsNoTracking()
            .SingleOrDefaultAsync(question => question.Id == request.Id, cancellationToken);

        return entity is null
            ? throw new ApiErrorException($"Question '{request.Id}' was not found.", (int)HttpStatusCode.NotFound)
            : entity.ToPublicQuestionDetailDto(request.Request);
    }

    private async Task<Guid> ResolveTenantIdAndSetContextAsync(CancellationToken cancellationToken)
    {
        var clientKey = clientKeyContextService.GetRequiredClientKey();
        var tenantId = await tenantClientKeyResolver.ResolveTenantId(clientKey, cancellationToken);
        httpContextAccessor.HttpContext?.Items[TenantContextKeys.TenantIdItemKey] = tenantId;
        return tenantId;
    }

    private IQueryable<Common.Persistence.QnADb.Entities.Question> BuildVisibleQuestionDetailQuery(Guid tenantId)
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
            .Include(question => question.Tags)
            .ThenInclude(link => link.Tag)
            .Include(question => question.Activities)
            .Where(question =>
                question.TenantId == tenantId &&
                question.Visibility == VisibilityScope.Public &&
                question.Status == QuestionStatus.Active);
    }
}
