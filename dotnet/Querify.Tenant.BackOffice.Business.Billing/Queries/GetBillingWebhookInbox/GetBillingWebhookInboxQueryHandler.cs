using Querify.Common.EntityFramework.Tenant;
using Querify.Models.Tenant.Dtos.Billing;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Querify.Tenant.BackOffice.Business.Billing.Queries.GetBillingWebhookInbox;

public sealed class GetBillingWebhookInboxQueryHandler(TenantDbContext dbContext)
    : IRequestHandler<GetBillingWebhookInboxQuery, BillingWebhookInboxDetailDto?>
{
    public Task<BillingWebhookInboxDetailDto?> Handle(
        GetBillingWebhookInboxQuery request,
        CancellationToken cancellationToken)
    {
        return dbContext.BillingWebhookInboxes
            .AsNoTracking()
            .Where(entry => entry.Id == request.Id)
            .Select(entry => new BillingWebhookInboxDetailDto
            {
                Id = entry.Id,
                TenantId = entry.TenantId,
                Provider = entry.Provider,
                ExternalEventId = entry.ExternalEventId,
                EventType = entry.EventType,
                SignatureValid = entry.SignatureValid,
                IsLiveMode = entry.IsLiveMode,
                ProviderAccountId = entry.ProviderAccountId,
                Status = entry.Status.ToString(),
                AttemptCount = entry.AttemptCount,
                ReceivedDateUtc = entry.ReceivedDateUtc,
                EventCreatedAtUtc = entry.EventCreatedAtUtc,
                LastAttemptDateUtc = entry.LastAttemptDateUtc,
                NextAttemptDateUtc = entry.NextAttemptDateUtc,
                ProcessedDateUtc = entry.ProcessedDateUtc,
                LastError = entry.LastError,
                PayloadJson = entry.PayloadJson,
                Signature = entry.Signature
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
