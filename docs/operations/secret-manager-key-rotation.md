# Tenant AI Provider Key Management

## Purpose

This runbook describes how AI provider credentials are stored, rotated, and validated in BaseFAQ.

## Storage model

The current backend model is tenant-driven:

- provider catalog metadata lives in `AiProviders`
- tenant-specific credential bindings live in `TenantAiProviders`
- the encrypted credential value is stored in `TenantAiProviders.AiProviderKey`
- AI workers resolve the credential by `TenantId` and `AiCommandType`

## Security rules

- do not store tenant provider keys in repository `appsettings*.json`
- do not hardcode live provider keys in the AI host configuration
- keep creation and rotation inside tenant administration flows
- keep tenant-level encryption enabled in the persistence layer

## Standard rotation workflow

1. Identify the tenant and the affected AI command:
   - `Generation`
   - `Matching`
2. Update the credential through the tenant AI-provider configuration flow.
3. Verify the tenant reports a configured key through the relevant query endpoints.
4. Run a controlled validation of the affected workflow.
5. Monitor logs, queue health, and error rates.
6. Revoke the previous vendor-side key after the new key is stable.

## Validation checklist

- tenant can still resolve the configured provider
- generation or matching requests no longer fail due to authentication or provider access
- queue consumers continue processing normally
- no sensitive credential material appears in logs or traces

## Incident response

1. Replace the tenant credential immediately.
2. Revoke the compromised vendor-side key.
3. Validate message processing and worker recovery.
4. Review failed AI operations during the exposure window.

## Related areas

- AI architecture: [`../architecture/basefaq-ai-generation-matching-architecture.md`](../architecture/basefaq-ai-generation-matching-architecture.md)
- seed prerequisites: [`../tools/seed-tool.md`](../tools/seed-tool.md)
