using System.Security.Cryptography;
using System.Text;

namespace BaseFaq.Tenant.Public.Test.IntegrationTests.Helpers;

public static class StripeTestHelper
{
    /// <summary>
    /// Computes a valid Stripe webhook signature header for the given payload and secret.
    /// Uses the current UTC timestamp so the 300-second tolerance window is satisfied.
    /// </summary>
    public static string ComputeSignature(string webhookSecret, string payload)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return ComputeSignature(webhookSecret, payload, timestamp);
    }

    public static string ComputeSignature(string webhookSecret, string payload, long timestamp)
    {
        var signedPayload = $"{timestamp}.{payload}";
        var secretBytes = Encoding.UTF8.GetBytes(webhookSecret);
        var payloadBytes = Encoding.UTF8.GetBytes(signedPayload);
        using var hmac = new HMACSHA256(secretBytes);
        var hash = hmac.ComputeHash(payloadBytes);
        var signature = Convert.ToHexString(hash).ToLowerInvariant();
        return $"t={timestamp},v1={signature}";
    }

    /// <summary>
    /// Builds a minimal Stripe event JSON payload. Embed <paramref name="tenantId"/> in
    /// data.object.metadata.tenant_id to exercise the tenant extraction path.
    /// </summary>
    public static string BuildPayload(
        string eventId,
        string eventType,
        Guid? tenantId = null,
        bool liveMode = false,
        string? objectId = null)
    {
        var metadataPart = tenantId.HasValue
            ? $@"""metadata"": {{ ""tenant_id"": ""{tenantId}"" }}"
            : @"""metadata"": {}";

        return $$"""
            {
              "id": "{{eventId}}",
              "type": "{{eventType}}",
              "livemode": {{(liveMode ? "true" : "false")}},
              "created": {{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}},
              "data": {
                "object": {
                  "id": "{{objectId ?? "obj_test_001"}}",
                  {{metadataPart}}
                }
              }
            }
            """;
    }
}
