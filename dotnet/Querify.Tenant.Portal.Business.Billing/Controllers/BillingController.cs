using Querify.Common.Infrastructure.Core.Extensions;
using Querify.Models.Common.Dtos;
using Querify.Models.Tenant.Dtos.Billing;
using Querify.Tenant.Portal.Business.Billing.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Querify.Tenant.Portal.Business.Billing.Controllers;

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
