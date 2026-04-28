using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.Source;
using BaseFaq.QnA.Common.Persistence.QnADb.DbContext;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.Source.Queries.GetSource;

public sealed class SourcesGetSourceQueryHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<SourcesGetSourceQuery, SourceDto>
{
    public async Task<SourceDto> Handle(SourcesGetSourceQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
        var entity = await dbContext.Sources.AsNoTracking()
            .Where(source => source.TenantId == tenantId && source.Id == request.Id)
            .Select(source => new SourceDto
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
                AllowsCitation = source.AllowsCitation,
                CapturedAtUtc = source.CapturedAtUtc,
                LastVerifiedAtUtc = source.LastVerifiedAtUtc,
                SpaceUsageCount = source.Spaces.Count,
                QuestionUsageCount = source.Questions.Count,
                AnswerUsageCount = source.Answers.Count
            })
            .SingleOrDefaultAsync(cancellationToken);

        return entity is null
            ? throw new ApiErrorException(
                $"Source '{request.Id}' was not found.",
                (int)HttpStatusCode.NotFound)
            : entity;
    }
}
