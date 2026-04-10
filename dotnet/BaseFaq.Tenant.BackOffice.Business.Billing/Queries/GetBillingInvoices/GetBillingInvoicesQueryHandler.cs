using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Common.EntityFramework.Tenant.Entities;
using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Tenant.Dtos.Billing;
using BaseFaq.Tenant.BackOffice.Business.Billing.Service;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Tenant.BackOffice.Business.Billing.Queries.GetBillingInvoices;

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
            .ToListAsync(cancellationToken);

        return new PagedResultDto<BillingInvoiceDto>(
            totalCount,
            items.Select(BillingDtoMapper.ToInvoiceDto).ToList());
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
