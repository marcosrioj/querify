using Querify.Common.EntityFramework.Tenant.Entities;
using MediatR;

namespace Querify.Tenant.Worker.Business.Billing.Commands.DispatchBillingWebhookInbox;

public sealed record DispatchBillingWebhookInboxCommand(
    BillingWebhookInbox WorkItem) : IRequest;
