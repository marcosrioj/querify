using System.Text;
using BaseFaq.Common.Infrastructure.Core.Attributes;
using BaseFaq.Tenant.Public.Business.Billing.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BaseFaq.Tenant.Public.Business.Billing.Controllers;

[AllowAnonymous]
[ApiController]
[SkipTenantAccessValidation]
[Route("api/public/billing/webhooks/stripe")]
public class BillingWebhookController(IBillingWebhookService billingWebhookService) : ControllerBase
{
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReceiveStripeWebhook(CancellationToken token)
    {
        var stripeSignature = Request.Headers["Stripe-Signature"].FirstOrDefault();

        using var reader = new StreamReader(
            Request.Body,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            leaveOpen: true);
        var payloadJson = await reader.ReadToEndAsync(token);

        await billingWebhookService.IngestStripeWebhookAsync(payloadJson, stripeSignature, token);
        return Ok();
    }
}
