namespace BaseFaq.Tenant.Worker.Business.Billing.Abstractions;

public interface IBillingWebhookInboxProcessor
{
    Task<int> ProcessBatchAsync(CancellationToken cancellationToken = default);
}
