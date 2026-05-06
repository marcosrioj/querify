using Querify.Models.Common.Dtos;
using Querify.Models.Tenant.Dtos.Billing;

namespace Querify.Tenant.BackOffice.Business.Billing.Abstractions;

public interface IBillingService
{
    Task<TenantBillingSummaryDto> GetSummary(Guid tenantId, CancellationToken cancellationToken);
    Task<TenantSubscriptionDetailDto> GetSubscription(Guid tenantId, CancellationToken cancellationToken);
    Task<PagedResultDto<BillingInvoiceDto>> GetInvoices(
        Guid tenantId,
        BillingInvoiceGetAllRequestDto requestDto,
        CancellationToken cancellationToken);
    Task<PagedResultDto<BillingPaymentDto>> GetPayments(
        Guid tenantId,
        BillingPaymentGetAllRequestDto requestDto,
        CancellationToken cancellationToken);
    Task<PagedResultDto<BillingWebhookInboxDto>> GetWebhookInboxes(
        BillingWebhookInboxGetAllRequestDto requestDto,
        CancellationToken cancellationToken);
    Task<BillingWebhookInboxDetailDto> GetWebhookInbox(Guid id, CancellationToken cancellationToken);
    Task<Guid> RequeueWebhookInbox(Guid id, CancellationToken cancellationToken);
    Task<Guid> RecomputeEntitlements(Guid tenantId, CancellationToken cancellationToken);
}
