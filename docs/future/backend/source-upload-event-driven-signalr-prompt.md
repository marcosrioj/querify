# Prompt: Event-Driven Source Upload Verification And Portal SignalR Notifications

Use this prompt to implement the next architecture change for QnA source uploads.

## Required Process

Follow [`docs/behavior-change-playbook.md`](../../behavior-change-playbook.md) before editing code.

This is a behavior change that touches backend workflow, messaging, Portal runtime behavior, tests, and documentation. Start with the playbook inventory step and keep repository artifacts in English.

Do not run or generate EF migrations unless the user explicitly asks for migration work.

## Architecture Decision

Replace the current recurring source-upload verification path with an event-driven path:

- Do not depend on MinIO/S3 object-created events.
- The browser still uploads directly through the presigned PUT URL.
- When the upload finishes, the frontend calls the existing `CompleteUpload` endpoint.
- `CompleteUpload` is the application-owned signal that the object is ready for verification.
- After `CompleteUpload` succeeds, publish a RabbitMQ message.
- The QnA worker consumes that message immediately and runs the existing upload verification logic.
- The worker publishes a status-changed message after verification completes.
- The Portal API receives status-changed messages and forwards them to connected Portal clients through SignalR.
- The Portal frontend updates source upload UI from SignalR notifications instead of repeatedly polling until verification completes.

Keep Hangfire installed, configured, and ready for unrelated future jobs. Remove only the recurring source-upload verification registration/path. Do not remove Hangfire infrastructure, dashboard wiring, `HangfireQnaDb`, or documentation for future operational jobs.

Keep the pending-upload expiry behavior separate. Do not remove expiry unless the user explicitly asks for that in a separate change.

## Reliability Position

Status in the database remains the source of truth. SignalR is a realtime UX layer, not the authoritative state store.

On page load or refresh, the frontend must still query the current source status from the API. SignalR should only eliminate the active "keep calling until verified" loop.

The upload verification command must remain idempotent:

- If the source is already `Verified`, `Failed`, or `Quarantined`, the worker must not corrupt it.
- If duplicate RabbitMQ messages arrive, processing must be safe.
- If another worker changed the source first, the command must skip cleanly.

For this implementation, prefer the simplest application-owned trigger:

- `CompleteUpload` saves the source state as `Uploaded`.
- After the DB save succeeds, publish `SourceUploadCompleted`.

If the repo already has a safe, tenant-aware transactional outbox pattern that works with the QnA tenant-scoped `QnADbContext`, use it. If not, do not reintroduce a custom polling outbox background service in this change. Instead, publish after commit and document the residual gap: if the publish fails after the DB commit, a later reconciliation job will be needed to find stuck `Uploaded` sources. That reconciliation can be Hangfire-based later, but it is not the main verification path.

## Messaging Design

Use RabbitMQ through the repository's MassTransit conventions.

Add integration event contracts in an appropriate shared contracts location:

- `SourceUploadCompletedIntegrationEvent`
- `SourceUploadStatusChangedIntegrationEvent`

Suggested fields for `SourceUploadCompletedIntegrationEvent`:

- `EventId`
- `OccurredAtUtc`
- `TenantId`
- `SourceId`
- `StorageKey`
- `ClientChecksum`
- `ContentType`
- `SizeBytes`
- `CompletedByUserId`

Suggested fields for `SourceUploadStatusChangedIntegrationEvent`:

- `EventId`
- `OccurredAtUtc`
- `TenantId`
- `SourceId`
- `UploadStatus`
- `StorageKey`
- `Checksum`
- `Reason`

Keep event versioning explicit in queue/exchange names, for example:

- `qna.source-upload.completed.v1`
- `qna.source-upload.status-changed.v1`

Follow the existing backend architecture rule:

```text
Consumer -> Service (Telemetry) -> Consumers (Only folder) -> Command/Query
Event -> NotificationService (Telemetry) -> Command/Query
```

Do not put verification logic directly in consumers.

## Backend Steps

1. Inventory current source upload behavior using the playbook.
   - Find `SourceUploadVerificationBackgroundService`.
   - Find `SourceUploadHangfireJobRegistrationHostedService`.
   - Find `SourceUploadVerificationSweepService`.
   - Find `VerifyUploadedSourcesForAllTenantsCommand`.
   - Find `VerifyUploadedSourceCommand`.
   - Find `SourcesCompleteUploadCommandHandler`.
   - Find all current frontend polling behavior for source upload status.

2. Add the RabbitMQ event contract for upload completion.
   - Keep contracts flat and explicit.
   - Do not return translated labels.
   - Do not introduce new source status enum values unless existing statuses cannot express the workflow.

3. Publish `SourceUploadCompletedIntegrationEvent` from the Portal API after `CompleteUpload`.
   - Preserve existing validation in `SourcesCompleteUploadCommandHandler`.
   - Publish only after the source state is successfully saved as `Uploaded`.
   - Include enough fields for the worker to verify the exact staging object.

4. Add a QnA worker consumer for `SourceUploadCompletedIntegrationEvent`.
   - The consumer should create/enter the tenant context.
   - It should call a focused service.
   - The service should send `VerifyUploadedSourceCommand`.
   - Let transient failures flow through MassTransit retry/dead-letter behavior.
   - Do not swallow exceptions in a way that marks messages successful when verification did not run.

5. Remove source upload verification as a recurring scheduled operation.
   - Remove or stop registering `SourceUploadHangfireJobRegistrationHostedService` for source upload verification.
   - Remove the `SourceUploadVerificationBackgroundService` recurring path if it has no remaining caller.
   - Remove or deprecate `SourceUploadVerificationSweepService` and `VerifyUploadedSourcesForAllTenantsCommand` if they exist only for the recurring sweep.
   - Keep Hangfire server/dashboard/storage registration intact for future jobs.
   - Keep pending upload expiry unchanged.

6. After verification finishes, publish `SourceUploadStatusChangedIntegrationEvent`.
   - Publish for terminal statuses: `Verified`, `Failed`, `Quarantined`.
   - Include `TenantId`, `SourceId`, status, and any useful diagnostic reason that is safe for Portal users.
   - Keep the database as the source of truth.

7. Add tests for the worker consumer.
   - Maps upload-completed event to `VerifyUploadedSourceCommand`.
   - Uses tenant context for the event tenant.
   - Duplicate or already-processed source does not break verification.
   - Failed verification produces the expected terminal status event if the publishing boundary is in scope.

## Portal SignalR Foundation

Create a reusable Portal notification foundation, not a source-specific one-off.

Backend target:

- Add a reusable Portal SignalR foundation in `Querify.Common.Infrastructure.Signalr`.
- Put Portal-specific base files first under its `Portal/` folder:
  - options
  - extensions
  - hubs
  - notification envelopes/groups
  - publishers
  - abstractions
- Add a general Portal notifications hub, for example `PortalNotificationsHub`.
- Use the Portal API endpoint path such as `/api/qna/hubs/portal-notifications`.
- Authenticate hub connections with the existing Portal JWT/auth setup.
- Do not allow clients to subscribe to arbitrary tenant groups without authorization checks.
- Group naming should be stable and generic, for example:
  - `tenant:{tenantId}:module:{module}`
  - `tenant:{tenantId}:user:{userId}`
- Add application-level notification abstractions in `Querify.Common.Infrastructure.Signalr`, for example:
  - `IPortalNotificationPublisher`
  - `PortalNotificationEnvelope`
- The envelope should be extensible:
  - `NotificationId`
  - `OccurredAtUtc`
  - `Type`
  - `Module`
  - `TenantId`
  - `ResourceKind`
  - `ResourceId`
  - `Version`
  - `Payload`
- Source upload status changes should be only the first notification type, for example:
  - `qna.source-upload.status-changed.v1`

Portal API messaging target:

- Add a Portal API RabbitMQ consumer for `SourceUploadStatusChangedIntegrationEvent`.
- Source-specific events and notifications belong in `Querify.QnA.Portal.Business.Source` under
  `Events/` and `Notifications/`.
- The consumer should call `SourceUploadStatusChangedNotificationService`.
- The notification service should own telemetry and send a notification command/query.
- The notification command should call `IPortalNotificationPublisher`.
- The publisher should send a SignalR envelope to the authorized tenant/module group.
- Do not make the QnA worker reference the Portal hub directly.

Frontend target:

- Add a shared realtime foundation under `apps/portal/src/shared`, not under the Sources domain.
- Suggested shape:
  - `shared/realtime/portal-notifications-client.ts`
  - `shared/realtime/portal-notifications-provider.tsx`
  - `shared/realtime/use-portal-notifications.ts`
  - `shared/realtime/portal-notification-types.ts`
- Use one SignalR connection per Portal app session.
- Use reconnect/backoff behavior.
- Use the existing auth token provider for the SignalR access token.
- Keep domain-specific handling outside the core client.
- Add a source-domain handler that listens for `qna.source-upload.status-changed.v1`.
- Remove the frontend loop that repeatedly calls the source status endpoint until verification completes.
- Keep the initial source query for page load/refresh correctness.

If `@microsoft/signalr` is not already installed in the Portal app, add it using the repo's package manager and lockfile conventions.

## Source Upload UX Steps

Keep the user flow simple:

1. User selects a source file.
2. Portal calls `CreateUploadIntent`.
3. Browser uploads the file to the presigned URL.
4. Browser calls `CompleteUpload`.
5. UI shows a non-blocking "verification pending" state.
6. QnA worker consumes the RabbitMQ upload-completed event.
7. Worker verifies the source and updates the database.
8. Worker publishes status changed.
9. Portal API notification flow handles the event:
   `SourceUploadStatusChangedConsumer -> SourceUploadStatusChangedNotificationService (Telemetry) -> NotifySourceUploadStatusChangedCommand`.
10. The notification command publishes the status through the shared Portal SignalR publisher.
11. Frontend updates the current source row/form state without polling.

If the SignalR connection is offline, the UI must remain correct after refresh because source status is persisted in the database.

## Hangfire Scope

Do not remove:

- `AddHangfireQnaDb`
- Hangfire dashboard support
- `HangfireQnaDbContext`
- Hangfire docs
- Hangfire package references
- generic Hangfire infrastructure

Do remove or stop using only the source upload verification recurring job.

Leave a short documentation note explaining:

- source verification is RabbitMQ-driven now
- Hangfire remains available for future operational jobs
- a future low-frequency reconciliation job may use Hangfire to find stuck `Uploaded` sources if needed

## Documentation Updates

Update the most specific docs after implementation:

- `docs/future/backend/source-upload.md` or its successor if the feature is now operational
- `docs/backend/architecture/dotnet-backend-overview.md` if runtime responsibilities changed
- `docs/frontend/architecture/portal-app.md` if SignalR becomes part of the Portal runtime
- `docs/frontend/tools/portal-runtime.md` if new environment variables or host/proxy paths are required

Make the docs clear that local MinIO is only object storage and is not an event source for this flow.

## Validation

Run focused validation first:

```bash
dotnet build dotnet/Querify.QnA.Portal.Api/Querify.QnA.Portal.Api.csproj -v minimal
dotnet build dotnet/Querify.QnA.Worker.Api/Querify.QnA.Worker.Api.csproj -v minimal
dotnet test dotnet/Querify.QnA.Portal.Test.IntegrationTests/Querify.QnA.Portal.Test.IntegrationTests.csproj -v minimal
dotnet test dotnet/Querify.QnA.Worker.Test.IntegrationTests/Querify.QnA.Worker.Test.IntegrationTests.csproj -v minimal
```

Run frontend validation if Portal files changed:

```bash
pnpm -C apps/portal test
pnpm -C apps/portal build
```

Run a local smoke test when infrastructure is available:

1. Start base services and backend services.
2. Start Portal frontend.
3. Create a source upload intent.
4. Upload the file through the presigned URL.
5. Complete the upload.
6. Confirm RabbitMQ receives the completion event.
7. Confirm the QnA worker consumes it.
8. Confirm the source reaches `Verified`, `Failed`, or `Quarantined`.
9. Confirm the frontend updates through SignalR without status polling.

## Explicit Non-Goals

- Do not implement MinIO/S3 object-created notifications.
- Do not make the frontend connect to RabbitMQ.
- Do not create a source-specific SignalR hub.
- Do not remove Hangfire infrastructure.
- Do not remove pending upload expiry.
- Do not add migrations unless explicitly requested.
- Do not add translated labels to backend DTOs or SignalR payloads.
