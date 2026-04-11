using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Tenant.Dtos.Billing;

namespace BaseFaq.Tenant.Portal.Business.Billing.Abstractions;

public interface IBillingPortalService
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
}
