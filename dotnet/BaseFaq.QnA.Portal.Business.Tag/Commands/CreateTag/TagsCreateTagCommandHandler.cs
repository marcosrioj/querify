using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.Tag.Commands.CreateTag;

public sealed class TagsCreateTagCommandHandler(QnADbContext dbContext, ISessionService sessionService)
    : IRequestHandler<TagsCreateTagCommand, Guid>
{
    public async Task<Guid> Handle(TagsCreateTagCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Request);

        var tenantId = sessionService.GetTenantId(AppEnum.QnA);
        var userId = sessionService.GetUserId().ToString();
        var entity = new Common.Persistence.QnADb.Entities.Tag
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
