using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.Source;
using BaseFaq.QnA.Common.Persistence.QnADb.DbContext;
using BaseFaq.QnA.Common.Persistence.QnADb.Projections;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.Source.Queries.GetSource;

public sealed class SourcesGetSourceQueryHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<SourcesGetSourceQuery, SourceDetailDto>
{
    public async Task<SourceDetailDto> Handle(SourcesGetSourceQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
        var entity = await dbContext.Sources.AsNoTracking()
            .Where(source => source.TenantId == tenantId && source.Id == request.Id)
            .Select(source => new SourceDetailDto
            {
                Id = source.Id,
                TenantId = source.TenantId,
                Kind = source.Kind,
                Locator = source.Locator,
                Label = source.Label,
                ContextNote = source.ContextNote,
                ExternalId = source.ExternalId,
                Language = source.Language,
                MediaType = source.MediaType,
                Checksum = source.Checksum,
                MetadataJson = source.MetadataJson,
                Visibility = source.Visibility,
                LastVerifiedAtUtc = source.LastVerifiedAtUtc,
                SpaceUsageCount = source.Spaces.Count,
                QuestionUsageCount = source.Questions.Count,
                AnswerUsageCount = source.Answers.Count,
                Spaces = source.Spaces
                    .OrderBy(link => link.Space.Name)
                    .ThenBy(link => link.Space.Slug)
                    .Select(link => new SourceSpaceRelationshipDto
                    {
                        Id = link.Id,
                        SpaceId = link.SpaceId,
                        Name = link.Space.Name,
                        Slug = link.Space.Slug,
                        Summary = link.Space.Summary,
                        Status = link.Space.Status,
                        Visibility = link.Space.Visibility,
                        AcceptsQuestions = link.Space.AcceptsQuestions,
                        AcceptsAnswers = link.Space.AcceptsAnswers,
                        QuestionCount = link.Space.Questions.Count
                    })
                    .ToList(),
                Questions = source.Questions
                    .OrderBy(link => link.Order)
                    .ThenBy(link => link.Question.Title)
                    .Select(link => new SourceQuestionRelationshipDto
                    {
                        Id = link.Id,
                        QuestionId = link.QuestionId,
                        SpaceId = link.Question.SpaceId,
                        SpaceSlug = link.Question.Space.Slug,
                        Title = link.Question.Title,
                        Summary = link.Question.Summary,
                        Status = QnAReadModelMappings.NormalizeQuestionStatus(link.Question.Status),
                        Visibility = link.Question.Visibility,
                        Role = link.Role,
                        Order = link.Order,
                        LastActivityAtUtc = link.Question.LastActivityAtUtc
                    })
                    .ToList(),
                Answers = source.Answers
                    .OrderBy(link => link.Order)
                    .ThenBy(link => link.Answer.Headline)
                    .Select(link => new SourceAnswerRelationshipDto
                    {
                        Id = link.Id,
                        AnswerId = link.AnswerId,
                        QuestionId = link.Answer.QuestionId,
                        QuestionTitle = link.Answer.Question.Title,
                        Headline = link.Answer.Headline,
                        Kind = link.Answer.Kind,
                        Status = link.Answer.Status,
                        Visibility = link.Answer.Visibility,
                        Role = link.Role,
                        Order = link.Order,
                        IsAccepted = link.Answer.Question.AcceptedAnswerId == link.AnswerId
                    })
                    .ToList()
            })
            .SingleOrDefaultAsync(cancellationToken);

        return entity is null
            ? throw new ApiErrorException(
                $"Source '{request.Id}' was not found.",
                (int)HttpStatusCode.NotFound)
            : entity;
    }
}
