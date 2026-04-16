using BaseFaq.Models.Common.Dtos;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.QuestionSpace;
using BaseFaq.QnA.Common.Persistence.QnADb;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.QuestionSpace.Queries.GetQuestionSpaceList;

public sealed class QuestionSpacesGetQuestionSpaceListQueryHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<QuestionSpacesGetQuestionSpaceListQuery, PagedResultDto<QuestionSpaceDto>>
{
    public async Task<PagedResultDto<QuestionSpaceDto>> Handle(QuestionSpacesGetQuestionSpaceListQuery request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(AppEnum.QnA);
        IQueryable<Common.Persistence.QnADb.Entities.QuestionSpace> query = dbContext.QuestionSpaces
            .Include(space => space.Questions)
            .Include(space => space.QuestionSpaceTopics)
            .ThenInclude(link => link.Topic)
            .Include(space => space.QuestionSpaceSources)
            .ThenInclude(link => link.KnowledgeSource)
            .Where(space => space.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(request.Request.SearchText))
        {
            query = query.Where(space =>
                EF.Functions.ILike(space.Name, $"%{request.Request.SearchText}%") ||
                EF.Functions.ILike(space.Key, $"%{request.Request.SearchText}%"));
        }

        if (request.Request.Visibility is not null)
        {
            query = query.Where(space => space.Visibility == request.Request.Visibility);
        }

        if (request.Request.Kind is not null)
        {
            query = query.Where(space => space.Kind == request.Request.Kind);
        }

        if (!string.IsNullOrWhiteSpace(request.Request.ProductScope))
        {
            query = query.Where(space => space.ProductScope == request.Request.ProductScope);
        }

        if (!string.IsNullOrWhiteSpace(request.Request.JourneyScope))
        {
            query = query.Where(space => space.JourneyScope == request.Request.JourneyScope);
        }

        if (request.Request.AcceptsQuestions is not null)
        {
            query = query.Where(space => space.AcceptsQuestions == request.Request.AcceptsQuestions);
        }

        if (request.Request.AcceptsAnswers is not null)
        {
            query = query.Where(space => space.AcceptsAnswers == request.Request.AcceptsAnswers);
        }

        query = request.Request.Sorting?.Trim().ToLowerInvariant() switch
        {
            "name desc" => query.OrderByDescending(space => space.Name),
            "key" => query.OrderBy(space => space.Key),
            "key desc" => query.OrderByDescending(space => space.Key),
            _ => query.OrderBy(space => space.Name)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .AsNoTracking()
            .Skip(request.Request.SkipCount)
            .Take(request.Request.MaxResultCount)
            .ToListAsync(cancellationToken);

        return new PagedResultDto<QuestionSpaceDto>(
            totalCount,
            items.Select(
                    space => new QuestionSpaceDto
                    {
                        Id = space.Id,
                        TenantId = space.TenantId,
                        Name = space.Name,
                        Key = space.Key,
                        Summary = space.Summary,
                        DefaultLanguage = space.DefaultLanguage,
                        Kind = space.Kind,
                        Visibility = space.Visibility,
                        ModerationPolicy = space.ModerationPolicy,
                        SearchMarkupMode = space.SearchMarkupMode,
                        ProductScope = space.ProductScope,
                        JourneyScope = space.JourneyScope,
                        AcceptsQuestions = space.AcceptsQuestions,
                        AcceptsAnswers = space.AcceptsAnswers,
                        RequiresQuestionReview = space.RequiresQuestionReview,
                        RequiresAnswerReview = space.RequiresAnswerReview,
                        PublishedAtUtc = space.PublishedAtUtc,
                        LastValidatedAtUtc = space.LastValidatedAtUtc,
                        QuestionCount = space.Questions.Count
                    })
                .ToList());
    }
}
