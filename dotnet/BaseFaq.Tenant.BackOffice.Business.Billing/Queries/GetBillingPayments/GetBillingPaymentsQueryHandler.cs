using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Common.EntityFramework.Tenant.Entities;
using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Tenant.Dtos.Billing;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Tenant.BackOffice.Business.Billing.Queries.GetBillingPayments;

public sealed class GetBillingPaymentsQueryHandler(TenantDbContext dbContext)
    : IRequestHandler<GetBillingPaymentsQuery, PagedResultDto<BillingPaymentDto>>
{
    public async Task<PagedResultDto<BillingPaymentDto>> Handle(
        GetBillingPaymentsQuery request,
        CancellationToken cancellationToken)
    {
        var query = dbContext.BillingPayments
            .AsNoTracking()
            .Where(entry => entry.TenantId == request.TenantId);

        query = ApplySorting(query, request.Request.Sorting);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip(request.Request.SkipCount)
            .Take(request.Request.MaxResultCount)
            .Select(payment => new BillingPaymentDto
            {
                Id = payment.Id,
                TenantId = payment.TenantId,
                BillingInvoiceId = payment.BillingInvoiceId,
                Provider = payment.Provider,
                ExternalPaymentId = payment.ExternalPaymentId,
                Method = payment.Method,
                AmountMinor = payment.AmountMinor,
                Currency = payment.Currency,
                Status = payment.Status,
                FailureCode = payment.FailureCode,
                FailureMessage = payment.FailureMessage,
                PaidAtUtc = payment.PaidAtUtc,
                CreatedDateUtc = payment.CreatedDate,
                UpdatedDateUtc = payment.UpdatedDate
            })
            .ToListAsync(cancellationToken);

        return new PagedResultDto<BillingPaymentDto>(
            totalCount,
            items);
    }

    private static IQueryable<BillingPayment> ApplySorting(IQueryable<BillingPayment> query, string? sorting)
    {
        return sorting?.Trim().ToLowerInvariant() switch
        {
            "paidatutc asc" => query.OrderBy(entry => entry.PaidAtUtc).ThenBy(entry => entry.CreatedDate),
            "paidatutc desc" => query.OrderByDescending(entry => entry.PaidAtUtc).ThenByDescending(entry => entry.CreatedDate),
            "amountminor asc" => query.OrderBy(entry => entry.AmountMinor).ThenBy(entry => entry.CreatedDate),
            "amountminor desc" => query.OrderByDescending(entry => entry.AmountMinor).ThenByDescending(entry => entry.CreatedDate),
            "status asc" => query.OrderBy(entry => entry.Status).ThenByDescending(entry => entry.UpdatedDate),
            "status desc" => query.OrderByDescending(entry => entry.Status).ThenByDescending(entry => entry.UpdatedDate),
            _ => query.OrderByDescending(entry => entry.UpdatedDate ?? entry.CreatedDate)
        };
    }
}
