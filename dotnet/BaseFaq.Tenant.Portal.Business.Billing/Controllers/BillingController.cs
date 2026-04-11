using BaseFaq.Common.Infrastructure.Core.Extensions;
using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Tenant.Dtos.Billing;
using BaseFaq.Tenant.Portal.Business.Billing.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BaseFaq.Tenant.Portal.Business.Billing.Controllers;

[Authorize]
[ApiController]
[Route("api/tenant/billing")]
public sealed class BillingController(IBillingPortalService billingService) : ControllerBase
{
    [HttpGet("summary")]
    [ProducesResponseType(typeof(TenantBillingSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary(CancellationToken token)
    {
        var tenantId = HttpContext.GetTenantIdFromHeader();
        var result = await billingService.GetSummary(tenantId, token);
        return Ok(result);
    }

    [HttpGet("subscription")]
    [ProducesResponseType(typeof(TenantSubscriptionDetailDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSubscription(CancellationToken token)
    {
        var tenantId = HttpContext.GetTenantIdFromHeader();
        var result = await billingService.GetSubscription(tenantId, token);
        return Ok(result);
    }

    [HttpGet("invoices")]
    [ProducesResponseType(typeof(PagedResultDto<BillingInvoiceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInvoices(
        [FromQuery] BillingInvoiceGetAllRequestDto requestDto,
        CancellationToken token)
    {
        var tenantId = HttpContext.GetTenantIdFromHeader();
        var result = await billingService.GetInvoices(tenantId, requestDto, token);
        return Ok(result);
    }

    [HttpGet("payments")]
    [ProducesResponseType(typeof(PagedResultDto<BillingPaymentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPayments(
        [FromQuery] BillingPaymentGetAllRequestDto requestDto,
        CancellationToken token)
    {
        var tenantId = HttpContext.GetTenantIdFromHeader();
        var result = await billingService.GetPayments(tenantId, requestDto, token);
        return Ok(result);
    }
}
