using MediatR;
using Microsoft.EntityFrameworkCore;
using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Models.Common.Dtos;
using Querify.Models.Common.Enums;
using Querify.Models.QnA.Dtos.Search;
using Querify.QnA.Common.Persistence.QnADb.DbContext;

namespace Querify.QnA.Public.Business.Search.Queries.Search;

public sealed class QnASearchQueryHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<QnASearchQuery, PagedResultDto<QnASearchResultDto>>
{
    private const int AnswerPreviewLength = 500;

    public async Task<PagedResultDto<QnASearchResultDto>> Handle(
        QnASearchQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Request);

        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
        var searchText = NormalizeSearchText(request.Request.SearchText);
        var hasSearchText = searchText is not null;
        var normalizedSearchText = searchText ?? string.Empty;

        var query = dbContext.Questions
            .AsNoTracking()
            .Where(question => question.TenantId == tenantId);

        if (request.Request.SpaceId is not null)
            query = query.Where(question => question.SpaceId == request.Request.SpaceId);

        if (!string.IsNullOrWhiteSpace(request.Request.SpaceSlug))
            query = query.Where(question => question.Space.Slug == request.Request.SpaceSlug);

        if (request.Request.Status is not null)
            query = query.Where(question => question.Status == request.Request.Status);

        if (request.Request.Visibility is not null)
            query = query.Where(question => question.Visibility == request.Request.Visibility);

        if (hasSearchText)
            query = query.Where(question =>
                question.Title.ToLower().Contains(normalizedSearchText) ||
                (question.Summary ?? string.Empty).ToLower().Contains(normalizedSearchText) ||
                (question.ContextNote ?? string.Empty).ToLower().Contains(normalizedSearchText) ||
                question.Space.Name.ToLower().Contains(normalizedSearchText) ||
                question.Space.Slug.ToLower().Contains(normalizedSearchText) ||
                (question.Space.Summary ?? string.Empty).ToLower().Contains(normalizedSearchText) ||
                question.Space.Language.ToLower().Contains(normalizedSearchText) ||
                question.Tags.Any(link => link.Tag.Name.ToLower().Contains(normalizedSearchText)) ||
                question.Answers.Any(answer =>
                    answer.Headline.ToLower().Contains(normalizedSearchText) ||
                    (answer.Body ?? string.Empty).ToLower().Contains(normalizedSearchText) ||
                    (answer.ContextNote ?? string.Empty).ToLower().Contains(normalizedSearchText) ||
                    (answer.AuthorLabel ?? string.Empty).ToLower().Contains(normalizedSearchText)));

        query = request.Request.Sorting?.Trim().ToLowerInvariant() switch
        {
            "title" or "title asc" => query.OrderBy(question => question.Title),
            "title desc" => query.OrderByDescending(question => question.Title),
            "lastupdatedatutc" or "lastupdatedatutc asc" or "updateddate" or "updateddate asc" =>
                query.OrderBy(question => question.UpdatedDate ?? question.CreatedDate),
            "lastupdatedatutc desc" or "updateddate desc" =>
                query.OrderByDescending(question => question.UpdatedDate ?? question.CreatedDate),
            "lastactivityatutc asc" => query.OrderBy(question =>
                question.LastActivityAtUtc ?? question.UpdatedDate ?? question.CreatedDate),
            _ => query.OrderByDescending(question =>
                    question.LastActivityAtUtc ?? question.UpdatedDate ?? question.CreatedDate)
                .ThenBy(question => question.Title)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip(request.Request.SkipCount)
            .Take(request.Request.MaxResultCount)
            .Select(question => new QnASearchResultDto
            {
                TenantId = question.TenantId,
                QuestionId = question.Id,
                SpaceId = question.SpaceId,
                SpaceSlug = question.Space.Slug,
                SpaceName = question.Space.Name,
                Title = question.Title,
                Summary = question.Summary,
                Status = question.Status,
                Visibility = question.Visibility,
                AcceptedAnswerId = question.AcceptedAnswerId,
                AcceptedAnswerHeadline = question.AcceptedAnswer == null ? null : question.AcceptedAnswer.Headline,
                AcceptedAnswerBodyPreview = question.AcceptedAnswer == null || question.AcceptedAnswer.Body == null
                    ? null
                    : question.AcceptedAnswer.Body.Length <= AnswerPreviewLength
                        ? question.AcceptedAnswer.Body
                        : question.AcceptedAnswer.Body.Substring(0, AnswerPreviewLength),
                AnswerCount = question.Answers.Count,
                Tags = question.Tags
                    .OrderBy(link => link.Tag.Name)
                    .Select(link => link.Tag.Name)
                    .ToList(),
                LastActivityAtUtc = question.LastActivityAtUtc,
                LastUpdatedAtUtc = question.UpdatedDate ?? question.CreatedDate,
                MatchedQuestion = hasSearchText && (
                    question.Title.ToLower().Contains(normalizedSearchText) ||
                    (question.Summary ?? string.Empty).ToLower().Contains(normalizedSearchText) ||
                    (question.ContextNote ?? string.Empty).ToLower().Contains(normalizedSearchText)),
                MatchedAnswer = hasSearchText && question.Answers.Any(answer =>
                    answer.Headline.ToLower().Contains(normalizedSearchText) ||
                    (answer.Body ?? string.Empty).ToLower().Contains(normalizedSearchText) ||
                    (answer.ContextNote ?? string.Empty).ToLower().Contains(normalizedSearchText) ||
                    (answer.AuthorLabel ?? string.Empty).ToLower().Contains(normalizedSearchText)),
                MatchedSpace = hasSearchText && (
                    question.Space.Name.ToLower().Contains(normalizedSearchText) ||
                    question.Space.Slug.ToLower().Contains(normalizedSearchText) ||
                    (question.Space.Summary ?? string.Empty).ToLower().Contains(normalizedSearchText) ||
                    question.Space.Language.ToLower().Contains(normalizedSearchText)),
                MatchedTag = hasSearchText &&
                             question.Tags.Any(link => link.Tag.Name.ToLower().Contains(normalizedSearchText))
            })
            .ToListAsync(cancellationToken);

        return new PagedResultDto<QnASearchResultDto>(totalCount, items);
    }

    private static string? NormalizeSearchText(string? searchText)
    {
        return string.IsNullOrWhiteSpace(searchText)
            ? null
            : searchText.Trim().ToLowerInvariant();
    }
}
