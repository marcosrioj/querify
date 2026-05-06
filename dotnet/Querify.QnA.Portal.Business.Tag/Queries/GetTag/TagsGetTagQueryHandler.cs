using System.Net;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Models.Common.Enums;
using Querify.Models.QnA.Dtos.Tag;
using Querify.QnA.Common.Persistence.QnADb.DbContext;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Querify.QnA.Portal.Business.Tag.Queries.GetTag;

public sealed class TagsGetTagQueryHandler(QnADbContext dbContext, ISessionService sessionService)
    : IRequestHandler<TagsGetTagQuery, TagDto>
{
    public async Task<TagDto> Handle(TagsGetTagQuery request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
        var entity = await dbContext.Tags.AsNoTracking()
            .Where(tag => tag.TenantId == tenantId && tag.Id == request.Id)
            .Select(tag => new TagDto
            {
                Id = tag.Id,
                TenantId = tag.TenantId,
                Name = tag.Name,
                SpaceUsageCount = tag.Spaces.Count,
                QuestionUsageCount = tag.Questions.Count,
                LastUpdatedAtUtc = tag.UpdatedDate ?? tag.CreatedDate
            })
            .FirstOrDefaultAsync(cancellationToken);

        return entity is null
            ? throw new ApiErrorException($"Tag '{request.Id}' was not found.", (int)HttpStatusCode.NotFound)
            : entity;
    }
}
