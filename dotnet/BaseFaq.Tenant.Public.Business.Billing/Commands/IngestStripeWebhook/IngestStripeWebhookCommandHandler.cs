using System.Net;
using System.Text.Json;
using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Common.EntityFramework.Tenant.Entities;
using BaseFaq.Common.EntityFramework.Tenant.Enums;
using BaseFaq.Common.EntityFramework.Tenant.Extensions;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Models.Tenant.Enums;
using BaseFaq.Tenant.Public.Business.Billing.Options;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;

namespace BaseFaq.Tenant.Public.Business.Billing.Commands.IngestStripeWebhook;

public sealed class IngestStripeWebhookCommandHandler(
    TenantDbContext dbContext,
    IOptions<StripeWebhookOptions> options,
    ILogger<IngestStripeWebhookCommandHandler> logger)
    : IRequestHandler<IngestStripeWebhookCommand, bool>
{
    private const string TableName = "BillingWebhookInboxes";
    private const BillingProviderType Provider = BillingProviderType.Stripe;

    public async Task<bool> Handle(IngestStripeWebhookCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!await dbContext.TableExistsAsync(TableName, cancellationToken))
        {
            throw new ApiErrorException(
                "Billing webhook ingress is not ready. Apply the TenantDb billing migration first.",
                errorCode: (int)HttpStatusCode.ServiceUnavailable);
        }

        if (string.IsNullOrWhiteSpace(request.Signature))
        {
            throw new ApiErrorException(
                "Stripe signature header is missing.",
                errorCode: (int)HttpStatusCode.BadRequest);
        }

        VerifyStripeSignature(request.PayloadJson, request.Signature);

        using var payloadDocument = JsonDocument.Parse(request.PayloadJson);
        var payloadRoot = payloadDocument.RootElement;

        var externalEventId = GetRequiredString(payloadRoot, "id");
        var eventType = GetRequiredString(payloadRoot, "type");
        var tenantId = TryGetTenantId(payloadRoot);

        var alreadyExists = await dbContext.BillingWebhookInboxes
            .AsNoTracking()
            .AnyAsync(
                entry => entry.Provider == Provider && entry.ExternalEventId == externalEventId,
                cancellationToken);

        if (alreadyExists)
        {
            logger.LogInformation(
                "Stripe webhook event {ExternalEventId} ({EventType}) was already ingested. Returning success without duplicating the inbox row.",
                externalEventId,
                eventType);
            return false;
        }

        var inboxItem = new BillingWebhookInbox
        {
            TenantId = tenantId,
            Provider = Provider,
            ExternalEventId = externalEventId,
            EventType = eventType,
            PayloadJson = request.PayloadJson,
            Signature = request.Signature,
            SignatureValid = true,
            IsLiveMode = TryGetBoolean(payloadRoot, "livemode") ?? false,
            ProviderAccountId = TryGetString(payloadRoot, "account"),
            ReceivedDateUtc = DateTime.UtcNow,
            EventCreatedAtUtc = TryGetUnixDateTimeUtc(payloadRoot, "created"),
            Status = ControlPlaneMessageStatus.Pending,
            AttemptCount = 0
        };

        await dbContext.BillingWebhookInboxes.AddAsync(inboxItem, cancellationToken);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException ex)
        {
            var duplicateDetected = await dbContext.BillingWebhookInboxes
                .AsNoTracking()
                .AnyAsync(
                    entry => entry.Provider == Provider && entry.ExternalEventId == externalEventId,
                    cancellationToken);

            if (duplicateDetected)
            {
                logger.LogInformation(
                    ex,
                    "Stripe webhook event {ExternalEventId} ({EventType}) raced with another ingress request and was already stored.",
                    externalEventId,
                    eventType);
                return false;
            }

            throw;
        }
    }

    private void VerifyStripeSignature(string payloadJson, string signature)
    {
        try
        {
            EventUtility.ConstructEvent(payloadJson, signature, options.Value.WebhookSecret, throwOnApiVersionMismatch: false);
        }
        catch (StripeException ex)
        {
            logger.LogWarning(ex, "Stripe webhook signature verification failed.");
            throw new ApiErrorException(
                "Stripe webhook signature is invalid.",
                errorCode: (int)HttpStatusCode.BadRequest);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Stripe webhook payload is not valid JSON.");
            throw new ApiErrorException(
                "Stripe webhook payload is invalid.",
                errorCode: (int)HttpStatusCode.BadRequest);
        }
    }

    private static Guid? TryGetTenantId(JsonElement root)
    {
        var tenantIdRaw = TryGetNestedString(root, "data", "object", "metadata", "tenant_id");
        return Guid.TryParse(tenantIdRaw, out var tenantId) ? tenantId : null;
    }

    private static string GetRequiredString(JsonElement root, string propertyName)
    {
        var value = TryGetString(root, propertyName);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ApiErrorException(
                "Stripe webhook payload is missing required metadata.",
                errorCode: (int)HttpStatusCode.BadRequest);
        }

        return value;
    }

    private static string? TryGetString(JsonElement root, string propertyName)
    {
        return root.TryGetProperty(propertyName, out var property) &&
               property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }

    private static string? TryGetNestedString(JsonElement root, params string[] path)
    {
        if (!TryGetNestedElement(root, out var property, path))
        {
            return null;
        }

        return property.ValueKind == JsonValueKind.String ? property.GetString() : null;
    }

    private static bool? TryGetBoolean(JsonElement root, string propertyName)
    {
        return root.TryGetProperty(propertyName, out var property) &&
               (property.ValueKind == JsonValueKind.True || property.ValueKind == JsonValueKind.False)
            ? property.GetBoolean()
            : null;
    }

    private static DateTime? TryGetUnixDateTimeUtc(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var property) ||
            property.ValueKind != JsonValueKind.Number ||
            !property.TryGetInt64(out var seconds))
        {
            return null;
        }

        return DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime;
    }

    private static bool TryGetNestedElement(
        JsonElement root,
        out JsonElement current,
        params string[] path)
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
}
