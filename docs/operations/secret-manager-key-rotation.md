# Tenant AI Provider Key Management

## Document purpose
Define how AI provider keys are managed after moving provider selection and credentials to tenant configuration.

## Current model
- Provider/model/prompt catalog is stored in `AiProviders`.
- Tenant-specific credential binding is stored in `TenantAiProviders`.
- Credential value is `TenantAiProviders.AiProviderKey`.
- Worker runtime resolves provider/model/key by `TenantId` + `AiCommandType` directly from tenant DB.

## Security model
- Do not store tenant provider keys in repository `appsettings*.json`.
- Keep key creation and rotation inside tenant management flows (`SetAiProviderCredentials` and related admin flows).
- Tenant keys are encrypted at persistence level via the `TenantDbContext` value converter.

## Rotation flow
1. Choose the target tenant and command (`Generation` or `Matching`).
2. Set/update credentials through tenant configuration APIs.
3. Confirm the tenant has a configured key with:
   - `GetConfiguredAiProviders`
   - `IsAiProviderKeyConfigured/{command}`
4. Validate worker success metrics and error rates.
5. Revoke previous provider key at vendor side after stabilization.

## Incident response
1. Update tenant credential immediately for the impacted command.
2. Revoke compromised key in provider console.
3. Validate command execution health and queue recovery.
4. Audit logs for failed AI operations during incident window.
