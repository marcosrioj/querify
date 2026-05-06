using Querify.Models.Common.Dtos;
using Querify.Models.Tenant.Dtos.Billing;
using Querify.Tenant.Portal.Business.Billing.Abstractions;
using Querify.Tenant.Portal.Business.Billing.Queries.GetBillingInvoices;
using Querify.Tenant.Portal.Business.Billing.Queries.GetBillingPayments;
using Querify.Tenant.Portal.Business.Billing.Queries.GetBillingSubscription;
using Querify.Tenant.Portal.Business.Billing.Queries.GetBillingSummary;
using Querify.Tenant.Portal.Business.Tenant.Abstractions;
using MediatR;

namespace Querify.Tenant.Portal.Business.Billing.Service;

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
