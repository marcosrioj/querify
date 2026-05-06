using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Models.Common.Enums;
using Querify.QnA.Common.Persistence.QnADb.DbContext;
using MediatR;

namespace Querify.QnA.Portal.Business.Tag.Commands.CreateTag;

public sealed class TagsCreateTagCommandHandler(QnADbContext dbContext, ISessionService sessionService)
    : IRequestHandler<TagsCreateTagCommand, Guid>
{
    public async Task<Guid> Handle(TagsCreateTagCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Request);

        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
        var userId = sessionService.GetUserId().ToString();
        var entity = new Common.Domain.Entities.Tag
        {
            TenantId = tenantId,
            Name = request.Request.Name,
            CreatedBy = userId,
            UpdatedBy = userId
        };

        dbContext.Tags.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}