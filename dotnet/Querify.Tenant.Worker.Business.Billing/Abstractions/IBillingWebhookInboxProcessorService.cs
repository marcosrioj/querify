namespace Querify.Tenant.Worker.Business.Billing.Abstractions;

public interface IBillingWebhookInboxProcessorService
{
    Task<bool> ProcessBatchAsync(CancellationToken cancellationToken = default);
}
