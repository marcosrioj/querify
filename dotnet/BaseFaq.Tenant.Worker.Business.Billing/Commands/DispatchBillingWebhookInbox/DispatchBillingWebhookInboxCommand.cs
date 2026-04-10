using BaseFaq.Common.EntityFramework.Tenant.Entities;
using MediatR;

namespace BaseFaq.Tenant.Worker.Business.Billing.Commands.DispatchBillingWebhookInbox;

public sealed record DispatchBillingWebhookInboxCommand(
    BillingWebhookInbox WorkItem) : IRequest;
