using MediatR;

namespace Querify.Tenant.Worker.Business.Billing.Commands.ProcessBillingWebhookInboxBatch;

public sealed class ProcessBillingWebhookInboxBatchCommand : IRequest<bool>;
