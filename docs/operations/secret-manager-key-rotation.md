# Secret Manager Integration and Key Rotation

## Document purpose
Define how AI provider API keys are configured, protected, and rotated for operational continuity and security compliance.

## Intended audience
- Engineers operating AI generation and matching services
- Security reviewers validating secret-handling controls
- On-call responders handling key compromise or provider access incidents

## Business outcomes
- Maintain AI feature availability during key rotation.
- Reduce security exposure through dual-slot active/standby key handling.
- Provide a clear incident runbook for compromised credentials.

## Technical outcomes
- Use configuration-backed runtime key slot switching.
- Avoid repository-based secret storage.
- Keep production startup guarded by `RequireApiKey=true`.

## Step-by-step operating sequence
1. Configure keys in development or production secret sources.
2. Validate active slot behavior through runtime config access.
3. Perform planned rotation using standby slot promotion.
4. Execute incident rotation when compromise is suspected.

## Scope
- Applies to AI generation worker hosting in `dotnet/BaseFaq.AI.Generation.Api`.
- Applies to AI matching worker hosting in `dotnet/BaseFaq.AI.Matching.Api`.
- Supports active/standby API keys with runtime selection using `IOptionsMonitor`.

## Step 1) Configuration model
- Section: `AiProvider`
- Keys:
  - `Provider`
  - `Model`
  - `ActiveKeySlot` (`Primary` or `Secondary`)
  - `PrimaryApiKey`
  - `SecondaryApiKey`
  - `RequireApiKey`

The worker reads current values via `IAiProviderCredentialAccessor`, so key slot changes are applied without process restart when configuration source supports reload.

## Step 2) Development secret source
- `UserSecretsId` is enabled for `BaseFaq.AI.Generation.Api`.
- Set keys with:
```bash
dotnet user-secrets --project dotnet/BaseFaq.AI.Generation.Api set "AiProvider:PrimaryApiKey" "<key>"
dotnet user-secrets --project dotnet/BaseFaq.AI.Generation.Api set "AiProvider:ActiveKeySlot" "Primary"
dotnet user-secrets --project dotnet/BaseFaq.AI.Matching.Api set "AiProvider:PrimaryApiKey" "<key>"
dotnet user-secrets --project dotnet/BaseFaq.AI.Matching.Api set "AiProvider:ActiveKeySlot" "Primary"
```

## Step 3) Production secret source
- Do not place API keys in repository `appsettings*.json`.
- Inject secret values from a managed secret store into runtime config (for example environment variables or platform secret mapping):
```bash
AiProvider__PrimaryApiKey=<key-a>
AiProvider__SecondaryApiKey=<key-b>
AiProvider__ActiveKeySlot=Primary
AiProvider__RequireApiKey=true
```
- Outside development, startup validation fails if no AI API key is configured.

## Step 4) Planned rotation process
1. Provision new key into standby slot in secret manager (for example set `AiProvider__SecondaryApiKey`).
2. Validate standby key health in provider account and monitoring.
3. Switch active key by updating `AiProvider__ActiveKeySlot` to standby slot.
4. Monitor generation success/failure and provider error rates.
5. Revoke old key after stabilization window.
6. Refill now-standby slot with the next key to keep dual-slot readiness.

## Step 5) Incident rotation process
1. Immediately switch `ActiveKeySlot` to the healthy slot.
2. Revoke compromised key.
3. Issue replacement key into standby slot.
4. Keep `RequireApiKey=true` in production to block insecure startup.
