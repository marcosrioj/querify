using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Tenant.Dtos.Billing;
using BaseFaq.Tenant.BackOffice.Business.Billing.Abstractions;
using BaseFaq.Tenant.BackOffice.Business.Billing.Commands.RecomputeTenantEntitlements;
using BaseFaq.Tenant.BackOffice.Business.Billing.Commands.RequeueBillingWebhookInbox;
using BaseFaq.Tenant.BackOffice.Business.Billing.Queries.GetBillingInvoices;
using BaseFaq.Tenant.BackOffice.Business.Billing.Queries.GetBillingPayments;
using BaseFaq.Tenant.BackOffice.Business.Billing.Queries.GetBillingSubscription;
using BaseFaq.Tenant.BackOffice.Business.Billing.Queries.GetBillingSummary;
using BaseFaq.Tenant.BackOffice.Business.Billing.Queries.GetBillingWebhookInbox;
using BaseFaq.Tenant.BackOffice.Business.Billing.Queries.GetBillingWebhookInboxList;
using MediatR;

namespace BaseFaq.Tenant.BackOffice.Business.Billing.Service;

public sealed class BillingService(IMediator mediator) : IBillingService
{
    public async Task<TenantBillingSummaryDto> GetSummary(Guid tenantId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetBillingSummaryQuery { TenantId = tenantId }, cancellationToken);
        if (result is null)
        {
            throw new ApiErrorException(
                $"Tenant '{tenantId}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        return result;
    }

    public async Task<TenantSubscriptionDetailDto> GetSubscription(Guid tenantId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetBillingSubscriptionQuery { TenantId = tenantId }, cancellationToken);
        if (result is null)
        {
            throw new ApiErrorException(
                $"Tenant '{tenantId}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        return result;
    }

    public Task<PagedResultDto<BillingInvoiceDto>> GetInvoices(
        Guid tenantId,
        BillingInvoiceGetAllRequestDto requestDto,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(requestDto);

        return mediator.Send(
            new GetBillingInvoicesQuery
            {
                TenantId = tenantId,
                Request = requestDto
            },
            cancellationToken);
    }

    public Task<PagedResultDto<BillingPaymentDto>> GetPayments(
        Guid tenantId,
        BillingPaymentGetAllRequestDto requestDto,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(requestDto);

        return mediator.Send(
            new GetBillingPaymentsQuery
            {
                TenantId = tenantId,
                Request = requestDto
            },
            cancellationToken);
    }

    public Task<PagedResultDto<BillingWebhookInboxDto>> GetWebhookInboxes(
        BillingWebhookInboxGetAllRequestDto requestDto,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(requestDto);
        return mediator.Send(new GetBillingWebhookInboxListQuery { Request = requestDto }, cancellationToken);
    }

    public async Task<BillingWebhookInboxDetailDto> GetWebhookInbox(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetBillingWebhookInboxQuery { Id = id }, cancellationToken);
        if (result is null)
        {
            throw new ApiErrorException(
                $"Billing webhook inbox item '{id}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        return result;
    }

    public Task<Guid> RequeueWebhookInbox(Guid id, CancellationToken cancellationToken)
    {
        return mediator.Send(new RequeueBillingWebhookInboxCommand { Id = id }, cancellationToken);
    }

    public Task<Guid> RecomputeEntitlements(Guid tenantId, CancellationToken cancellationToken)
    {
        return mediator.Send(new RecomputeTenantEntitlementsCommand { TenantId = tenantId }, cancellationToken);
    }
}
