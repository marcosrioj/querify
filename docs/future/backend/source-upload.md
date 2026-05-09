# Source Upload: File Ingestion for the QnA Module

## Purpose

This document is the complete design reference for adding **file upload** capability to the
`Source` entity in the QnA module. Today every `Source` points to an external locator (URL, ticket
id, repo path); the platform has no way for a tenant to upload a PDF, video, or document and link
it as a Source.

This document describes the target architecture, the design decisions behind it, the implementation
roadmap, and a self-contained agent prompt for each implementation step. It incorporates the
deep-research review supplied for this update and defines the current implementation as
**single presigned PUT only**.

**Status:** implemented for the core presigned upload flow, RabbitMQ-driven verification, and
Portal SignalR status notifications. Earlier staged prompts in this document that describe
Hangfire as the primary source-upload verification trigger are historical and superseded by the
event-driven runtime described below. See [`../README.md`](../README.md).

---

## Current state

The `Source` entity already supports the shape needed for a hosted file:

| Field | Today | Used for upload as |
|---|---|---|
| `Locator` (required, max 1000) | URL, ticket id, repo path | current storage key (`staging/`, then `verified/` or `quarantine/`) |
| `StorageKey` (new) | not present | nullable explicit storage key for uploaded sources |
| `MediaType` (optional, max 100) | declared content type | declared content type during `Pending`, resolved content type after `HEAD` |
| `Checksum` (required, max 128) | `sha256:<hex of locator>` placeholder | real `sha256:<hex of file bytes>` after worker verification |
| `MetadataJson` (optional, max 8000) | free-form JSON | reserved for extracted metadata |
| `SizeBytes` (new) | not present | expected size during `Pending`, confirmed size after `upload-complete` |

Implemented runtime pieces:

1. Local object storage is provided through MinIO while production remains S3-compatible-provider
   agnostic.
2. `Querify.Common.Infrastructure.Storage` owns the shared `IObjectStorage` abstraction.
3. `Querify.QnA.Portal.Api` exposes upload intent, upload completion, and verified download URL
   endpoints through the Source service and command/query boundary.
4. `Querify.QnA.Worker.Api` verifies uploaded bytes from RabbitMQ `SourceUploadCompleted` events,
   recomputes checksum, validates content, scans, and transitions each source to a terminal upload
   status.
5. `Querify.Common.Infrastructure.Signalr` owns the reusable Portal SignalR foundation, while
   Source-specific notification events, services, and commands live in
   `Querify.QnA.Portal.Business.Source`.

---

## Target architecture

### One-line summary

Tenant creates an upload intent, receives a short-lived presigned PUT from the backend, uploads the
file directly to S3-compatible object storage, finalizes through a portal endpoint that records the
object metadata, and a QnA worker asynchronously validates the bytes, copies the trusted object to
a verified key, and flips `UploadStatus` to `Verified`.

### Topology

```
Portal (React)              QnA Portal API                Object Storage           QnA Worker
                            (Querify.QnA.Portal.Api)      (MinIO local / S3 API)
─────────────────           ─────────────────────────     ────────────────         ───────────
1. POST /upload-intent ───► validate + create
                            Source(Pending)               
                            presign single PUT URL ──────► staging key signed
                            ◄───────── { sourceId, url, key, ttl, headers }
2. PUT (file bytes) ─────────────────────────────────────► stored under staging/
3. POST /upload-complete ─► HEAD object
                            validate size/type
                            persist SizeBytes/MediaType
                            UploadStatus = Uploaded
                            publish SourceUploadCompleted ─────────────────────────► RabbitMQ
                                                                                    consume event
                                                                                    service sends command
                                                                                    stream SHA-256
                                                                                    validate MIME/magic
                                                                                    malware scan
                                                                                    copy to verified/
                                                                                    delete staging/
                                                                                    UploadStatus = Verified
                                                                                    publish status changed
                            consume status changed
                            SignalR tenant notification ───────────────────────────► Portal UI
4. GET /download-url ─────► presign GET URL after Verified
   ◄──── { url }
```

### Why presigned URLs and not stream-through

- The QnA Portal API stays free of file bytes — no memory pressure, no request timeout per file
  size, horizontal scaling stays trivial.
- Uploads do not contend with HTTP request worker threads.
- The same pattern is reusable across modules later (Direct attachments, Broadcast media) without
  reinventing transport.
- The `Locator` field stays an opaque string — URL sources and storage-key sources coexist in the
  same column, no schema split.

### Implementation scope

- **Single presigned PUT only.** No multipart or resumable endpoints. The default max file size is
  50 MB, configurable per environment and tenant tier.
- **Local support is mandatory.** `devops/local/docker/docker-compose.baseservices.yml` gets a
  MinIO service plus an idempotent bucket initializer. Local development, integration tests, S3,
  R2, and other S3-compatible providers all use the same `IObjectStorage` abstraction.
- **Shared-by-default for cost.** Production starts with a private shared bucket and tenant-prefixed
  keys. Bucket-per-tenant, account-per-tenant, or tenant-managed encryption keys are premium or
  regulated-tier options only.
- **Uploaded bytes are not trusted.** The presigned URL writes only to a staging key. Downloads are
  refused until the worker has validated checksum, size, content type, magic bytes, and malware
  scan result, then copied the object to a verified key.
- **No API stream-through fallback.** Streaming uploads through the Portal API is intentionally out
  of scope for the default path because it increases compute, bandwidth, timeout, and scaling cost.

### Presigned PUT limitations

S3-compatible presigned PUTs are simple and cheap, but they cannot be treated as a complete policy
engine. The backend validates the declared `SizeBytes` and `ContentType` when the intent is created,
signs a short TTL and required headers for the server-generated staging key, then validates the
actual object with `HEAD` and streaming reads after upload. If the object is oversized, has the
wrong type, or fails verification, the system marks the `Source` as `Failed` or `Quarantined` and
never signs a download URL for it.

### Why a dedicated QnA worker, not the existing tenant worker

`Querify.Tenant.Worker.Api` is the **control plane** worker — billing webhooks, email outbox,
entitlements. Per [`../../backend/architecture/querify-tenant-worker.md`](../../backend/architecture/querify-tenant-worker.md)
and [`../../backend/architecture/solution-architecture.md`](../../backend/architecture/solution-architecture.md)
section 7, it must not take ownership of product module workflows.

QnA-owned async work belongs to `Querify.QnA.Worker.Api`, mirroring the project layout of the
tenant worker. Source upload verification is RabbitMQ-driven: the Portal API publishes
`SourceUploadCompletedIntegrationEvent` after `upload-complete`, and the QnA worker consumer calls a
service that dispatches `VerifyUploadedSourceCommand`. Hangfire remains installed and configured in
the worker for unrelated operational jobs and future reconciliation, but it is not the primary
source-upload verification trigger.

---

## Multi-tenant key strategy

Storage key formats:

```
{tenantId}/sources/{sourceId}/staging/{sanitized-filename}
{tenantId}/sources/{sourceId}/verified/{sanitized-filename}
{tenantId}/sources/{sourceId}/quarantine/{sanitized-filename}
```

- `tenantId` prefix isolates blobs per tenant. A bucket policy can enforce `s3:prefix` per-tenant
  if the credentials are ever scoped down.
- `sourceId` is generated server-side when the intent is created, so all keys are deterministic and
  retry-safe without exposing tenant-controlled paths.
- The presigned PUT always targets `staging/`. After successful verification, the worker copies the
  object to `verified/`, deletes the staging object, and updates `Source.StorageKey` + `Locator` to
  the verified key.
- If malware is detected, the worker copies or moves the object to `quarantine/`, updates
  `Source.StorageKey` to that key, and sets `UploadStatus = Quarantined`. Quarantined objects are
  never downloadable through `download-url`.
- `sanitized-filename` only allows `[a-zA-Z0-9._-]`; everything else is replaced with `-`.
- `(TenantId, StorageKey)` carries a unique partial index in PostgreSQL where `StorageKey IS NOT
  NULL`, preventing duplicate storage references across the tenant.

Cross-tenant integrity is already enforced by `QnADbContext.OnBeforeSaveChangesRules()` because
`Source` is `IMustHaveTenant`. No new tenant-integrity extension is required by this feature
because `StorageKey` does not reference another tenant-owned record (per the
[`../../behavior-change-playbook.md`](../../behavior-change-playbook.md) Step 3.20 rule).

---

## API flow with documented playbook exception

### API step A — `POST /api/qna/source/upload-intent`

Body: `SourceUploadIntentRequestDto`

```
{ FileName, ContentType, SizeBytes, Language, Label?, ContextNote?, ExternalId?, MetadataJson? }
```

Returns `SourceUploadIntentResponseDto { SourceId, UploadUrl, RequiredHeaders, StorageKey,
ExpiresAtUtc }` (`200 OK`).

Handler is a command with a documented playbook exception
(`IRequestHandler<TCommand, SourceUploadIntentResponseDto>`):

1. Resolve `tenantId` via `ISessionService.GetTenantId(ModuleEnum.QnA)`.
2. Validate extension + declared `ContentType` against an allowlist (`application/pdf`,
   `image/png`, `image/jpeg`, `video/mp4`, `text/plain`, `text/markdown` initially).
3. Validate `0 < SizeBytes <= SourceUploadOptions.MaxUploadBytes` (50 MB default;
   configurable).
4. Generate `sourceId = Guid.NewGuid()`.
5. Build `stagingKey = SourceStorageKey.BuildStagingKey(tenantId, sourceId, fileName)`.
6. Create `Source` with:
   - `UploadStatus = Pending`
   - `Locator = stagingKey`
   - `Checksum = SourceChecksum.FromLocator(stagingKey)` (placeholder; the worker overwrites this)
   - `StorageKey = stagingKey`
   - `MediaType = request.ContentType` (declared type; worker later confirms content)
   - `SizeBytes = request.SizeBytes` (expected size while `Pending`)
7. Persist the pending source.
8. `presign = await objectStorage.PresignPutAsync(stagingKey, contentType, request.SizeBytes, ct)`.
9. Return `SourceUploadIntentResponseDto { SourceId, UploadUrl, RequiredHeaders, StorageKey,
    ExpiresAtUtc }`.

Playbook exception:

- `behavior-change-playbook.md` says command handlers return simple values and complex DTOs belong
  to queries.
- `upload-intent` is the only exception in this implementation because creating the intent and
  issuing the short-lived upload credential are one atomic user workflow.
- Splitting this into a separate command and query adds a round trip without improving persistence
  safety, because the presigned URL is not persisted and does not mutate domain state.
- The backend still owns presigned URL generation; the client never signs storage requests and never
  receives storage credentials.

Failure modes:
- Invalid content type → `422 Unprocessable Entity` via `ApiErrorException`.
- Oversized request → `422`.
- Storage unreachable → `503 Service Unavailable` (`ApiErrorException`).

Required upload headers:
- `Content-Type` must match the declared content type used in the signature.
- Provider-specific encryption headers are included when configured (`x-amz-server-side-encryption`
  for S3-compatible providers, for example).
- `Content-Length` is still checked by the API after upload; do not assume every S3-compatible
  presigned PUT can enforce it at signature time.

### API step B — `POST /api/qna/source/{id:guid}/upload-complete`

Body: `SourceUploadCompleteRequestDto { ClientChecksum? }`

Returns `Guid` (`200 OK`).

Handler is a real command (`IRequestHandler<TCommand, Guid>`):

1. Load the `Source` by `(TenantId, Id)`. `404` if missing.
2. If `UploadStatus != Pending`: `409 Conflict` (already finalized).
3. `head = await objectStorage.HeadAsync(entity.StorageKey, ct)`.
4. If `head == null`: `422` (the client never PUT the bytes).
5. If `head.SizeBytes != entity.SizeBytes` or
   `head.SizeBytes > SourceUploadOptions.MaxUploadBytes`, delete the staging object, set
   `UploadStatus = Failed`, persist, and return `422`.
6. If `head.ContentType` is missing, not allowlisted, or does not
   match the declared `MediaType`, delete the staging object, set `UploadStatus = Failed`,
   persist, and return `422`.
7. Persist `SizeBytes = head.SizeBytes`, `MediaType = head.ContentType`,
   `Checksum = ClientChecksum ?? SourceChecksum.FromLocator(entity.Locator)`,
   `UploadStatus = Uploaded`, `UpdatedBy = userId`.
8. After the database save succeeds, publish `SourceUploadCompletedIntegrationEvent`. If this
   publish fails after the commit, the row can remain `Uploaded`; a future low-frequency
   reconciliation job can use Hangfire to find and re-enqueue those stuck sources.

### API step C — `GET /api/qna/source/{id:guid}/download-url`

Returns `SourceDownloadUrlDto { Url, ExpiresAtUtc }` (`200 OK`).

Handler is a query:

1. Project `{ StorageKey, Visibility, UploadStatus, TenantId }` directly with `AsNoTracking() +
   Select(...)`.
2. `404` if missing or wrong tenant.
3. Apply the existing `Source` visibility/access rules for the current Portal user before signing.
4. `422` if `StorageKey == null` (URL-only sources should expose `Locator` directly to the client,
   not via this endpoint).
5. `422` if `UploadStatus != Verified`; uploaded, failed, expired, or quarantined objects are not
   downloadable.
6. `422` if the key is not under `/verified/`; this prevents a stale staging or quarantine key from
   ever being signed.
7. `presign = await objectStorage.PresignGetAsync(storageKey, downloadTtl, ct)` (5 min default).
8. Return `{ Url, ExpiresAtUtc }`.

---

## Async verification (worker)

Worker command/query structure:

- `SourceUploadCompletedConsumer` is the RabbitMQ adapter. It does not own verification logic.
- `SourceUploadCompletedConsumerService` owns telemetry, enters the event tenant context, and sends
  `VerifyUploadedSourceCommand` through MediatR.
- `VerifyUploadedSourceCommandHandler` owns the verification business flow and all state mutation.
- `VerifyUploadedSourceCommandHandler` publishes `SourceUploadStatusChangedIntegrationEvent` after
  terminal transitions (`Verified`, `Failed`, `Quarantined`).
- `PendingSourceUploadExpiryHostedService` is a scheduler adapter only. It sends
  `ExpirePendingSourceUploadsCommand` through MediatR.
- Worker business code follows the same command/query layout as Portal business code: commands
  mutate state and return simple values; queries are used only for read DTOs if a worker read use
  case is introduced later.

Command (`VerifyUploadedSourceCommandHandler` in
`Querify.QnA.Worker.Business.Source.Commands.VerifyUploadedSource`):

1. Resolve the tenant-scoped `QnADbContext` connection from the command's `TenantId` (uses
   `Querify.Common.EntityFramework.Tenant` resolution).
2. Load the `Source`. If `UploadStatus != Uploaded`: return without changes (idempotent).
3. Treat `command.StorageKey` as `stagingKey`; if it is not under `/staging/`, mark the source
   `Failed` and return.
4. Stream the blob via `IObjectStorage.OpenReadAsync(stagingKey)`. Compute SHA-256 with
   `IncrementalHash.CreateHash(HashAlgorithmName.SHA256)` so the file is never fully materialized.
5. Detect the actual file family from magic bytes and compare it with the allowlisted source kind
   and `MediaType`; never trust the browser-declared content type alone.
6. `head = HeadAsync(stagingKey)` — re-read size/content-type for defense in depth.
7. Run `IUploadThreatScanner.ScanAsync(stagingKey, hash metadata, ct)` using a fresh object read or
   provider-native scan result. Local development may use a no-op scanner, but production must use
   ClamAV, provider-native malware scanning, or an equivalent managed scanner before anything can
   become `Verified`.
8. If `ClientChecksum` was provided and does not match the streamed hash:
   - conditionally mark `UploadStatus = Failed` only when the row is still `Uploaded` and
     `StorageKey` still equals the staging key.
   - delete the staging object after the terminal row transition; log cleanup failure, but **do not
     rethrow** for a permanent failure.
9. If MIME/magic validation fails:
   - conditionally mark `UploadStatus = Failed`, delete the staging object after the terminal row
     transition, return.
10. If malware is detected or the scanner returns an unsafe verdict:
   - `quarantineKey = SourceStorageKey.ToQuarantineKey(stagingKey)`
   - copy the object to `quarantineKey`
   - conditionally update `StorageKey = quarantineKey`, `Locator = quarantineKey`,
     `UploadStatus = Quarantined`
   - delete the staging object after the terminal row transition; return; do not sign downloads for
     quarantined objects.
11. Otherwise:
   - `verifiedKey = SourceStorageKey.ToVerifiedKey(stagingKey)`
   - copy the object to `verifiedKey`
   - conditionally update `StorageKey = verifiedKey`, `Locator = verifiedKey`
   - `Checksum = "sha256:<hex>"`
   - `SizeBytes = head.SizeBytes`
   - `UploadStatus = Verified`
   - `UpdatedBy = "system:qna-worker"`
   - persist, then delete the staging object.

Retry semantics:
- Transient storage errors (5xx, timeouts) propagate so MassTransit/RabbitMQ retries the message.
- Permanent errors (checksum mismatch, MIME mismatch, malware detection) flip the row to `Failed`
  or `Quarantined`; the command returns without throwing.
- Cleanup after a successful terminal row transition is best effort. Downloads still require
  `UploadStatus = Verified` and a `/verified/` key, so a leftover staging object is not exposed.
- If status-event publishing fails after a terminal database transition, a retry sees the terminal
  status and republishes the status notification without mutating the source again.

---

## Operational process and cost controls

This feature should ship as a bounded, economical upload pipeline:

1. The client asks for an intent; the API authorizes the tenant, validates file metadata, creates a
   `Pending` source, and returns `sourceId` plus a short-lived presigned PUT.
2. The browser uploads directly to object storage. The API never proxies file bytes.
3. The client calls `upload-complete`; the API checks the object exists and that actual size/type
   still match the intent.
4. `Source.UploadStatus = Uploaded` becomes the durable state for the pending verification.
5. The QnA Portal API publishes `SourceUploadCompletedIntegrationEvent`; the QnA worker consumes it
   and calls the service layer, which dispatches `VerifyUploadedSourceCommand`.
6. The QnA worker command handler streams the object, validates content, scans for malware, and
   moves trusted bytes from `staging/` to `verified/`.
7. The worker publishes `SourceUploadStatusChangedIntegrationEvent`; the Portal API consumer calls
   `SourceUploadStatusChangedNotificationService`, which opens telemetry and sends
   `NotifySourceUploadStatusChangedCommand`. The command publishes the generic SignalR envelope to
   authorized Portal clients through `IPortalNotificationPublisher`.
8. Downloads are always private presigned GETs, and only for `Verified` objects under `verified/`.

Security and cost rules:

- Bucket/container is private. No public ACLs. CDN can be added later with private origin access.
- Upload URL TTL defaults to 10 minutes; download URL TTL defaults to 5 minutes.
- Pending intents older than 24 hours are expired by a sweeper: delete staging object if present and
  set `UploadStatus = Expired`.
- Store metadata in PostgreSQL and bytes in object storage. Do not store uploaded binaries in the
  relational database.
- Use shared bucket + tenant prefix by default. Dedicated buckets, tenant-managed keys, Object Lock,
  or longer retention are paid/regulatory tier features.
- Track per-tenant uploaded bytes, rejected bytes, verified bytes, egress, scanner failures, and
  pending-intent abandonment. These metrics are needed for quotas and cost attribution.
- Keep quarantine retention short and explicit. Unsafe objects have security value for triage but
  also increase privacy and storage exposure.

---

## Schema changes

Three additive columns plus one new enum and one partial unique index:

```
ALTER TABLE Sources ADD StorageKey nvarchar(1000) NULL;
ALTER TABLE Sources ADD SizeBytes bigint NULL;
ALTER TABLE Sources ADD UploadStatus int NOT NULL DEFAULT 1;

CREATE UNIQUE INDEX IX_Sources_TenantId_StorageKey
  ON Sources(TenantId, StorageKey)
  WHERE StorageKey IS NOT NULL;
```

`SourceUploadStatus` enum (Querify numeric allocation increments by five):

| Value | Meaning |
|---|---|
| `None = 1` | URL-based source; no upload involved. |
| `Pending = 6` | Intent issued; the client has not finalized PUT yet. |
| `Uploaded = 11` | Client confirmed via `upload-complete`; awaiting async verification. |
| `Verified = 16` | Worker validated checksum, size, and content type. |
| `Quarantined = 21` | Malware scanner or content policy marked the object unsafe. |
| `Failed = 26` | Verification rejected the upload and no quarantined artifact is retained. |
| `Expired = 31` | Intent expired before completion; staging object was deleted or absent. |

The Portal mirror (`apps/portal/src/shared/constants/backend-enums.ts`) must be updated in the
same change per [`../../behavior-change-playbook.md`](../../behavior-change-playbook.md) Step 3.4.

---

## Implementation steps

This implementation follows [`../../behavior-change-playbook.md`](../../behavior-change-playbook.md)
instead of treating upload as a one-off API feature.

| Step | Playbook alignment | Scope | Touches |
|---|---|---|---|
| 0 | Step 0 | Confirm staged delivery and capture handoff expectations. | This document |
| 1 | Steps 1-2 | Inventory existing `Source` behavior and normalize the upload concept. | `dotnet`, `apps`, `docs` search only |
| 2 | Supporting infrastructure | Add local S3-compatible storage and shared `IObjectStorage`. | `devops/local/docker/`, new `Querify.Common.Infrastructure.Storage` |
| 3 | Steps 3-5 | Update model contract, enum, DTOs, EF configuration, and manual migration notes. | `Querify.QnA.Common.Domain`, `Querify.Models.QnA`, `Querify.QnA.Common.Persistence.QnADb` |
| 4 | Step 6 | Add Portal API behavior, including the documented `upload-intent` command DTO exception. | `Querify.QnA.Portal.Business.Source`, `Querify.QnA.Portal.Api` |
| 5 | Step 6 | Add RabbitMQ worker consumers and worker commands for verification, scan, quarantine, status notification, and pending-expiry processing. Keep Hangfire available for unrelated future jobs. | `Querify.QnA.Worker.Api`, `Querify.QnA.Worker.Business.Source`, `Querify.QnA.Portal.Api` |
| 6 | Steps 7-8 | Add seed data and integration tests. | `Querify.Tools.Seed`, QnA Portal/Worker integration tests |
| 7 | Step 9 | Add Portal UI, API client hooks, enum presentation, and `en-US` copy keys. | `apps/portal/src/domains/sources/`, `en-US.json` |
| 8 | Steps 10-12 | Add all locale values, run targeted validation, and write final migration/deployment handoff. | locale files, build/test outputs, release notes |

Each implementation prompt below is self-contained. It names the documents the executor must read
before coding, the exact files to create/edit, the Querify rules in scope, and the validation
commands.

---

## Implementation step 0 prompt — Stage decision

```text
Querify monorepo. Required reading:
- docs/behavior-change-playbook.md (Step 0)

GOAL
Confirm that Source upload is staged before implementation begins.

DECISION
This change must be staged because it touches model contract, persistence,
Portal API behavior, object storage infrastructure, async processing, tests,
Portal UI, and localization.

STAGES
1. Inventory and concept normalization.
2. Storage infrastructure.
3. Model contract.
4. Backend API behavior.
5. Async worker behavior.
6. Seed and integration tests.
7. Portal frontend.
8. Localization, validation, and final handoff.

HANDOFF
Record the current stage, what builds, what is intentionally pending, and the
manual migration note after each implementation step.
```

---

## Implementation step 1 prompt — Inventory and concept normalization

```text
Querify monorepo. Required reading:
- docs/behavior-change-playbook.md (Steps 0-2)
- docs/backend/architecture/qna-domain-boundary.md
- docs/backend/architecture/repository-rules.md

GOAL
Inventory existing Source behavior and confirm that upload is represented as
Source-owned file ingestion, not a new artifact kind, not a separate module
workflow, and not a Trust validation shortcut.

DELIVERABLES

1) Run inventory searches:
   rg -n "Source|SourceRole|Locator|Checksum|MediaType|StorageKey|UploadStatus" dotnet apps docs
   rg --files dotnet/Querify.Models.QnA dotnet/Querify.QnA.Common.Domain dotnet/Querify.QnA.Common.Persistence.QnADb apps/portal/src/domains

2) Capture the current owning surfaces:
   - Source entity and EF configuration
   - Source DTOs
   - Source command/query handlers
   - Source controller/service
   - Source seed examples
   - Source tests
   - Portal source screens, hooks, API client, and locale keys

3) Confirm canonical concepts:
   - Upload artifact classification is represented by `MediaType`.
   - Source audience exposure is no longer part of the Source model.
   - Upload origin is represented by nullable `StorageKey`.
   - Upload workflow is represented by `SourceUploadStatus`.
   - `Locator` remains the current opaque locator and mirrors `StorageKey` for uploaded sources.

RULES
- Do not edit code in this step.
- Do not create placeholder entities.
- Do not add Trust-owned validation state to QnA.

HANDOFF
Write the inventory summary before starting the storage/model work.
```

---

## Implementation step 2 prompt — Storage infrastructure

```text
You are working in the Querify monorepo (.NET 10 + multi-tenant + microservices).
Required reading before coding:
- docs/backend/architecture/solution-architecture.md (section 8: Cross-cutting
  concerns / shared libraries)
- docs/backend/architecture/repository-rules.md
- docs/backend/architecture/dotnet-backend-overview.md (section "Shared
  infrastructure and persistence")

GOAL
Add shared S3-compatible object storage infrastructure without touching any
business module. Nothing in this implementation step consumes the new library.
The implementation uses single presigned PUT only; do not add multipart or
resumable abstractions.

DELIVERABLES

1) Local compose — devops/local/docker/docker-compose.baseservices.yml
   Add a `minio` service following the pattern of postgres/rabbitmq:
   - image: minio/minio:<pinned-RELEASE-tag> (do not commit `latest`)
   - container_name: minio
   - ports 5900:9000 (S3 API) and 5901:9001 (console)
   - environment MINIO_ROOT_USER=minio, MINIO_ROOT_PASSWORD=Pass123$$
   - named volume `minio` mounted at /data
   - command: server /data --console-address ":9001"
   - networks: qf-network, extra_hosts host.docker.internal
   - healthcheck via curl on /minio/health/live
   Add `minio:` to the `volumes:` section of the same compose file.
   Container-internal ports stay 9000/9001; only the host-published ports are
   5900/5901.

   Also add a one-shot `minio-init` service that creates the private bucket
   `querify-sources` (idempotent; uses a pinned minio/mc image, depends_on minio with
   condition: service_healthy). Configure bucket CORS for the local Portal
   origins so browser PUT/HEAD/GET requests with `Content-Type` and S3
   checksum/encryption headers work against MinIO. Do not make the bucket
   anonymous/public.

2) New shared project — dotnet/Querify.Common.Infrastructure.Storage/
   Create csproj net10.0, namespace Querify.Common.Infrastructure.Storage.
   Add to Querify.sln.
   NuGet dependency: AWSSDK.S3 (the SDK speaks MinIO/R2/S3 with
   ForcePathStyle).

   Folder structure (follow other Querify.Common.Infrastructure.* projects):
   - Abstractions/IObjectStorage.cs
   - Options/ObjectStorageOptions.cs    (Endpoint, PublicEndpoint?, Region,
     AccessKey, SecretKey, Bucket, ForcePathStyle, UploadPresignTtlMinutes,
     DownloadPresignTtlMinutes, ServerSideEncryptionMode?)
   - Services/S3ObjectStorage.cs
   - Extensions/ServiceCollectionExtensions.cs
                                        (AddObjectStorage(IConfiguration))

   IObjectStorage minimal signatures:
     Task<PresignedPutResult> PresignPutAsync(string key, string contentType,
         long expectedSizeBytes, CancellationToken ct);
     Task<Uri> PresignGetAsync(string key, TimeSpan ttl,
         CancellationToken ct);
     Task<ObjectMetadata?> HeadAsync(string key, CancellationToken ct);
     Task CopyAsync(string sourceKey, string destinationKey,
         CancellationToken ct);
     Task DeleteAsync(string key, CancellationToken ct);
     Task<Stream> OpenReadAsync(string key, CancellationToken ct);

   PresignedPutResult: { Uri Url, IReadOnlyDictionary<string,string>
       RequiredHeaders, DateTime ExpiresAtUtc }
   ObjectMetadata: { long SizeBytes, string ContentType, string ETag,
       DateTime LastModifiedUtc }

   AddObjectStorage:
   - services.AddOptions<ObjectStorageOptions>()
       .BindConfiguration("ObjectStorage")
       .ValidateDataAnnotations().ValidateOnStart();
   - register IAmazonS3 as singleton with ForcePathStyle=true and
     ServiceURL=Endpoint
   - presigned PUT/GET URLs must use PublicEndpoint when configured; this is
     required when the API runs in Docker via `http://minio:9000` but the
     browser must upload to `http://localhost:5900`
   - services.AddSingleton<IObjectStorage, S3ObjectStorage>();

3) QnA Portal API configuration (`appsettings.json`, or
   `appsettings.Development.json` if this step adds an environment-specific
   file):
   "ObjectStorage": {
     "Endpoint": "http://localhost:5900",
     "PublicEndpoint": "http://localhost:5900",
     "Region": "us-east-1",
     "AccessKey": "minio",
     "SecretKey": "Pass123$$",
     "Bucket": "querify-sources",
     "ForcePathStyle": true,
     "UploadPresignTtlMinutes": 10,
     "DownloadPresignTtlMinutes": 5,
     "ServerSideEncryptionMode": null
   }

   When the QnA Portal API runs inside docker-compose.backend.yml, override
   `ObjectStorage:Endpoint` to `http://minio:9000` for backend/worker storage
   operations, but keep `ObjectStorage:PublicEndpoint` as
   `http://localhost:5900` so the frontend receives browser-reachable
   presigned URLs.

NON-NEGOTIABLE RULES
- Do not touch Querify.QnA business/domain/persistence projects,
  Querify.Tenant.*, Querify.Direct.*, or Querify.Broadcast.* — this step is
  infrastructure only. QnA Portal host configuration may be updated only for
  `ObjectStorage`.
- No comments explaining what code does; XML doc only on public surfaces
  (IObjectStorage, ObjectStorageOptions).
- Do not run EF migrations.
- Do not introduce parallel helpers (BlobService, FileService); IObjectStorage
  is the single entry point.
- Do not introduce multipart or resumable APIs in this implementation.
- Presigned PUT size enforcement is defense-in-depth: validate declared size
  before signing, then validate actual size with HEAD after upload. Do not
  claim the presigned URL alone enforces max bytes for every S3-compatible
  provider.

VALIDATION
- dotnet build dotnet/Querify.Common.Infrastructure.Storage -v minimal
- docker compose -f devops/local/docker/docker-compose.baseservices.yml up -d
    minio minio-init
- curl http://localhost:5900/minio/health/live → 200
- console at http://localhost:5901 opens; bucket querify-sources exists and is
  private

HANDOFF
List of projects that build, bucket creation confirmation, next step: model
contract for Source upload.
```

---

## Implementation step 3 prompt — Model contract

```text
Querify monorepo. Required reading:
- docs/behavior-change-playbook.md (Steps 1–5)
- docs/backend/architecture/qna-domain-boundary.md
- docs/backend/architecture/repository-rules.md (Module model contract
  boundary)

CONTEXT
Source today supports only external sources via `Locator`. We are adding
support for tenant-uploaded files. Storage layer already exists (implementation step 2:
Querify.Common.Infrastructure.Storage with IObjectStorage). This step is
model + DTOs + EF only; no handlers, controllers, or frontend.

DESIGN PRINCIPLES
- Keep `Locator` as the opaque key: external URL for URL sources, storage
  key for uploaded sources.
- Do not reintroduce an artifact-kind field; the content type and upload
  status describe uploaded artifacts, and origin is derived from the presence
  of `StorageKey`.
- Source remains anemic (repository-rules §6).

DELIVERABLES

1) dotnet/Querify.QnA.Common.Domain/Entities/Source.cs
   Add persisted properties (with mandatory XML doc explaining usage,
   playbook Step 3):
   - string? StorageKey { get; set; }   (max 1000; nullable for URL sources)
   - long? SizeBytes { get; set; }      (expected size while Pending,
                                         confirmed size after complete)
   - SourceUploadStatus UploadStatus { get; set; } = SourceUploadStatus.None;

   New constant: public const int MaxStorageKeyLength = 1000;
   Do not duplicate CreatedDate/UpdatedDate (already in BaseEntity).

2) dotnet/Querify.Models.QnA/Enums/SourceUploadStatus.cs (NEW)
   Enum with Querify numeric allocation (1,6,11,16,21,26,31 — playbook Step 3.4),
   each value with an XML summary explaining behavior (not naming):
   - None = 1            (URL/external source; no upload associated)
   - Pending = 6         (intent issued; PUT not yet confirmed)
   - Uploaded = 11       (client PUT succeeded; awaiting async processing)
   - Verified = 16       (worker validated checksum/size/MIME)
   - Quarantined = 21    (malware/content policy retained unsafe artifact)
   - Failed = 26         (upload rejected and staging object removed)
   - Expired = 31        (intent expired before completion)
   Also add the enum to apps/portal/src/shared/constants/backend-enums.ts in
   the same change (playbook Step 3.4).

3) dotnet/Querify.Models.QnA/Dtos/Source/ — NEW DTOs (feature-folder layout,
   repository-rules §7):
   - SourceUploadIntentRequestDto.cs:
       string FileName, string ContentType, long SizeBytes, string Language,
       string? Label, string? ContextNote, string? ExternalId,
       string? MetadataJson
   - SourceUploadIntentResponseDto.cs:
       Guid SourceId, string UploadUrl,
       IReadOnlyDictionary<string,string> RequiredHeaders, string StorageKey,
       DateTime ExpiresAtUtc
   - SourceUploadCompleteRequestDto.cs:
       string? ClientChecksum (sha256:hex optional)
   - SourceDownloadUrlDto.cs:
       string Url, DateTime ExpiresAtUtc

   ALSO update:
   - SourceDto.cs: add StorageKey, SizeBytes, UploadStatus.
   - SourceCreateRequestDto.cs: do NOT add upload fields; URL-creation flow
     stays flat and separate (write DTOs are flat per repository-rules).

4) dotnet/Querify.QnA.Common.Persistence.QnADb/Configurations/SourceConfiguration.cs
   Add property configuration for the 3 new fields:
   - StorageKey: HasMaxLength(Source.MaxStorageKeyLength), nullable
   - SizeBytes: nullable long
   - UploadStatus: HasDefaultValue(SourceUploadStatus.None).IsRequired()
   Add partial unique index: (TenantId, StorageKey) WHERE
   StorageKey IS NOT NULL (prevents duplicate StorageKey within the tenant).

5) dotnet/Querify.QnA.Common.Domain/BusinessRules/Sources/SourceStorageKey.cs
   (NEW)
   Pure static class (no DbContext dependency, playbook Step 3.16):
     public static string BuildStagingKey(Guid tenantId, Guid sourceId,
         string fileName)
     public static string BuildVerifiedKey(Guid tenantId, Guid sourceId,
         string fileName)
     public static string ToVerifiedKey(string stagingKey)
     public static string ToQuarantineKey(string stagingKey)
   Sanitize fileName (strip paths, keep [a-zA-Z0-9._-]). Format:
     "{tenantId}/sources/{sourceId}/staging/{sanitized}"
     "{tenantId}/sources/{sourceId}/verified/{sanitized}"
     "{tenantId}/sources/{sourceId}/quarantine/{sanitized}"

6) dotnet/Querify.QnA.Common.Domain/BusinessRules/Sources/SourceRules.cs
   Add EnsureStorageKeyIsDownloadable(Source source) used by download-url:
   StorageKey must be non-null, UploadStatus must be Verified, and the key
   must contain `/verified/`.

7) dotnet/Querify.QnA.Common.Domain/Options/SourceUploadOptions.cs
   Shared options consumed by Portal handlers and the worker:
   - MaxUploadBytes = 52428800
   - PendingExpirationHours = 24
   - AllowedContentTypes / extension mapping if the repository has an options
     pattern for allowlists; otherwise keep the first allowlist in SourceRules.

NON-NEGOTIABLE RULES
- Do NOT run or generate EF migrations (playbook). Leave a manual migration
  note at the end.
- Do NOT remove `required` modifiers or add silent defaults.
- Source stays anemic — no behavior methods.
- Source.cs MUST have XML doc on every new persisted property.
- Tenant integrity: StorageKey does not reference another tenant-owned
  entity, so do NOT create a new DbContext/TenantIntegrity extension
  (playbook Step 3.20).
- Do not touch Direct, Broadcast, or Tenant projects.

VALIDATION
dotnet build dotnet/Querify.Models.Common -v minimal
dotnet build dotnet/Querify.Models.QnA -v minimal
dotnet build dotnet/Querify.QnA.Common.Domain -v minimal
dotnet build dotnet/Querify.QnA.Common.Persistence.QnADb -v minimal

HANDOFF (playbook Step 12)
- Canonical concepts added: SourceUploadStatus, Source.StorageKey,
  Source.SizeBytes, Source.UploadStatus.
- Manual migration pending:
    ALTER TABLE Sources ADD StorageKey nvarchar(1000) NULL;
    ALTER TABLE Sources ADD SizeBytes bigint NULL;
    ALTER TABLE Sources ADD UploadStatus int NOT NULL DEFAULT 1;
    CREATE UNIQUE INDEX IX_Sources_TenantId_StorageKey
      ON Sources(TenantId, StorageKey) WHERE StorageKey IS NOT NULL;
- Nothing intentionally broken.
- Next step: handlers + controller (implementation step 4).
```

---

## Implementation step 4 prompt — Backend behavior (Portal)

> Historical note: this prompt was written before the event-driven verification change. The current
> implementation publishes `SourceUploadCompletedIntegrationEvent` after upload completion instead of
> relying on the old recurring Hangfire verification sweep.

```text
Querify monorepo. Required reading:
- docs/backend/architecture/solution-cqrs-write-rules.md
- docs/backend/architecture/repository-rules.md (CQRS write contract, Folder
  ownership rules, API write response conventions, API error conventions)
- docs/behavior-change-playbook.md (Step 6)

CONTEXT
- Implementation step 2 delivered Querify.Common.Infrastructure.Storage with IObjectStorage.
- Implementation step 3 added StorageKey/SizeBytes/UploadStatus on Source plus the new DTOs.
- This step adds the upload flow (intent with presigned URL → direct PUT →
  complete) to the Portal API (Querify.QnA.Portal.Business.Source) only.
  Public API does NOT receive uploads (multi-tenant principle: public is
  read-only).
- Async verification is triggered by the QnA worker's RabbitMQ `SourceUploadCompleted` consumer in
  implementation step 5. `Source.UploadStatus = Uploaded` is persisted state, but not the primary
  polling trigger.
- The implementation uses single presigned PUT only. Do not add multipart or
  resumable contracts.
- `upload-intent` is the only documented exception to the playbook rule that
  commands return simple values. The exception exists because the returned
  presigned URL is a short-lived credential derived from the newly-created
  intent, is not persisted, and is required for the immediate next user step.

DELIVERABLES

In dotnet/Querify.QnA.Portal.Business.Source/, mirror the existing command
folder layout:

Shared options:
- Bind `SourceUploadOptions` from implementation step 3 in the Portal API host.

1) Commands/CreateUploadIntent/
   - SourcesCreateUploadIntentCommand.cs
       : IRequest<SourceUploadIntentResponseDto>
       { SourceUploadIntentRequestDto Dto }
   - SourcesCreateUploadIntentCommandHandler.cs

   Handler:
   - tenantId via ISessionService.GetTenantId(ModuleEnum.QnA)
   - validate extension + ContentType allowlist (application/pdf, image/png,
     image/jpeg, video/mp4, text/plain, text/markdown). 422 on mismatch.
   - add SourceUploadOptions with MaxUploadBytes default 50 MB, bind from
     configuration, and validate 0 < SizeBytes <= MaxUploadBytes.
   - sourceId = Guid.NewGuid()
   - stagingKey = SourceStorageKey.BuildStagingKey(tenantId, sourceId,
     fileName)
   - new Source { Id = sourceId, UploadStatus = Pending,
       StorageKey = stagingKey,
       Locator = stagingKey,
       Checksum = SourceChecksum.FromLocator(stagingKey),
       Language, Label, ContextNote, ExternalId, MetadataJson,
       MediaType = request.ContentType, SizeBytes = request.SizeBytes,
       TenantId, CreatedBy, UpdatedBy }
   - dbContext.Sources.Add; SaveChangesAsync
   - presign = await objectStorage.PresignPutAsync(stagingKey, contentType,
       request.SizeBytes, ct)
   - return SourceUploadIntentResponseDto { SourceId, UploadUrl,
       RequiredHeaders, StorageKey, ExpiresAtUtc }

2) Commands/CompleteUpload/
   - SourcesCompleteUploadCommand.cs   : IRequest<Guid>
       { Guid SourceId, string? ClientChecksum }
   - SourcesCompleteUploadCommandHandler.cs

   Handler (orchestration blocks per repository-rules §3):
   - Load Source by (TenantId, Id); 404 if missing.
   - 409 Conflict if UploadStatus != Pending.
   - head = await objectStorage.HeadAsync(entity.StorageKey, ct).
   - 422 if head == null ("Upload not received").
   - if head.SizeBytes != entity.SizeBytes or head.SizeBytes >
     SourceUploadOptions.MaxUploadBytes: DeleteAsync(entity.StorageKey), set
     Failed, save, 422.
   - if head.ContentType is missing, not allowlisted, or
     does not match entity.MediaType: DeleteAsync(entity.StorageKey), set
     Failed, save, 422.
   - entity.SizeBytes = head.SizeBytes
   - entity.MediaType = head.ContentType
   - entity.Checksum = request.ClientChecksum ?? SourceChecksum.FromLocator(entity.Locator)
   - entity.UploadStatus = SourceUploadStatus.Uploaded
   - entity.UpdatedBy = userId
   - SaveChangesAsync. After the database save succeeds, publish
     SourceUploadCompletedIntegrationEvent. If publishing fails after commit, a future reconciliation
     job can find stuck `Uploaded` staging sources.
   - return entity.Id

3) Queries/GetDownloadUrl/
   - SourcesGetDownloadUrlQuery.cs
       : IRequest<SourceDownloadUrlDto> { Guid Id }
   - SourcesGetDownloadUrlQueryHandler.cs
   - AsNoTracking, Select { StorageKey, UploadStatus, TenantId }.
   - Filter by TenantId; 404 if missing.
   - 422 if StorageKey == null (URL source: client should use Locator
     directly).
   - 422 if UploadStatus != Verified.
   - 422 if StorageKey does not contain `/verified/`.
   - presign GET with configurable TTL (5 min default).
   - return { Url, ExpiresAtUtc }.

4) Service/SourceService.cs + Abstractions/ISourceService.cs
   Add thin delegating methods:
     Task<SourceUploadIntentResponseDto> CreateUploadIntent(
         SourceUploadIntentRequestDto dto, CancellationToken token);
     Task<Guid> CompleteUpload(Guid sourceId,
         SourceUploadCompleteRequestDto dto, CancellationToken token);
     Task<SourceDownloadUrlDto> GetDownloadUrl(Guid id,
         CancellationToken token);

5) Controllers/SourceController.cs
   Add 3 endpoints (kebab-case route segments per repository-rules §3):
     POST /api/qna/source/upload-intent
       → 200 + SourceUploadIntentResponseDto
     POST /api/qna/source/{id:guid}/upload-complete
       → 200 + Guid
     GET  /api/qna/source/{id:guid}/download-url
       → 200 + SourceDownloadUrlDto
   Use [Authorize] + [ProducesResponseType] for each status code.

6) Querify.QnA.Portal.Api/Extensions/ServiceCollectionExtensions.cs:
   Ensure AddObjectStorage(configuration) is called once in the host
   composition root (solution-architecture §1).
   Add project reference: Querify.Common.Infrastructure.Storage in
   Querify.QnA.Portal.Business.Source.csproj.
   Bind SourceUploadOptions from "SourceUpload":
     MaxUploadBytes = 52428800
     PendingExpirationHours = 24

7) devops/local/docker/docker-compose.backend.yml
   When `querify.qna.portal.api` runs in Docker, set:
   - ObjectStorage__Endpoint=http://minio:9000
   - ObjectStorage__PublicEndpoint=http://localhost:5900
   The first value is for backend-to-MinIO operations on the Docker network.
   The second value is for presigned URLs returned to the browser.

RULES
- Controller is HTTP-only.
- Service is thin and delegates via MediatR.
- Commands return Guid except `CreateUploadIntent`, which returns
  `SourceUploadIntentResponseDto` as the documented presigned URL exception.
  Queries return DTOs.
- Use ApiErrorException with the appropriate HttpStatusCode.
- AsNoTracking + Select projection in queries.
- Do not touch Querify.QnA.Public.* — upload is portal-only.
- No multipart or resumable code in this implementation.

VALIDATION
dotnet build dotnet/Querify.QnA.Portal.Business.Source
dotnet build dotnet/Querify.QnA.Portal.Api

HANDOFF
- New endpoints: upload-intent, upload-complete, download-url.
- UploadStatus reaches Uploaded; Verified depends on implementation step 5.
- Tests pending (implementation step 6).
```

---

## Implementation step 5 prompt — Worker async verification, scan, and quarantine

> Historical note: this prompt originally used Hangfire as the primary verification trigger. The
> current implementation keeps Hangfire ready for future jobs, but source-upload verification is
> RabbitMQ-driven through the QnA worker consumer.

```text
Querify monorepo. Required reading:
- docs/backend/architecture/querify-tenant-worker.md
- docs/backend/architecture/solution-architecture.md (§7 control-plane
  workers)
- Querify.Common.Infrastructure.Hangfire for persisted Hangfire registration

CONTEXT
After upload-complete (implementation step 4), Source is in UploadStatus.Uploaded. We need
async processing to:
1. Recompute SHA-256 from real bytes (not from the locator string).
2. Confirm SizeBytes and MediaType (defense-in-depth against HEAD lying).
3. Validate magic bytes / real file family against the allowlist.
4. Run malware scanning before any uploaded file becomes downloadable.
5. Copy trusted bytes from `staging/` to `verified/`, delete staging, and set
   `UploadStatus = Verified`.
6. Move unsafe bytes to `quarantine/` and set UploadStatus = Quarantined.

ARCHITECTURAL DECISION
Source belongs to the QnA module, but the playbook
(querify-tenant-worker.md) requires that Tenant.Worker.Api host only
control-plane work, not product workflows. Create a new project:
Querify.QnA.Worker.Api, mirroring the tenant worker layout.

Use persisted Hangfire, not in-memory jobs. `Source.UploadStatus = Uploaded` is the recoverable
backlog if enqueue/registration is interrupted.

DELIVERABLES (assuming a dedicated worker)

1) New project: dotnet/Querify.QnA.Worker.Api/
   - Program.cs: generic IHost, AddHangFire with PostgreSQL storage and queue
     `qna-source-upload`
   - AddObjectStorage, AddDbContext<QnADbContext>
   - tenant connection resolution mirroring Querify.Tenant.Worker.Api
   - appsettings with ObjectStorage + SourceUpload + SourceUpload:ThreatScanningMode
     + ConnectionStrings + HangFire
   - Dockerfile (mirror Querify.Tenant.Worker.Api/Dockerfile)

2) New project: dotnet/Querify.QnA.Worker.Business.Source/
   - BackgroundServices/SourceUploadVerificationBackgroundService.cs
       Hangfire adapter only; calls SourceUploadVerificationSweepService
   - Services/SourceUploadVerificationSweepService.cs
       owns telemetry and sends VerifyUploadedSourcesForAllTenantsCommand
   - Commands/VerifyUploadedSourcesForAllTenants/
       finds active QnA tenants and uploaded staging sources, then dispatches
       VerifyUploadedSourceCommand
   - Commands/VerifyUploadedSource/VerifyUploadedSourceCommand.cs
       : IRequest<Guid>
       { Guid TenantId, Guid SourceId, string StorageKey }
   - Commands/VerifyUploadedSource/VerifyUploadedSourceCommandHandler.cs
   - Commands/ExpirePendingSourceUploads/ExpirePendingSourceUploadsCommand.cs
       : IRequest<bool>
       { DateTime NowUtc }
   - Commands/ExpirePendingSourceUploads/ExpirePendingSourceUploadsCommandHandler.cs
   - Processor services coordinate telemetry and dispatch commands; hosted services do not
     own Source upload workflow behavior directly.
   - Abstractions/IUploadThreatScanner.cs
   - Services/NoopUploadThreatScanner.cs (Development only)
   - Common Domain BusinessRules/Sources/SourceUploadContentInspector.cs (magic-byte validation)
   - HostedServices/PendingSourceUploadExpiryHostedService.cs

   Hangfire flow:
   - do not put verification business logic in the Hangfire background job class
   - BackgroundService (Hangfire) calls SourceUploadVerificationSweepService
   - SourceUploadVerificationSweepService opens telemetry and sends MediatR commands
   - transient failures propagate so Hangfire retries according to persisted job state

   VerifyUploadedSourceCommandHandler flow:
   - resolve QnADbContext for the command's TenantId (use existing
     Querify.Common.EntityFramework.Tenant resolution)
   - load Source; idempotent — return if UploadStatus != Uploaded
   - stagingKey = command.StorageKey; reject as Failed if it does not contain
     `/staging/`
   - using IObjectStorage.OpenReadAsync(stagingKey), compute SHA-256 in
     stream (do NOT materialize the full blob; use IncrementalHash)
   - re-confirm SizeBytes via HeadAsync
   - validate magic bytes and real type against entity.MediaType
   - run IUploadThreatScanner. In Development, NoopUploadThreatScanner is
     allowed only when SourceUpload:ThreatScanningMode=Noop. In production,
     fail startup unless a real scanner mode is configured.
   - if a client checksum was provided and does not match: UploadStatus =
     Failed, DeleteAsync(stagingKey), log warning, persist, return without
     rethrowing (no retry).
   - if magic/type validation fails: UploadStatus = Failed,
     DeleteAsync(stagingKey), persist, return without throwing.
   - if malware/unsafe content is detected: quarantineKey =
     SourceStorageKey.ToQuarantineKey(stagingKey); CopyAsync(stagingKey,
     quarantineKey); DeleteAsync(stagingKey); entity.StorageKey =
     quarantineKey; entity.Locator = quarantineKey; entity.UploadStatus =
     SourceUploadStatus.Quarantined; persist, return without throwing.
   - otherwise: verifiedKey = SourceStorageKey.ToVerifiedKey(stagingKey);
     CopyAsync(stagingKey, verifiedKey); DeleteAsync(stagingKey);
     entity.StorageKey = verifiedKey; entity.Locator = verifiedKey;
     entity.Checksum = "sha256:..."; entity.SizeBytes = head.SizeBytes;
     entity.UploadStatus = SourceUploadStatus.Verified;
     entity.UpdatedBy = "system:qna-worker"
   - SaveChangesAsync

   PendingSourceUploadExpiryHostedService:
   - runs on a conservative interval (default 1 hour)
   - does not mutate Source directly
   - sends ExpirePendingSourceUploadsCommand through MediatR

   ExpirePendingSourceUploadsCommandHandler:
   - finds Pending sources older than SourceUpload:PendingExpirationHours
     (default 24)
   - deletes staging objects when present
   - sets UploadStatus = Expired and UpdatedBy = "system:qna-worker"
   - returns the number of expired sources

3) Service registration extension in
   Querify.QnA.Worker.Business.Source/Extensions/ServiceCollectionExtensions.cs:
   AddSourceWorker(IConfiguration) registers Hangfire-callable background service classes,
   telemetry-owning services, commands, options, scanner, and pending-expiry hosted service.

4) Add both csprojs to Querify.sln.

5) devops/local/docker/docker-compose.backend.yml — add
   `querify.qna.worker.api` service following the pattern of other backend
   services (build context, depends_on postgres/minio, env vars).
   Set:
   - ObjectStorage__Endpoint=http://minio:9000
   - ObjectStorage__PublicEndpoint=http://localhost:5900

RULES
- Worker does not perform read-after-write on entities of other modules.
- Hangfire background job classes are adapters only; services own telemetry and commands own
  business behavior.
- VerifyUploadedSourceCommandHandler is idempotent (filter by UploadStatus ==
  Uploaded and the expected staging StorageKey before terminal transitions).
- Do not materialize the full blob — streaming SHA-256 only.
- Transient errors (storage 5xx) propagate for Hangfire retry. Permanent errors
  (checksum mismatch, MIME mismatch) flip to
  Failed or Quarantined and return without throwing.
- QnADbContext is tenant-scoped — connection string must be resolved from
  the command TenantId before save.
- A source is downloadable only after the worker updates its key to
  `/verified/`.

VALIDATION
dotnet build dotnet/Querify.QnA.Worker.Api
dotnet build dotnet/Querify.QnA.Worker.Business.Source
docker compose -f devops/local/docker/docker-compose.baseservices.yml up -d
  postgres minio minio-init
docker compose -f devops/local/docker/docker-compose.backend.yml up -d
  querify.qna.worker.api
- Run /api/qna/source/upload-intent → PUT to returned presigned URL →
  upload-complete
- Inspect Hangfire job state, command logs, and Sources.UploadStatus column.

HANDOFF
- Text extraction for AI indexing (depends on AI roadmap).
- Optional managed/provider-native malware scanner integration if the current
  implementation starts with ClamAV.
- Lifecycle/retention policy for quarantine artifacts.
```

---

## Implementation step 6 prompt — Seed and integration tests

```text
Querify monorepo. Required reading:
- docs/behavior-change-playbook.md (Steps 7–8)
- docs/backend/testing/integration-testing-strategy.md
- docs/backend/tools/seed-tool.md

CONTEXT
Implementation steps 3-5 delivered the full upload flow. This step adds seed examples
and integration coverage.

DELIVERABLES

1) dotnet/Querify.Tools.Seed/Application/QnASeedCatalog.*.cs
   Locate the existing Source catalog file. Add 2 deterministic Source
   examples with simulated upload state:
   - "Manual de produto.pdf" — StorageKey under `/verified/`,
     SizeBytes, UploadStatus=Verified, deterministic SHA-256 Checksum
   - "Política de privacidade.pdf" — second example
   The seed must NOT actually upload to MinIO; it only populates the entity
   with synthetic StorageKey and Checksum. (Seed is data-only — it does not
   touch external systems.)

2) dotnet/Querify.QnA.Portal.Test.IntegrationTests/Tests/Source/
   SourceUploadCommandQueryTests.cs (real PostgreSQL, mirroring the
   project's existing test patterns):
   - CreateUploadIntent_ValidPdf_CreatesPendingSourceAndReturnsPresignedUrl
   - CreateUploadIntent_InvalidContentType_Returns422
   - CreateUploadIntent_OversizeBytes_Returns422
   - CreateUploadIntent_PublicVisibility_Returns422
   - CompleteUpload_NoBlobInStorage_Returns422
   - CompleteUpload_SizeMismatch_DeletesStagingObjectAndReturns422
   - CompleteUpload_ContentTypeMismatch_DeletesStagingObjectAndReturns422
   - CompleteUpload_StatusNotPending_Returns409
   - CompleteUpload_HappyPath_TransitionsToUploaded
   - GetDownloadUrl_UrlSource_Returns422
   - GetDownloadUrl_UploadedSource_Returns422
   - GetDownloadUrl_VerifiedSource_ReturnsPresignedGet
   - GetDownloadUrl_QuarantinedSource_Returns422
   - CrossTenant_SourceVisibility_NotLeaked  (cross-tenant regression)

   IObjectStorage is replaced by an in-memory fake ONLY because storage is
   not the boundary under test. Document this as an explicit exception. For
   real end-to-end MinIO tests, gate with [Trait("Category",
   "RequiresMinio")].

3) dotnet/Querify.QnA.Portal.Test.IntegrationTests/Common/Factories/
   SourceFactory.cs (update or create):
   CreateVerifiedUploadedSource(Guid tenantId, ...) producing entity with
   `/verified/` StorageKey + UploadStatus=Verified + valid Checksum.

4) dotnet/Querify.QnA.Worker.Test.IntegrationTests/ (NEW project mirroring
   Querify.Tenant.Worker.Test.IntegrationTests):
   Tests for SourceUploadedConsumer:
   - EventConsumer_MapsEventToVerifyUploadedSourceCommand

   Tests for VerifyUploadedSourceCommandHandler:
   - HappyPath_CopiesStagingToVerifiedDeletesStagingAndTransitionsToVerified
   - ChecksumMismatch_TransitionsToFailed
   - MagicBytesMismatch_TransitionsToFailed
   - MalwareDetected_TransitionsToQuarantined
   - StatusNotUploaded_IsIdempotent

   Tests for ExpirePendingSourceUploadsCommandHandler:
   - ExpiredPendingSource_DeletesStagingObjectAndTransitionsToExpired
   - FreshPendingSource_IsIgnored

RULES
- Use real DB and real EF migrations (testing-strategy).
- Do not mock SessionService for tenant context — use the existing context
  builder.
- Each test independent (use the established WebApplicationFactory pattern).

VALIDATION
dotnet test dotnet/Querify.QnA.Portal.Test.IntegrationTests
dotnet test dotnet/Querify.QnA.Worker.Test.IntegrationTests
dotnet run --project dotnet/Querify.Tools.Seed
- After seed, list /api/qna/source on the Portal API and verify both
  examples carry StorageKey + UploadStatus=Verified.

HANDOFF
- Coverage: Portal command/query covered. Worker Hangfire adapters and worker
  command handlers covered.
- Pending: i18n, frontend.
```

---

## Implementation step 7 prompt — Portal frontend (sources domain)

```text
Querify monorepo. Required reading:
- docs/frontend/architecture/portal-app.md
- docs/frontend/architecture/portal-app-ui-prompt-guidance.md
- docs/behavior-change-playbook.md (Step 9)

CONTEXT
Backend (implementation steps 3-5) exposes:
  POST /api/qna/source/upload-intent           → SourceUploadIntentResponseDto
  POST /api/qna/source/{id}/upload-complete    → Guid
  GET  /api/qna/source/{id}/download-url       → SourceDownloadUrlDto

Source has new fields: StorageKey, SizeBytes, UploadStatus.

Existing frontend domain: apps/portal/src/domains/sources/

DELIVERABLES

1) apps/portal/src/domains/sources/types.ts
   - Add types: SourceUploadIntentRequestDto,
     SourceUploadIntentResponseDto, SourceUploadCompleteRequestDto,
     SourceDownloadUrlDto, SourceUploadStatus (mirror Querify enum).
   - Update SourceDto with StorageKey, SizeBytes, UploadStatus.

2) apps/portal/src/domains/sources/api.ts
   Three new functions following the existing portalRequest pattern:
   - createSourceUploadIntent(accessToken, tenantId, body)
   - completeSourceUpload(accessToken, tenantId, id, body)
   - getSourceDownloadUrl(accessToken, tenantId, id)

3) apps/portal/src/domains/sources/hooks.ts
   - useCreateSourceUploadIntent (mutation)
   - useCompleteSourceUpload (mutation)
   - useSourceDownloadUrl(id) (query, enabled=Boolean(id))
   - Invalidate ['sources','list'] and ['sources', id] on completeSourceUpload
     onSuccess.

4) apps/portal/src/domains/sources/upload-flow.ts (NEW)
   Pure function uploadSourceFile({ file, intentResponse }) that:
   - performs `await fetch(intentResponse.uploadUrl, { method: 'PUT', body:
       file, headers: intentResponse.requiredHeaders })`
   - throws a typed error on non-2xx
   - reports progress via optional onProgress callback using XHR
     ProgressEvent (fetch does not expose upload progress)
   - sends exactly the headers returned by the API. Do not add multipart,
     chunking, or resumable behavior.

5) apps/portal/src/domains/sources/source-form-page.tsx
   Add an "upload" mode alongside the existing "external URL" mode in the
   create form:
   - segmented control: "External URL" | "File upload"
   - upload mode: <input type="file"> + name/size/MIME preview
   - on submit: createSourceUploadIntent → uploadSourceFile (with progress
     state) → completeSourceUpload → redirect to detail.
   - Follow portal-app-ui-prompt-guidance:
     - shared/ui components (Button, Card, Input, Select)
     - states: loading/empty/error/pending/success/destructive
     - mobile: card stack below xl
     - light + dark themes

6) apps/portal/src/domains/sources/source-detail-page.tsx
   - Show UploadStatus as a badge (use shared/constants/enum-ui.ts).
   - If StorageKey != null and UploadStatus == Verified: a "Download" button
     that calls useSourceDownloadUrl and navigates to the returned URL.
   - For Pending/Uploaded, show the status without offering download.
   - For Failed/Expired/Quarantined, show the status and no download action.
   - Show SizeBytes formatted (KB/MB) when present.

7) apps/portal/src/domains/sources/source-list-page.tsx
   - New "Origin" column: "URL" | "File" based on StorageKey != null.
   - New filter on UploadStatus (Select backed by the enum).

8) apps/portal/src/shared/constants/backend-enums.ts
   Add SourceUploadStatus mapped to the same numeric values as the .NET
   enum (1, 6, 11, 16, 21, 26, 31).

9) apps/portal/src/shared/constants/enum-ui.ts
   Add presentation metadata for SourceUploadStatus:
   None=ghost, Pending=warning, Uploaded=info, Verified=success,
   Quarantined=destructive, Failed=destructive, Expired=muted.

RULES
- TypeScript types mirror .NET DTOs exactly.
- Do not concatenate translated copy in components (i18n in the next
  implementation step).
- Every editable field must have `description`, `hint`, or visible label
  with ContextHint.
- No hardcoded English copy — use t() with placeholder keys and add the key
  to en-US.json. Implementation step 8 propagates to other locales.
- No horizontal overflow at 320, 360, 768, 1024, 1280.
- Schema validation via zod (existing *-page.tsx pattern).

VALIDATION
cd apps/portal && npm run lint && npm run build
Manual:
- "File upload" tab creates a source.
- List shows Origin=File + Status=Uploaded/Verified.
- Detail shows Download.

HANDOFF
- Frontend functional in en-US. Other locales pending.
```

---

## Implementation step 8 prompt — Localization and final handoff

```text
Querify monorepo. Required reading:
- docs/frontend/architecture/portal-localization.md
- docs/behavior-change-playbook.md (Steps 10-12)

CONTEXT
Implementation step 7 introduced new copy keys for the sources domain, all with
en-US placeholder values. This step propagates them to all 20 locales:
en-US, ar-SA, bn-BD, de-DE, es-ES, fr-FR, he-IL, hi-IN, id-ID, it-IT,
ja-JP, ko-KR, pl-PL, pt-BR, ru-RU, th-TH, tr-TR, ur-PK, vi-VN, zh-CN.

DELIVERABLES

1) Audit apps/portal/src/shared/lib/i18n/locales/en-US.json and list every
   new key added in implementation step 7 (look for sources.upload.*, sources.download.*,
   enums.SourceUploadStatus.*).

2) For every locale (except en-US), add EXACTLY the same keys with native
   translations. Do not copy the English value verbatim (Step 10.7) unless
   the term is universally untranslated (e.g. "PDF").

3) Ensure every locale file has exactly the same key set (Step 10.8). Use a
   diff script if needed.

4) Final key tree (example):
   sources.upload.title
   sources.upload.button.start
   sources.upload.button.cancel
   sources.upload.dropzone.hint
   sources.upload.progress.label
   sources.upload.error.contentType
   sources.upload.error.size
   sources.upload.error.network
   sources.download.button
   sources.list.column.origin
   sources.list.origin.url
   sources.list.origin.file
   enums.SourceUploadStatus.None
   enums.SourceUploadStatus.Pending
   enums.SourceUploadStatus.Uploaded
   enums.SourceUploadStatus.Verified
   enums.SourceUploadStatus.Quarantined
   enums.SourceUploadStatus.Failed
   enums.SourceUploadStatus.Expired

5) Validate RTL languages (ar-SA, he-IL, ur-PK): the step 7 UI must work
   in RTL — verify dropzone and progress layout (no hardcoded directional
   margin/padding).

RULES
- Preserve placeholders exactly: {fileName}, {sizeMb}, {percent}.
- Do not introduce keys that are not present in en-US.
- Do not leave orphan keys (copy removed during implementation step 7).

VALIDATION
cd apps/portal && npm run lint && npm run build
Visual: switch language in the portal and open the upload form in pt-BR,
ar-SA, ja-JP, ko-KR, ur-PK.

FEATURE-LEVEL HANDOFF (final consolidated PR/release notes)
- Summary of what shipped (intent with presigned URL → PUT → complete →
  worker event adapter → worker command → verify async).
- Manual EF migration to apply before deploy:
    ALTER TABLE Sources ADD StorageKey nvarchar(1000) NULL;
    ALTER TABLE Sources ADD SizeBytes bigint NULL;
    ALTER TABLE Sources ADD UploadStatus int NOT NULL DEFAULT 1;
    CREATE UNIQUE INDEX IX_Sources_TenantId_StorageKey
      ON Sources(TenantId, StorageKey) WHERE StorageKey IS NOT NULL;
- ObjectStorage:* environment variables required in production.
- S3 bucket to provision with multi-tenant prefix-based policy.
- New worker Querify.QnA.Worker.Api to add to the deployment pipeline.
- Known operational extensions: provider-native malware scanning if the current
  implementation starts with ClamAV, text extraction, per-tenant quotas, and
  quarantine retention policy.
```

---

## Operational extensions

1. **Provider-native malware scanning** — the implementation must have a real scanner before
   production. If the economical first production path uses ClamAV, regulated or larger tenants can
   later use provider-native scanning such as cloud storage malware protection.
2. **Text extraction** — for AI-driven matching/generation (see
   [`../integrations/mcp-source-to-qna.md`](../integrations/mcp-source-to-qna.md)), the worker
   should populate `MetadataJson` with extracted text and structural metadata. Coupled to the AI
   roadmap.
3. **Per-tenant quotas** — bytes total, file count, max single file. Owned by
   `Tenant.BackOffice.Business.Billing`, enforced in `CreateUploadIntent` before a source is
   created.
4. **Quarantine retention policy** — retain unsafe objects only as long as the security process
   requires. Default to short retention because storage cost and privacy exposure grow with every
   retained artifact.
5. **Direct download streaming option** — for cases where presigned GET is undesirable (audit
   logging, watermarking), expose a `GET /api/qna/source/{id}/download` that streams through the
   API. Default remains presigned for cost and scalability.
