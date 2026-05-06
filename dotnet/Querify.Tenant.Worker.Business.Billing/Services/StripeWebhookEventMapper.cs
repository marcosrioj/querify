using System.Text.Json;
using Querify.Common.EntityFramework.Tenant.Entities;
using Querify.Models.Tenant.Enums;
using Querify.Tenant.Worker.Business.Billing.Models;
using Stripe;

namespace Querify.Tenant.Worker.Business.Billing.Services;

public sealed class StripeWebhookEventMapper
{
    public BillingWebhookEvent Map(BillingWebhookInbox workItem)
    {
        ArgumentNullException.ThrowIfNull(workItem);

        _ = EventUtility.ParseEvent(workItem.PayloadJson);

        using var payloadDocument = JsonDocument.Parse(workItem.PayloadJson);
        var payloadRoot = payloadDocument.RootElement;
        var dataObject = GetDataObject(payloadRoot);
        var kind = MapKind(workItem.EventType);
        var rawObjectJson = dataObject.ValueKind == JsonValueKind.Undefined
            ? workItem.PayloadJson
            : dataObject.GetRawText();

        return kind switch
        {
            BillingWebhookEventKind.CheckoutCompleted => BuildCheckoutCompleted(workItem, dataObject, rawObjectJson),
            BillingWebhookEventKind.SubscriptionCreated => BuildSubscriptionEvent(
                workItem,
                dataObject,
                rawObjectJson,
                BillingWebhookEventKind.SubscriptionCreated),
            BillingWebhookEventKind.SubscriptionUpdated => BuildSubscriptionEvent(
                workItem,
                dataObject,
                rawObjectJson,
                BillingWebhookEventKind.SubscriptionUpdated),
            BillingWebhookEventKind.SubscriptionCanceled => BuildSubscriptionEvent(
                workItem,
                dataObject,
                rawObjectJson,
                BillingWebhookEventKind.SubscriptionCanceled),
            BillingWebhookEventKind.InvoicePaid => BuildInvoiceEvent(
                workItem,
                dataObject,
                rawObjectJson,
                BillingWebhookEventKind.InvoicePaid),
            BillingWebhookEventKind.InvoicePaymentFailed => BuildInvoiceEvent(
                workItem,
                dataObject,
                rawObjectJson,
                BillingWebhookEventKind.InvoicePaymentFailed),
            _ => BuildUnknown(workItem, rawObjectJson)
        };
    }

    private static BillingWebhookEvent BuildCheckoutCompleted(
        BillingWebhookInbox workItem,
        JsonElement dataObject,
        string rawObjectJson)
    {
        var paymentStatus = GetString(dataObject, "payment_status");

        return CreateBaseEvent(workItem, BillingWebhookEventKind.CheckoutCompleted, rawObjectJson) with
        {
            ExternalCheckoutSessionId = GetString(dataObject, "id"),
            ExternalCustomerId = GetString(dataObject, "customer"),
            CustomerEmail = FirstNonEmpty(
                GetString(dataObject, "customer_email"),
                GetNestedString(dataObject, "customer_details", "email")),
            ExternalSubscriptionId = GetString(dataObject, "subscription"),
            PlanCode = GetMetadataValue(dataObject, "plan_code"),
            BillingInterval = MapBillingInterval(GetMetadataValue(dataObject, "billing_interval")),
            SubscriptionStatus = MapCheckoutSubscriptionStatus(paymentStatus),
            Currency = NormalizeCurrency(GetString(dataObject, "currency")),
            CountryCode = NormalizeCountryCode(GetNestedString(dataObject, "customer_details", "address", "country"))
        };
    }

    private static BillingWebhookEvent BuildSubscriptionEvent(
        BillingWebhookInbox workItem,
        JsonElement dataObject,
        string rawObjectJson,
        BillingWebhookEventKind kind)
    {
        var firstPrice = GetFirstArrayItemNestedObject(dataObject, "items", "data", "price");

        return CreateBaseEvent(workItem, kind, rawObjectJson) with
        {
            TenantId = TryGetTenantId(dataObject),
            ExternalCustomerId = GetString(dataObject, "customer"),
            ExternalSubscriptionId = GetString(dataObject, "id"),
            ExternalPriceId = GetString(firstPrice, "id"),
            ExternalProductId = GetString(firstPrice, "product"),
            PlanCode = ResolvePlanCode(dataObject, firstPrice),
            BillingInterval = MapBillingInterval(GetNestedString(firstPrice, "recurring", "interval")),
            SubscriptionStatus = kind == BillingWebhookEventKind.SubscriptionCanceled
                ? TenantSubscriptionStatus.Canceled
                : MapSubscriptionStatus(GetString(dataObject, "status")),
            Currency = NormalizeCurrency(FirstNonEmpty(
                GetString(dataObject, "currency"),
                GetString(firstPrice, "currency"))),
            TrialEndsAtUtc = GetUnixDateTimeUtc(dataObject, "trial_end"),
            CurrentPeriodStartUtc = GetUnixDateTimeUtc(dataObject, "current_period_start"),
            CurrentPeriodEndUtc = GetUnixDateTimeUtc(dataObject, "current_period_end"),
            CancelAtPeriodEnd = GetBoolean(dataObject, "cancel_at_period_end") ?? false,
            CancelledAtUtc = GetUnixDateTimeUtc(dataObject, "canceled_at")
        };
    }

    private static BillingWebhookEvent BuildInvoiceEvent(
        BillingWebhookInbox workItem,
        JsonElement dataObject,
        string rawObjectJson,
        BillingWebhookEventKind kind)
    {
        var firstPrice = GetFirstArrayItemNestedObject(dataObject, "lines", "data", "price");
        var amountMinor = kind == BillingWebhookEventKind.InvoicePaid
            ? GetLong(dataObject, "amount_paid") ?? GetLong(dataObject, "amount_due")
            : GetLong(dataObject, "amount_due");

        return CreateBaseEvent(workItem, kind, rawObjectJson) with
        {
            TenantId = TryGetTenantId(dataObject),
            ExternalCustomerId = GetString(dataObject, "customer"),
            CustomerEmail = FirstNonEmpty(
                GetString(dataObject, "customer_email"),
                GetNestedString(dataObject, "customer_details", "email")),
            ExternalSubscriptionId = GetString(dataObject, "subscription"),
            ExternalInvoiceId = GetString(dataObject, "id"),
            ExternalPaymentId = FirstNonEmpty(
                GetString(dataObject, "payment_intent"),
                GetString(dataObject, "charge")),
            ExternalPriceId = GetString(firstPrice, "id"),
            ExternalProductId = GetString(firstPrice, "product"),
            PlanCode = ResolvePlanCode(dataObject, firstPrice),
            BillingInterval = MapBillingInterval(GetNestedString(firstPrice, "recurring", "interval")),
            SubscriptionStatus = kind == BillingWebhookEventKind.InvoicePaid
                ? TenantSubscriptionStatus.Active
                : TenantSubscriptionStatus.PastDue,
            InvoiceStatus = kind == BillingWebhookEventKind.InvoicePaid
                ? BillingInvoiceStatus.Paid
                : BillingInvoiceStatus.Failed,
            PaymentStatus = kind == BillingWebhookEventKind.InvoicePaid
                ? BillingPaymentStatus.Succeeded
                : BillingPaymentStatus.Failed,
            Currency = NormalizeCurrency(GetString(dataObject, "currency")),
            CountryCode = NormalizeCountryCode(GetNestedString(dataObject, "customer_address", "country")),
            AmountMinor = amountMinor,
            DueDateUtc = GetUnixDateTimeUtc(dataObject, "due_date"),
            PaidAtUtc = CoalesceDate(
                GetNestedUnixDateTimeUtc(dataObject, "status_transitions", "paid_at"),
                kind == BillingWebhookEventKind.InvoicePaid ? workItem.EventCreatedAtUtc : null),
            HostedInvoiceUrl = GetString(dataObject, "hosted_invoice_url"),
            PdfUrl = GetString(dataObject, "invoice_pdf"),
            PaymentMethod = FirstNonEmpty(
                GetNestedString(dataObject, "payment_settings", "default_payment_method"),
                GetString(dataObject, "default_payment_method")),
            FailureCode = GetNestedString(dataObject, "last_payment_error", "code"),
            FailureMessage = GetNestedString(dataObject, "last_payment_error", "message")
        };
    }

    private static BillingWebhookEvent BuildUnknown(BillingWebhookInbox workItem, string rawObjectJson)
    {
        return CreateBaseEvent(workItem, BillingWebhookEventKind.Unknown, rawObjectJson);
    }

    private static BillingWebhookEvent CreateBaseEvent(
        BillingWebhookInbox workItem,
        BillingWebhookEventKind kind,
        string rawObjectJson)
    {
        return new BillingWebhookEvent
        {
            InboxId = workItem.Id,
            Provider = workItem.Provider,
            Kind = kind,
            ExternalEventId = workItem.ExternalEventId,
            EventType = workItem.EventType,
            TenantId = workItem.TenantId,
            EventCreatedAtUtc = workItem.EventCreatedAtUtc,
            IsLiveMode = workItem.IsLiveMode,
            ProviderAccountId = workItem.ProviderAccountId,
            RawPayloadJson = workItem.PayloadJson,
            RawObjectJson = rawObjectJson
        };
    }

    private static BillingWebhookEventKind MapKind(string eventType)
    {
        return eventType switch
        {
            "checkout.session.completed" => BillingWebhookEventKind.CheckoutCompleted,
            "customer.subscription.created" => BillingWebhookEventKind.SubscriptionCreated,
            "customer.subscription.updated" => BillingWebhookEventKind.SubscriptionUpdated,
            "customer.subscription.deleted" => BillingWebhookEventKind.SubscriptionCanceled,
            "invoice.paid" => BillingWebhookEventKind.InvoicePaid,
            "invoice.payment_failed" => BillingWebhookEventKind.InvoicePaymentFailed,
            _ => BillingWebhookEventKind.Unknown
        };
    }

    private static TenantSubscriptionStatus MapCheckoutSubscriptionStatus(string? paymentStatus)
    {
        return paymentStatus switch
        {
            "paid" => TenantSubscriptionStatus.Active,
            "no_payment_required" => TenantSubscriptionStatus.Trialing,
            _ => TenantSubscriptionStatus.Unknown
        };
    }

    private static TenantSubscriptionStatus MapSubscriptionStatus(string? status)
    {
        return status?.ToLowerInvariant() switch
        {
            "trialing" => TenantSubscriptionStatus.Trialing,
            "active" => TenantSubscriptionStatus.Active,
            "past_due" => TenantSubscriptionStatus.PastDue,
            "unpaid" => TenantSubscriptionStatus.Unpaid,
            "canceled" => TenantSubscriptionStatus.Canceled,
            "incomplete" => TenantSubscriptionStatus.Incomplete,
            "incomplete_expired" => TenantSubscriptionStatus.IncompleteExpired,
            "paused" => TenantSubscriptionStatus.Paused,
            _ => TenantSubscriptionStatus.Unknown
        };
    }

    private static BillingIntervalType MapBillingInterval(string? interval)
    {
        return interval?.ToLowerInvariant() switch
        {
            "day" => BillingIntervalType.Day,
            "week" => BillingIntervalType.Week,
            "month" => BillingIntervalType.Month,
            "year" => BillingIntervalType.Year,
            _ => BillingIntervalType.Unknown
        };
    }

    private static string? ResolvePlanCode(JsonElement sourceObject, JsonElement priceObject)
    {
        return FirstNonEmpty(
            GetMetadataValue(sourceObject, "plan_code"),
            FirstNonEmpty(
                GetMetadataValue(priceObject, "plan_code"),
                FirstNonEmpty(GetString(priceObject, "lookup_key"), GetString(priceObject, "id"))));
    }

    private static Guid? TryGetTenantId(JsonElement sourceObject)
    {
        var tenantIdRaw = GetMetadataValue(sourceObject, "tenant_id");
        return Guid.TryParse(tenantIdRaw, out var tenantId) ? tenantId : null;
    }

    private static JsonElement GetDataObject(JsonElement payloadRoot)
    {
        if (TryGetNestedElement(payloadRoot, out var dataObject, "data", "object"))
        {
            return dataObject;
        }

        return default;
    }

    private static string? GetMetadataValue(JsonElement sourceObject, string key)
    {
        return GetNestedString(sourceObject, "metadata", key);
    }

    private static JsonElement GetFirstArrayItemNestedObject(
        JsonElement root,
        string arrayParentProperty,
        string arrayProperty,
        string nestedProperty)
    {
        if (!TryGetNestedElement(root, out var arrayRoot, arrayParentProperty, arrayProperty) ||
            arrayRoot.ValueKind != JsonValueKind.Array)
        {
            return default;
        }

        foreach (var item in arrayRoot.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.Object &&
                item.TryGetProperty(nestedProperty, out var nestedValue))
            {
                return nestedValue;
            }
        }

        return default;
    }

    private static string? GetString(JsonElement root, string propertyName)
    {
        return root.ValueKind == JsonValueKind.Object &&
               root.TryGetProperty(propertyName, out var property) &&
               property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }

    private static string? GetNestedString(JsonElement root, params string[] path)
    {
        return TryGetNestedElement(root, out var current, path) && current.ValueKind == JsonValueKind.String
            ? current.GetString()
            : null;
    }

    private static bool? GetBoolean(JsonElement root, string propertyName)
    {
        return root.ValueKind == JsonValueKind.Object &&
               root.TryGetProperty(propertyName, out var property) &&
               (property.ValueKind == JsonValueKind.True || property.ValueKind == JsonValueKind.False)
            ? property.GetBoolean()
            : null;
    }

    private static long? GetLong(JsonElement root, string propertyName)
    {
        return root.ValueKind == JsonValueKind.Object &&
               root.TryGetProperty(propertyName, out var property) &&
               property.ValueKind == JsonValueKind.Number &&
               property.TryGetInt64(out var value)
            ? value
            : null;
    }

    private static DateTime? GetUnixDateTimeUtc(JsonElement root, string propertyName)
    {
        var value = GetLong(root, propertyName);
        return value.HasValue ? DateTimeOffset.FromUnixTimeSeconds(value.Value).UtcDateTime : null;
    }

    private static DateTime? GetNestedUnixDateTimeUtc(JsonElement root, params string[] path)
    {
        if (!TryGetNestedElement(root, out var current, path) ||
            current.ValueKind != JsonValueKind.Number ||
            !current.TryGetInt64(out var seconds))
        {
            return null;
        }

        return DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime;
    }

    private static bool TryGetNestedElement(JsonElement root, out JsonElement current, params string[] path)
    {
        current = root;

        foreach (var segment in path)
        {
            if (current.ValueKind != JsonValueKind.Object ||
                !current.TryGetProperty(segment, out current))
            {
                current = default;
                return false;
            }
        }

        return true;
    }

    private static string? NormalizeCurrency(string? currency)
    {
        return currency?.ToUpperInvariant();
    }

    private static string? NormalizeCountryCode(string? countryCode)
    {
        return countryCode?.ToUpperInvariant();
    }

    private static string? FirstNonEmpty(string? first, string? fallback)
    {
        return !string.IsNullOrWhiteSpace(first) ? first : fallback;
    }

    private static DateTime? CoalesceDate(DateTime? first, DateTime? fallback)
    {
        return first ?? fallback;
    }
}
