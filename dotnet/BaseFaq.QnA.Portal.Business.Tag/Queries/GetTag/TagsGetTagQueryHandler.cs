using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.Tag;
using BaseFaq.QnA.Common.Persistence.QnADb.DbContext;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.Tag.Queries.GetTag;

public sealed class TagsGetTagQueryHandler(QnADbContext dbContext, ISessionService sessionService)
    : IRequestHandler<TagsGetTagQuery, TagDto>
{
    public async Task<TagDto> Handle(TagsGetTagQuery request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
        var entity = await dbContext.Tags.AsNoTracking()
            .SingleOrDefaultAsync(tag => tag.TenantId == tenantId && tag.Id == request.Id, cancellationToken);

        return entity is null
            ? throw new ApiErrorException($"Tag '{request.Id}' was not found.", (int)HttpStatusCode.NotFound)
            : new TagDto
            {
                Id = entity.Id,
                TenantId = entity.TenantId,
                Name = entity.Name
            };
    }
}