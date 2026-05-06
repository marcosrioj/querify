using Querify.Common.EntityFramework.Tenant;
using Querify.Common.EntityFramework.Tenant.Entities;
using Querify.Models.Common.Dtos;
using Querify.Models.Tenant.Dtos.Billing;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Querify.Tenant.Portal.Business.Billing.Queries.GetBillingInvoices;

public sealed class GetBillingInvoicesQueryHandler(TenantDbContext dbContext)
    : IRequestHandler<GetBillingInvoicesQuery, PagedResultDto<BillingInvoiceDto>>
{
    public async Task<PagedResultDto<BillingInvoiceDto>> Handle(
        GetBillingInvoicesQuery request,
        CancellationToken cancellationToken)
    {
        var query = dbContext.BillingInvoices
            .AsNoTracking()
            .Where(entry => entry.TenantId == request.TenantId);

        query = ApplySorting(query, request.Request.Sorting);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip(request.Request.SkipCount)
            .Take(request.Request.MaxResultCount)
            .Select(invoice => new BillingInvoiceDto
            {
                Id = invoice.Id,
                TenantId = invoice.TenantId,
                TenantSubscriptionId = invoice.TenantSubscriptionId,
                Provider = invoice.Provider,
                ExternalInvoiceId = invoice.ExternalInvoiceId,
                AmountMinor = invoice.AmountMinor,
                Currency = invoice.Currency,
                DueDateUtc = invoice.DueDateUtc,
                PaidAtUtc = invoice.PaidAtUtc,
                Status = invoice.Status,
                HostedUrl = invoice.HostedUrl,
                PdfUrl = invoice.PdfUrl,
                CreatedDateUtc = invoice.CreatedDate,
                UpdatedDateUtc = invoice.UpdatedDate
            })
            .ToListAsync(cancellationToken);

        return new PagedResultDto<BillingInvoiceDto>(
            totalCount,
            items);
    }

    private static IQueryable<BillingInvoice> ApplySorting(IQueryable<BillingInvoice> query, string? sorting)
    {
        return sorting?.Trim().ToLowerInvariant() switch
        {
            "duedateutc asc" => query.OrderBy(entry => entry.DueDateUtc).ThenBy(entry => entry.CreatedDate),
            "duedateutc desc" => query.OrderByDescending(entry => entry.DueDateUtc).ThenByDescending(entry => entry.CreatedDate),
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
