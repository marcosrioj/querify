using Querify.Common.EntityFramework.Tenant;
using Querify.Common.EntityFramework.Tenant.Entities;
using Querify.Common.EntityFramework.Tenant.Enums;
using Querify.Models.Common.Dtos;
using Querify.Models.Tenant.Dtos.Billing;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Querify.Tenant.BackOffice.Business.Billing.Queries.GetBillingWebhookInboxList;

public sealed class GetBillingWebhookInboxListQueryHandler(TenantDbContext dbContext)
    : IRequestHandler<GetBillingWebhookInboxListQuery, PagedResultDto<BillingWebhookInboxDto>>
{
    public async Task<PagedResultDto<BillingWebhookInboxDto>> Handle(
        GetBillingWebhookInboxListQuery request,
        CancellationToken cancellationToken)
    {
        var query = dbContext.BillingWebhookInboxes.AsNoTracking();

        if (request.Request.TenantId.HasValue)
        {
            query = query.Where(entry => entry.TenantId == request.Request.TenantId.Value);
        }

        if (request.Request.Provider.HasValue)
        {
            query = query.Where(entry => entry.Provider == request.Request.Provider.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Request.EventType))
        {
            query = query.Where(entry => entry.EventType == request.Request.EventType);
        }

        if (request.Request.ReceivedFromUtc.HasValue)
        {
            query = query.Where(entry => entry.ReceivedDateUtc >= request.Request.ReceivedFromUtc.Value);
        }

        if (request.Request.ReceivedToUtc.HasValue)
        {
            query = query.Where(entry => entry.ReceivedDateUtc <= request.Request.ReceivedToUtc.Value);
        }

        if (Enum.TryParse<ControlPlaneMessageStatus>(request.Request.Status, true, out var status))
        {
            query = query.Where(entry => entry.Status == status);
        }

        query = ApplySorting(query, request.Request.Sorting);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip(request.Request.SkipCount)
            .Take(request.Request.MaxResultCount)
            .Select(entry => new BillingWebhookInboxDto
            {
                Id = entry.Id,
                TenantId = entry.TenantId,
                Provider = entry.Provider,
                ExternalEventId = entry.ExternalEventId,
                EventType = entry.EventType,
                SignatureValid = entry.SignatureValid,
                IsLiveMode = entry.IsLiveMode,
                ProviderAccountId = entry.ProviderAccountId,
                Status = entry.Status.ToString(),
                AttemptCount = entry.AttemptCount,
                ReceivedDateUtc = entry.ReceivedDateUtc,
                EventCreatedAtUtc = entry.EventCreatedAtUtc,
                LastAttemptDateUtc = entry.LastAttemptDateUtc,
                NextAttemptDateUtc = entry.NextAttemptDateUtc,
                ProcessedDateUtc = entry.ProcessedDateUtc,
                LastError = entry.LastError
            })
            .ToListAsync(cancellationToken);

        return new PagedResultDto<BillingWebhookInboxDto>(totalCount, items);
    }

    private static IQueryable<BillingWebhookInbox> ApplySorting(
        IQueryable<BillingWebhookInbox> query,
        string? sorting)
    {
        return sorting?.Trim().ToLowerInvariant() switch
        {
            "attemptcount asc" => query.OrderBy(entry => entry.AttemptCount).ThenByDescending(entry => entry.ReceivedDateUtc),
            "attemptcount desc" => query.OrderByDescending(entry => entry.AttemptCount).ThenByDescending(entry => entry.ReceivedDateUtc),
            "processeddateutc asc" => query.OrderBy(entry => entry.ProcessedDateUtc).ThenByDescending(entry => entry.ReceivedDateUtc),
            "processeddateutc desc" => query.OrderByDescending(entry => entry.ProcessedDateUtc).ThenByDescending(entry => entry.ReceivedDateUtc),
            _ => query.OrderByDescending(entry => entry.ReceivedDateUtc)
        };
    }
}
