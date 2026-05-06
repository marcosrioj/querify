using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Common.Infrastructure.Core.Middleware;
using Querify.Models.Common.Dtos;
using Querify.Models.Tenant.Dtos.Billing;
using Querify.Tenant.Portal.Business.Billing.Abstractions;
using Querify.Tenant.Portal.Business.Billing.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Querify.Tenant.Portal.Test.IntegrationTests.Tests.Billing;

public class BillingControllerTests
{
    [Fact]
    public async Task GetSummary_UsesTenantIdFromHeader()
    {
        var tenantId = Guid.NewGuid();
        var service = new RecordingBillingPortalService();
        var controller = CreateController(service, tenantId);

        var response = await controller.GetSummary(CancellationToken.None);

        var result = Assert.IsType<OkObjectResult>(response);
        var payload = Assert.IsType<TenantBillingSummaryDto>(result.Value);
        Assert.Equal(tenantId, service.SummaryTenantId);
        Assert.Equal(tenantId, payload.TenantId);
    }

    [Fact]
    public async Task GetSubscription_UsesTenantIdFromHeader()
    {
        var tenantId = Guid.NewGuid();
        var service = new RecordingBillingPortalService();
        var controller = CreateController(service, tenantId);

        var response = await controller.GetSubscription(CancellationToken.None);

        var result = Assert.IsType<OkObjectResult>(response);
        var payload = Assert.IsType<TenantSubscriptionDetailDto>(result.Value);
        Assert.Equal(tenantId, service.SubscriptionTenantId);
        Assert.Equal(tenantId, payload.TenantId);
    }

    [Fact]
    public async Task GetInvoices_UsesTenantIdFromHeader()
    {
        var tenantId = Guid.NewGuid();
        var service = new RecordingBillingPortalService();
        var controller = CreateController(service, tenantId);
        var request = new BillingInvoiceGetAllRequestDto();

        var response = await controller.GetInvoices(request, CancellationToken.None);

        var result = Assert.IsType<OkObjectResult>(response);
        _ = Assert.IsType<PagedResultDto<BillingInvoiceDto>>(result.Value);
        Assert.Equal(tenantId, service.InvoicesTenantId);
        Assert.Same(request, service.InvoicesRequest);
    }

    [Fact]
    public async Task GetPayments_UsesTenantIdFromHeader()
    {
        var tenantId = Guid.NewGuid();
        var service = new RecordingBillingPortalService();
        var controller = CreateController(service, tenantId);
        var request = new BillingPaymentGetAllRequestDto();

        var response = await controller.GetPayments(request, CancellationToken.None);

        var result = Assert.IsType<OkObjectResult>(response);
        _ = Assert.IsType<PagedResultDto<BillingPaymentDto>>(result.Value);
        Assert.Equal(tenantId, service.PaymentsTenantId);
        Assert.Same(request, service.PaymentsRequest);
    }

    [Fact]
    public async Task GetSummary_WithoutTenantHeader_ThrowsBadRequest()
    {
        var service = new RecordingBillingPortalService();
        var controller = new BillingController(service)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        var exception = await Assert.ThrowsAsync<ApiErrorException>(() => controller.GetSummary(CancellationToken.None));

        Assert.Equal(StatusCodes.Status400BadRequest, exception.ErrorCode);
        Assert.Equal($"Missing required header '{TenantResolutionMiddleware.TenantHeaderName}'.", exception.Message);
    }

    private static BillingController CreateController(IBillingPortalService service, Guid tenantId)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers[TenantResolutionMiddleware.TenantHeaderName] = tenantId.ToString();

        return new BillingController(service)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            }
        };
    }

    private sealed class RecordingBillingPortalService : IBillingPortalService
    {
        public Guid SummaryTenantId { get; private set; }
        public Guid SubscriptionTenantId { get; private set; }
        public Guid InvoicesTenantId { get; private set; }
        public Guid PaymentsTenantId { get; private set; }
        public BillingInvoiceGetAllRequestDto? InvoicesRequest { get; private set; }
        public BillingPaymentGetAllRequestDto? PaymentsRequest { get; private set; }

        public Task<TenantBillingSummaryDto> GetSummary(Guid tenantId, CancellationToken cancellationToken)
        {
            SummaryTenantId = tenantId;
            return Task.FromResult(new TenantBillingSummaryDto { TenantId = tenantId });
        }

        public Task<TenantSubscriptionDetailDto> GetSubscription(Guid tenantId, CancellationToken cancellationToken)
        {
            SubscriptionTenantId = tenantId;
            return Task.FromResult(new TenantSubscriptionDetailDto { TenantId = tenantId });
        }

        public Task<PagedResultDto<BillingInvoiceDto>> GetInvoices(
            Guid tenantId,
            BillingInvoiceGetAllRequestDto requestDto,
            CancellationToken cancellationToken)
        {
            InvoicesTenantId = tenantId;
            InvoicesRequest = requestDto;
            return Task.FromResult(new PagedResultDto<BillingInvoiceDto>());
        }

        public Task<PagedResultDto<BillingPaymentDto>> GetPayments(
            Guid tenantId,
            BillingPaymentGetAllRequestDto requestDto,
            CancellationToken cancellationToken)
        {
            PaymentsTenantId = tenantId;
            PaymentsRequest = requestDto;
            return Task.FromResult(new PagedResultDto<BillingPaymentDto>());
        }
    }
}
