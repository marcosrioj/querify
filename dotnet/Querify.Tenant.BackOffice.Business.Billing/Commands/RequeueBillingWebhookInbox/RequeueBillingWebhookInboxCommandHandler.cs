using System.Net;
using Querify.Common.EntityFramework.Tenant;
using Querify.Common.EntityFramework.Tenant.Enums;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Querify.Tenant.BackOffice.Business.Billing.Commands.RequeueBillingWebhookInbox;

public sealed class RequeueBillingWebhookInboxCommandHandler(TenantDbContext dbContext)
    : IRequestHandler<RequeueBillingWebhookInboxCommand, Guid>
{
    public async Task<Guid> Handle(
        RequeueBillingWebhookInboxCommand request,
        CancellationToken cancellationToken)
    {
        var inboxItem = await dbContext.BillingWebhookInboxes
            .FirstOrDefaultAsync(entry => entry.Id == request.Id, cancellationToken);

        if (inboxItem is null)
        {
            throw new ApiErrorException(
                $"Billing webhook inbox item '{request.Id}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        inboxItem.Status = ControlPlaneMessageStatus.Pending;
        inboxItem.AttemptCount = 0;
        inboxItem.LastAttemptDateUtc = null;
        inboxItem.NextAttemptDateUtc = DateTime.UtcNow;
        inboxItem.ProcessedDateUtc = null;
        inboxItem.LockedUntilDateUtc = null;
        inboxItem.ProcessingToken = null;
        inboxItem.LastError = null;

        await dbContext.SaveChangesAsync(cancellationToken);

        return inboxItem.Id;
    }
}
