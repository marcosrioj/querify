using System.Net;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Models.Common.Enums;
using Querify.QnA.Common.Persistence.QnADb.DbContext;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Querify.QnA.Portal.Business.Tag.Commands.UpdateTag;

public sealed class TagsUpdateTagCommandHandler(QnADbContext dbContext, ISessionService sessionService)
    : IRequestHandler<TagsUpdateTagCommand, Guid>
{
    public async Task<Guid> Handle(TagsUpdateTagCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Request);

        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
        var userId = sessionService.GetUserId().ToString();
        var entity = await dbContext.Tags
            .SingleOrDefaultAsync(tag => tag.TenantId == tenantId && tag.Id == request.Id, cancellationToken);

        if (entity is null)
            throw new ApiErrorException($"Tag '{request.Id}' was not found.", (int)HttpStatusCode.NotFound);

        entity.Name = request.Request.Name;
        entity.UpdatedBy = userId;
        await dbContext.SaveChangesAsync(cancellationToken);
        return request.Id;
    }
}