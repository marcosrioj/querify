using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Tenant.Dtos.Billing;
using BaseFaq.Tenant.Portal.Business.Billing.Abstractions;
using BaseFaq.Tenant.Portal.Business.Billing.Queries.GetBillingInvoices;
using BaseFaq.Tenant.Portal.Business.Billing.Queries.GetBillingPayments;
using BaseFaq.Tenant.Portal.Business.Billing.Queries.GetBillingSubscription;
using BaseFaq.Tenant.Portal.Business.Billing.Queries.GetBillingSummary;
using BaseFaq.Tenant.Portal.Business.Tenant.Abstractions;
using MediatR;

namespace BaseFaq.Tenant.Portal.Business.Billing.Service;

public sealed class BillingPortalService(
    IMediator mediator,
    ITenantPortalAccessService tenantPortalAccessService)
    : IBillingPortalService
{
    public async Task<TenantBillingSummaryDto> GetSummary(Guid tenantId, CancellationToken cancellationToken)
    {
        await tenantPortalAccessService.EnsureAccessAsync(tenantId, cancellationToken);
        return await mediator.Send(new GetBillingSummaryQuery { TenantId = tenantId }, cancellationToken) ??
               new TenantBillingSummaryDto { TenantId = tenantId };
    }

    public async Task<TenantSubscriptionDetailDto> GetSubscription(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        await tenantPortalAccessService.EnsureAccessAsync(tenantId, cancellationToken);
        return await mediator.Send(new GetBillingSubscriptionQuery { TenantId = tenantId }, cancellationToken) ??
               new TenantSubscriptionDetailDto { TenantId = tenantId };
    }

    public async Task<PagedResultDto<BillingInvoiceDto>> GetInvoices(
        Guid tenantId,
        BillingInvoiceGetAllRequestDto requestDto,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(requestDto);

        await tenantPortalAccessService.EnsureAccessAsync(tenantId, cancellationToken);
        return await mediator.Send(
            new GetBillingInvoicesQuery
            {
                TenantId = tenantId,
                Request = requestDto
            },
            cancellationToken);
    }

    public async Task<PagedResultDto<BillingPaymentDto>> GetPayments(
        Guid tenantId,
        BillingPaymentGetAllRequestDto requestDto,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(requestDto);

        await tenantPortalAccessService.EnsureAccessAsync(tenantId, cancellationToken);
        return await mediator.Send(
            new GetBillingPaymentsQuery
            {
                TenantId = tenantId,
                Request = requestDto
            },
            cancellationToken);
    }
}
