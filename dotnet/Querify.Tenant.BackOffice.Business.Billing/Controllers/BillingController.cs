using Querify.Models.Common.Dtos;
using Querify.Models.Tenant.Dtos.Billing;
using Querify.Tenant.BackOffice.Business.Billing.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Querify.Tenant.BackOffice.Business.Billing.Controllers;

[Authorize]
[ApiController]
[Route("api/tenant/billing")]
public class BillingController(IBillingService billingService) : ControllerBase
{
    [HttpGet("tenants/{tenantId:guid}/summary")]
    [ProducesResponseType(typeof(TenantBillingSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary(Guid tenantId, CancellationToken token)
    {
        var result = await billingService.GetSummary(tenantId, token);
        return Ok(result);
    }

    [HttpGet("tenants/{tenantId:guid}/subscription")]
    [ProducesResponseType(typeof(TenantSubscriptionDetailDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSubscription(Guid tenantId, CancellationToken token)
    {
        var result = await billingService.GetSubscription(tenantId, token);
        return Ok(result);
    }

    [HttpGet("tenants/{tenantId:guid}/invoices")]
    [ProducesResponseType(typeof(PagedResultDto<BillingInvoiceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInvoices(
        Guid tenantId,
        [FromQuery] BillingInvoiceGetAllRequestDto requestDto,
        CancellationToken token)
    {
        var result = await billingService.GetInvoices(tenantId, requestDto, token);
        return Ok(result);
    }

    [HttpGet("tenants/{tenantId:guid}/payments")]
    [ProducesResponseType(typeof(PagedResultDto<BillingPaymentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPayments(
        Guid tenantId,
        [FromQuery] BillingPaymentGetAllRequestDto requestDto,
        CancellationToken token)
    {
        var result = await billingService.GetPayments(tenantId, requestDto, token);
        return Ok(result);
    }

    [HttpGet("webhook-inboxes")]
    [ProducesResponseType(typeof(PagedResultDto<BillingWebhookInboxDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWebhookInboxes(
        [FromQuery] BillingWebhookInboxGetAllRequestDto requestDto,
        CancellationToken token)
    {
        var result = await billingService.GetWebhookInboxes(requestDto, token);
        return Ok(result);
    }

    [HttpGet("webhook-inboxes/{id:guid}")]
    [ProducesResponseType(typeof(BillingWebhookInboxDetailDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWebhookInbox(Guid id, CancellationToken token)
    {
        var result = await billingService.GetWebhookInbox(id, token);
        return Ok(result);
    }

    [HttpPost("webhook-inboxes/{id:guid}/requeue")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    public async Task<IActionResult> RequeueWebhookInbox(Guid id, CancellationToken token)
    {
        var result = await billingService.RequeueWebhookInbox(id, token);
        return Ok(result);
    }

    [HttpPost("tenants/{tenantId:guid}/recompute-entitlements")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    public async Task<IActionResult> RecomputeEntitlements(Guid tenantId, CancellationToken token)
    {
        var result = await billingService.RecomputeEntitlements(tenantId, token);
        return Ok(result);
    }
}
