using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Common.EntityFramework.Tenant.Entities;
using BaseFaq.Models.Tenant.Enums;
using BaseFaq.Tenant.Worker.Business.Billing.Models;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Tenant.Worker.Business.Billing.Services;

public sealed class BillingStateService(
    TenantDbContext dbContext,
    BillingTenantResolver billingTenantResolver)
{
    public async Task<Guid> ResolveRequiredTenantIdAsync(
        BillingWebhookEvent billingEvent,
        CancellationToken cancellationToken = default)
    {
        var tenantId = await billingTenantResolver.ResolveTenantIdAsync(billingEvent, cancellationToken);
        if (tenantId.HasValue)
        {
            return tenantId.Value;
        }

        throw new InvalidOperationException(
            $"Tenant could not be resolved for billing event '{billingEvent.ExternalEventId}' ({billingEvent.EventType}).");
    }

    public async Task<BillingCustomer?> UpsertCustomerAsync(
        BillingWebhookEvent billingEvent,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(billingEvent.ExternalCustomerId))
        {
            return null;
        }

        var customer = await dbContext.BillingCustomers
            .FirstOrDefaultAsync(
                entry =>
                    entry.Provider == billingEvent.Provider &&
                    entry.ExternalCustomerId == billingEvent.ExternalCustomerId,
                cancellationToken);

        if (customer is null)
        {
            customer = new BillingCustomer
            {
                TenantId = tenantId,
                Provider = billingEvent.Provider,
                ExternalCustomerId = billingEvent.ExternalCustomerId
            };

            await dbContext.BillingCustomers.AddAsync(customer, cancellationToken);
        }

        customer.TenantId = tenantId;

        if (ShouldApplyEvent(customer.LastEventCreatedAtUtc, billingEvent.EventCreatedAtUtc))
        {
            customer.Email = FirstNonEmpty(billingEvent.CustomerEmail, customer.Email);
            customer.CountryCode = FirstNonEmpty(billingEvent.CountryCode, customer.CountryCode);
            customer.LastEventCreatedAtUtc = MaxDate(customer.LastEventCreatedAtUtc, billingEvent.EventCreatedAtUtc);
        }
        else
        {
            customer.Email ??= billingEvent.CustomerEmail;
            customer.CountryCode ??= billingEvent.CountryCode;
        }

        return customer;
    }

    public async Task<TenantSubscription> UpsertTenantSubscriptionAsync(
        BillingWebhookEvent billingEvent,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var subscription = await dbContext.TenantSubscriptions
            .FirstOrDefaultAsync(entry => entry.TenantId == tenantId, cancellationToken);

        if (subscription is null)
        {
            subscription = new TenantSubscription
            {
                TenantId = tenantId
            };

            await dbContext.TenantSubscriptions.AddAsync(subscription, cancellationToken);
        }

        subscription.TenantId = tenantId;
        subscription.DefaultProvider = billingEvent.Provider;

        if (ShouldApplyEvent(subscription.LastEventCreatedAtUtc, billingEvent.EventCreatedAtUtc))
        {
            subscription.PlanCode = FirstNonEmpty(billingEvent.PlanCode, subscription.PlanCode);
            subscription.BillingInterval = billingEvent.BillingInterval != BillingIntervalType.Unknown
                ? billingEvent.BillingInterval
                : subscription.BillingInterval;
            subscription.Status = billingEvent.SubscriptionStatus != TenantSubscriptionStatus.Unknown
                ? billingEvent.SubscriptionStatus
                : subscription.Status;
            subscription.Currency = FirstNonEmpty(billingEvent.Currency, subscription.Currency);
            subscription.CountryCode = FirstNonEmpty(billingEvent.CountryCode, subscription.CountryCode);
            subscription.TrialEndsAtUtc = CoalesceDate(billingEvent.TrialEndsAtUtc, subscription.TrialEndsAtUtc);
            subscription.CurrentPeriodStartUtc = CoalesceDate(
                billingEvent.CurrentPeriodStartUtc,
                subscription.CurrentPeriodStartUtc);
            subscription.CurrentPeriodEndUtc = CoalesceDate(
                billingEvent.CurrentPeriodEndUtc,
                subscription.CurrentPeriodEndUtc);
            subscription.CancelAtPeriodEnd = billingEvent.CancelAtPeriodEnd;
            subscription.CancelledAtUtc = CoalesceDate(billingEvent.CancelledAtUtc, subscription.CancelledAtUtc);
            subscription.GraceUntilUtc = ResolveGraceUntilUtc(subscription, billingEvent);
            subscription.LastEventCreatedAtUtc = MaxDate(
                subscription.LastEventCreatedAtUtc,
                billingEvent.EventCreatedAtUtc);
        }
        else
        {
            subscription.PlanCode ??= billingEvent.PlanCode;
            subscription.Currency ??= billingEvent.Currency;
            subscription.CountryCode ??= billingEvent.CountryCode;
        }

        return subscription;
    }

    public async Task<BillingProviderSubscription?> UpsertProviderSubscriptionAsync(
        BillingWebhookEvent billingEvent,
        Guid tenantId,
        Guid tenantSubscriptionId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(billingEvent.ExternalSubscriptionId))
        {
            return null;
        }

        var providerSubscription = await dbContext.BillingProviderSubscriptions
            .FirstOrDefaultAsync(
                entry =>
                    entry.Provider == billingEvent.Provider &&
                    entry.ExternalSubscriptionId == billingEvent.ExternalSubscriptionId,
                cancellationToken);

        if (providerSubscription is null)
        {
            providerSubscription = new BillingProviderSubscription
            {
                TenantId = tenantId,
                TenantSubscriptionId = tenantSubscriptionId,
                Provider = billingEvent.Provider,
                ExternalSubscriptionId = billingEvent.ExternalSubscriptionId
            };

            await dbContext.BillingProviderSubscriptions.AddAsync(providerSubscription, cancellationToken);
        }

        providerSubscription.TenantId = tenantId;
        providerSubscription.TenantSubscriptionId = tenantSubscriptionId;

        if (ShouldApplyEvent(providerSubscription.LastEventCreatedAtUtc, billingEvent.EventCreatedAtUtc))
        {
            providerSubscription.ExternalPriceId = FirstNonEmpty(
                billingEvent.ExternalPriceId,
                providerSubscription.ExternalPriceId);
            providerSubscription.ExternalProductId = FirstNonEmpty(
                billingEvent.ExternalProductId,
                providerSubscription.ExternalProductId);
            providerSubscription.Status = billingEvent.SubscriptionStatus != TenantSubscriptionStatus.Unknown
                ? billingEvent.SubscriptionStatus
                : providerSubscription.Status;
            providerSubscription.CurrentPeriodStartUtc = CoalesceDate(
                billingEvent.CurrentPeriodStartUtc,
                providerSubscription.CurrentPeriodStartUtc);
            providerSubscription.CurrentPeriodEndUtc = CoalesceDate(
                billingEvent.CurrentPeriodEndUtc,
                providerSubscription.CurrentPeriodEndUtc);
            providerSubscription.TrialEndsAtUtc = CoalesceDate(
                billingEvent.TrialEndsAtUtc,
                providerSubscription.TrialEndsAtUtc);
            providerSubscription.CancelAtPeriodEnd = billingEvent.CancelAtPeriodEnd;
            providerSubscription.CancelledAtUtc = CoalesceDate(
                billingEvent.CancelledAtUtc,
                providerSubscription.CancelledAtUtc);
            providerSubscription.RawSnapshotJson = billingEvent.RawObjectJson;
            providerSubscription.LastEventCreatedAtUtc = MaxDate(
                providerSubscription.LastEventCreatedAtUtc,
                billingEvent.EventCreatedAtUtc);
        }
        else
        {
            providerSubscription.ExternalPriceId ??= billingEvent.ExternalPriceId;
            providerSubscription.ExternalProductId ??= billingEvent.ExternalProductId;
        }

        return providerSubscription;
    }

    public async Task<BillingInvoice?> UpsertInvoiceAsync(
        BillingWebhookEvent billingEvent,
        Guid tenantId,
        Guid? tenantSubscriptionId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(billingEvent.ExternalInvoiceId))
        {
            return null;
        }

        var invoice = await dbContext.BillingInvoices
            .FirstOrDefaultAsync(
                entry =>
                    entry.Provider == billingEvent.Provider &&
                    entry.ExternalInvoiceId == billingEvent.ExternalInvoiceId,
                cancellationToken);

        if (invoice is null)
        {
            invoice = new BillingInvoice
            {
                TenantId = tenantId,
                Provider = billingEvent.Provider,
                ExternalInvoiceId = billingEvent.ExternalInvoiceId
            };

            await dbContext.BillingInvoices.AddAsync(invoice, cancellationToken);
        }

        invoice.TenantId = tenantId;
        invoice.TenantSubscriptionId = tenantSubscriptionId ?? invoice.TenantSubscriptionId;

        if (ShouldApplyEvent(invoice.LastEventCreatedAtUtc, billingEvent.EventCreatedAtUtc))
        {
            invoice.AmountMinor = billingEvent.AmountMinor ?? invoice.AmountMinor;
            invoice.Currency = FirstNonEmpty(billingEvent.Currency, invoice.Currency) ?? string.Empty;
            invoice.DueDateUtc = CoalesceDate(billingEvent.DueDateUtc, invoice.DueDateUtc);
            invoice.PaidAtUtc = CoalesceDate(billingEvent.PaidAtUtc, invoice.PaidAtUtc);
            invoice.Status = billingEvent.InvoiceStatus != BillingInvoiceStatus.Unknown
                ? billingEvent.InvoiceStatus
                : invoice.Status;
            invoice.HostedUrl = FirstNonEmpty(billingEvent.HostedInvoiceUrl, invoice.HostedUrl);
            invoice.PdfUrl = FirstNonEmpty(billingEvent.PdfUrl, invoice.PdfUrl);
            invoice.RawSnapshotJson = billingEvent.RawObjectJson;
            invoice.LastEventCreatedAtUtc = MaxDate(invoice.LastEventCreatedAtUtc, billingEvent.EventCreatedAtUtc);
        }
        else if (string.IsNullOrWhiteSpace(invoice.Currency))
        {
            invoice.Currency = billingEvent.Currency ?? invoice.Currency;
        }

        return invoice;
    }

    public async Task<BillingPayment?> UpsertPaymentAsync(
        BillingWebhookEvent billingEvent,
        Guid tenantId,
        Guid? billingInvoiceId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(billingEvent.ExternalPaymentId))
        {
            return null;
        }

        var payment = await dbContext.BillingPayments
            .FirstOrDefaultAsync(
                entry =>
                    entry.Provider == billingEvent.Provider &&
                    entry.ExternalPaymentId == billingEvent.ExternalPaymentId,
                cancellationToken);

        if (payment is null)
        {
            payment = new BillingPayment
            {
                TenantId = tenantId,
                BillingInvoiceId = billingInvoiceId,
                Provider = billingEvent.Provider,
                ExternalPaymentId = billingEvent.ExternalPaymentId
            };

            await dbContext.BillingPayments.AddAsync(payment, cancellationToken);
        }

        payment.TenantId = tenantId;
        payment.BillingInvoiceId = billingInvoiceId ?? payment.BillingInvoiceId;

        if (ShouldApplyEvent(payment.LastEventCreatedAtUtc, billingEvent.EventCreatedAtUtc))
        {
            payment.Method = FirstNonEmpty(billingEvent.PaymentMethod, payment.Method);
            payment.AmountMinor = billingEvent.AmountMinor ?? payment.AmountMinor;
            payment.Currency = FirstNonEmpty(billingEvent.Currency, payment.Currency) ?? string.Empty;
            payment.Status = billingEvent.PaymentStatus != BillingPaymentStatus.Unknown
                ? billingEvent.PaymentStatus
                : payment.Status;
            payment.FailureCode = FirstNonEmpty(billingEvent.FailureCode, payment.FailureCode);
            payment.FailureMessage = FirstNonEmpty(billingEvent.FailureMessage, payment.FailureMessage);
            payment.PaidAtUtc = CoalesceDate(billingEvent.PaidAtUtc, payment.PaidAtUtc);
            payment.RawSnapshotJson = billingEvent.RawObjectJson;
            payment.LastEventCreatedAtUtc = MaxDate(payment.LastEventCreatedAtUtc, billingEvent.EventCreatedAtUtc);
        }
        else if (string.IsNullOrWhiteSpace(payment.Currency))
        {
            payment.Currency = billingEvent.Currency ?? payment.Currency;
        }

        return payment;
    }

    private static bool ShouldApplyEvent(DateTime? currentEventCreatedAtUtc, DateTime? incomingEventCreatedAtUtc)
    {
        if (!currentEventCreatedAtUtc.HasValue || !incomingEventCreatedAtUtc.HasValue)
        {
            return true;
        }

        return incomingEventCreatedAtUtc.Value >= currentEventCreatedAtUtc.Value;
    }

    private static DateTime? ResolveGraceUntilUtc(TenantSubscription subscription, BillingWebhookEvent billingEvent)
    {
        return billingEvent.SubscriptionStatus switch
        {
            TenantSubscriptionStatus.PastDue => billingEvent.CurrentPeriodEndUtc ??
                                                billingEvent.DueDateUtc ??
                                                subscription.GraceUntilUtc,
            TenantSubscriptionStatus.Unpaid => billingEvent.CurrentPeriodEndUtc ??
                                               billingEvent.DueDateUtc ??
                                               subscription.GraceUntilUtc,
            TenantSubscriptionStatus.Active => null,
            TenantSubscriptionStatus.Trialing => null,
            _ => subscription.GraceUntilUtc
        };
    }

    private static string? FirstNonEmpty(string? first, string? fallback)
    {
        return !string.IsNullOrWhiteSpace(first) ? first : fallback;
    }

    private static DateTime? CoalesceDate(DateTime? first, DateTime? fallback)
    {
        return first ?? fallback;
    }

    private static DateTime? MaxDate(DateTime? left, DateTime? right)
    {
        if (!left.HasValue)
        {
            return right;
        }

        if (!right.HasValue)
        {
            return left;
        }

        return left.Value >= right.Value ? left : right;
    }
}
