# Source Upload: File Ingestion for the QnA Module

## Purpose

This document is the complete design reference for adding **file upload** capability to the
`Source` entity in the QnA module. Today every `Source` points to an external locator (URL, ticket
id, repo path); the platform has no way for a tenant to upload a PDF, video, or document and link
it as a Source.

This document describes the target architecture, the design decisions behind it, the phased
implementation roadmap, and a self-contained agent prompt for each phase.

**Status:** designed, not yet built. See [`../README.md`](../README.md).

---

## Current state

The `Source` entity already supports the shape needed for a hosted file:

| Field | Today | Used for upload as |
|---|---|---|
| `Locator` (required, max 1000) | URL, ticket id, repo path | storage key (`{tenantId}/sources/{id}/{filename}`) |
| `MediaType` (optional, max 100) | declared content type | resolved content type from the storage HEAD |
| `Checksum` (required, max 128) | `sha256:<hex of locator>` placeholder | real `sha256:<hex of file bytes>` after worker verification |
| `MetadataJson` (optional, max 8000) | free-form JSON | reserved for extracted metadata |
| `LastVerifiedAtUtc` (optional) | manual verification timestamp | set by the worker after verification |
| `Visibility` | audience exposure | unchanged; still gated by `SourceRules.EnsureVisibilityAllowed` |
| `Kind` | artifact type (`Pdf`, `Video`, ...) | **unchanged** — the kind describes the artifact, not the origin |

What is missing:

1. No object storage in the local stack (`devops/local/docker/docker-compose.baseservices.yml` has
   PostgreSQL, RabbitMQ, Redis, SMTP4Dev, Prometheus, Grafana, Alertmanager, Jaeger — no MinIO/S3).
2. No shared `IObjectStorage` abstraction in `BaseFaq.Common.Infrastructure.*`.
3. No upload endpoints on the Portal API. The current `SourceController` is plain CRUD and assumes
   the caller already has a valid `Locator`.
4. No worker capable of reading a freshly uploaded blob, recomputing the checksum, and marking the
   `Source` as verified.

---

## Target architecture

### One-line summary

Tenant requests a presigned PUT, uploads the file directly to S3-compatible object storage,
finalizes through a portal endpoint that records `SizeBytes` and `MediaType`, and a QnA worker
asynchronously verifies the bytes and flips `UploadStatus` to `Verified`.

### Topology

```
Portal (React)              QnA Portal API                Object Storage           QnA Worker
                            (BaseFaq.QnA.Portal.Api)      (MinIO / S3 / R2)
─────────────────           ─────────────────────────     ────────────────         ───────────
1. POST /upload-intent ───► validate + create
                            Source(Pending)               
                            presign PUT URL ─────────────► (URL signed)
                            ◄───────── { url, key, ttl }
2. PUT (file bytes) ─────────────────────────────────────► stored
3. POST /upload-complete ─► HEAD object
                            persist SizeBytes/MediaType
                            UploadStatus = Uploaded
                            publish SourceUploadedEvent ──► RabbitMQ ─────────────► consume
                                                                                    stream SHA-256
                                                                                    verify HEAD
                                                                                    UploadStatus =
                                                                                    Verified
                                                                                    LastVerifiedAtUtc
4. GET /download-url ─────► presign GET URL
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

### Why a dedicated QnA worker, not the existing tenant worker

`BaseFaq.Tenant.Worker.Api` is the **control plane** worker — billing webhooks, email outbox,
entitlements. Per [`../../backend/architecture/basefaq-tenant-worker.md`](../../backend/architecture/basefaq-tenant-worker.md)
and [`../../backend/architecture/solution-architecture.md`](../../backend/architecture/solution-architecture.md)
section 7, it must not take ownership of product module workflows.

QnA-owned async work belongs to a new host: `BaseFaq.QnA.Worker.Api`, mirroring the project layout
of the tenant worker. If that creates too much overhead in early phases, a transitional option is
to run the consumer as an `IHostedService` inside `BaseFaq.QnA.Portal.Api` and split it out later;
the integration event contract stays the same.

---

## Multi-tenant key strategy

Storage key format:

```
{tenantId}/sources/{sourceId}/{sanitized-filename}
```

- `tenantId` prefix isolates blobs per tenant. A bucket policy can enforce `s3:prefix` per-tenant
  if the credentials are ever scoped down.
- `sourceId` is generated server-side before the presign call, so the storage key is stable across
  the two-phase flow even if the client retries.
- `sanitized-filename` only allows `[a-zA-Z0-9._-]`; everything else is replaced with `-`.
- `(TenantId, StorageKey)` carries a unique partial index in PostgreSQL where `StorageKey IS NOT
  NULL`, preventing duplicate storage references across the tenant.

Cross-tenant integrity is already enforced by `QnADbContext.OnBeforeSaveChangesRules()` because
`Source` is `IMustHaveTenant`. No new tenant-integrity extension is required by this feature
because `StorageKey` does not reference another tenant-owned record (per the
[`../../behavior-change-playbook.md`](../../behavior-change-playbook.md) Step 3.20 rule).

---

## Two-phase write flow

### Phase A — `POST /api/qna/source/upload-intent`

Body: `SourceUploadIntentRequestDto`

```
{ FileName, ContentType, SizeBytes, Kind, Language, Visibility, Label?, ContextNote? }
```

Handler responsibilities (this is a **query** handler in CQRS terms, not a command — it returns a
DTO synchronously and creates a placeholder row as a side effect; rationale below):

1. Resolve `tenantId` via `ISessionService.GetTenantId(ModuleEnum.QnA)`.
2. Validate `ContentType` against an allowlist (`application/pdf`, `image/png`, `image/jpeg`,
   `video/mp4`, `text/plain`, `text/markdown` initially).
3. Validate `SizeBytes <= MaxUploadBytes` (50 MB default; configurable).
4. Generate `sourceId = Guid.NewGuid()`.
5. Build `storageKey = SourceStorageKey.Build(tenantId, sourceId, fileName)`.
6. Create `Source` with:
   - `UploadStatus = Pending`
   - `Locator = storageKey`
   - `Checksum = SourceChecksum.FromLocator(storageKey)` (placeholder; the worker overwrites this)
   - `StorageKey = storageKey`
7. `presign = await objectStorage.PresignPutAsync(storageKey, contentType, maxBytes, ct)`
8. Return `SourceUploadIntentResponseDto { SourceId, UploadUrl, RequiredHeaders, StorageKey,
   ExpiresAtUtc }`.

CQRS rationale: this endpoint returns a DTO and therefore lives under `Queries/`, even though it
mutates state. This is a deliberate exception driven by the synchronous nature of presigning. The
alternative — a command returning `Guid` plus a separate query for the URL — duplicates round
trips and complicates retries. We keep one endpoint and document the exception in the
implementation prompt.

Failure modes:
- Invalid content type → `422 Unprocessable Entity` via `ApiErrorException`.
- Oversized request → `422`.
- Storage unreachable → `503 Service Unavailable` (`ApiErrorException`).

### Phase B — `POST /api/qna/source/{id:guid}/upload-complete`

Body: `SourceUploadCompleteRequestDto { SourceId, ClientChecksum? }`

Returns `Guid` (`200 OK`).

Handler is a real command (`IRequestHandler<TCommand, Guid>`):

1. Load the `Source` by `(TenantId, Id)`. `404` if missing.
2. If `UploadStatus != Pending`: `409 Conflict` (already finalized).
3. `head = await objectStorage.HeadAsync(entity.StorageKey, ct)`.
4. If `head == null`: `422` (the client never PUT the bytes).
5. Persist `SizeBytes = head.SizeBytes`, `MediaType = head.ContentType`,
   `UploadStatus = Uploaded`, `UpdatedBy = userId`.
6. `SaveChangesAsync`.
7. Publish `SourceUploadedIntegrationEvent` via `IPublishEndpoint`. **Outbox transactional
   pattern is deferred** — see "Known follow-ups" below.

### Phase C — `GET /api/qna/source/{id:guid}/download-url`

Returns `SourceDownloadUrlDto { Url, ExpiresAtUtc }` (`200 OK`).

Handler is a query:

1. Project `{ StorageKey, Visibility, UploadStatus, TenantId }` directly with `AsNoTracking() +
   Select(...)`.
2. `404` if missing or wrong tenant.
3. `422` if `StorageKey == null` (URL-only sources should expose `Locator` directly to the client,
   not via this endpoint).
4. `presign = await objectStorage.PresignGetAsync(storageKey, downloadTtl, ct)` (5 min default).
5. Return `{ Url, ExpiresAtUtc }`.

---

## Async verification (worker)

`SourceUploadedIntegrationEvent` shape:

```
{ Guid TenantId, Guid SourceId, string StorageKey, string? ClientChecksum,
  DateTime UploadedAtUtc }
```

Consumer (`SourceUploadedConsumer` in `BaseFaq.QnA.Worker.Business.Source`):

1. Resolve the tenant-scoped `QnADbContext` connection from the event's `TenantId` (uses
   `BaseFaq.Common.EntityFramework.Tenant` resolution).
2. Load the `Source`. If `UploadStatus != Uploaded`: ack and exit (idempotent).
3. Stream the blob via `IObjectStorage.OpenReadAsync(storageKey)`. Compute SHA-256 with
   `IncrementalHash.CreateHash(HashAlgorithmName.SHA256)` so the file is never fully materialized.
4. `head = HeadAsync(storageKey)` — re-read size/content-type for defense in depth.
5. If `ClientChecksum` was provided and does not match the streamed hash:
   - `UploadStatus = Failed`, persist, log a warning, **do not rethrow** (no retry — permanent
     failure).
6. Otherwise:
   - `Checksum = "sha256:<hex>"`
   - `SizeBytes = head.SizeBytes`
   - `LastVerifiedAtUtc = DateTime.UtcNow`
   - `UploadStatus = Verified`
   - `UpdatedBy = "system:qna-worker"`
   - persist.

Retry semantics:
- Transient storage errors (5xx, timeouts) propagate, MassTransit retries with the standard
  policy.
- Permanent errors (checksum mismatch, MIME mismatch) flip the row to `Failed` and ack.

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

`SourceUploadStatus` enum (BaseFaq numeric allocation `1, 6, 11, 16, 21`):

| Value | Meaning |
|---|---|
| `None = 1` | URL-based source; no upload involved. |
| `Pending = 6` | Intent issued; the client has not finalized PUT yet. |
| `Uploaded = 11` | Client confirmed via `upload-complete`; awaiting async verification. |
| `Verified = 16` | Worker validated checksum, size, and content type. |
| `Failed = 21` | Verification rejected the upload. |

The Portal mirror (`apps/portal/src/shared/constants/backend-enums.ts`) must be updated in the
same change per [`../../behavior-change-playbook.md`](../../behavior-change-playbook.md) Step 3.4.

---

## Phased roadmap

| Phase | Scope | Touches |
|---|---|---|
| 0 | Object storage infrastructure | `devops/local/docker/`, new `BaseFaq.Common.Infrastructure.Storage` |
| 1 | Model contract | `BaseFaq.QnA.Common.Domain`, `BaseFaq.Models.QnA`, `BaseFaq.QnA.Common.Persistence.QnADb` |
| 2 | Backend behavior (Portal) | `BaseFaq.QnA.Portal.Business.Source`, `BaseFaq.QnA.Portal.Api` |
| 3 | Async verification worker | new `BaseFaq.QnA.Worker.Api`, new `BaseFaq.QnA.Worker.Business.Source`, integration event |
| 4 | Seed and integration tests | `BaseFaq.Tools.Seed`, `BaseFaq.QnA.Portal.Test.IntegrationTests`, new `BaseFaq.QnA.Worker.Test.IntegrationTests` |
| 5 | Portal frontend | `apps/portal/src/domains/sources/` |
| 6 | Localization | `apps/portal/src/shared/lib/i18n/locales/*.json` (20 locales) |

Each phase below is a **self-contained agent prompt** suitable for execution by a smaller model.
Each prompt names the documents the executor must read before coding, the exact files to
create/edit, the BaseFAQ rules in scope, and the validation commands.

---

## Phase 0 prompt — Storage infrastructure

```text
You are working in the BaseFAQ monorepo (.NET 10 + multi-tenant + microservices).
Required reading before coding:
- docs/backend/architecture/solution-architecture.md (section 8: Cross-cutting
  concerns / shared libraries)
- docs/backend/architecture/repository-rules.md
- docs/backend/architecture/dotnet-backend-overview.md (section "Shared
  infrastructure and persistence")

GOAL
Add shared S3-compatible object storage infrastructure without touching any
business module. Nothing in this phase consumes the new library.

DELIVERABLES

1) Local compose — devops/local/docker/docker-compose.baseservices.yml
   Add a `minio` service following the pattern of postgres/rabbitmq:
   - image: minio/minio:latest
   - container_name: minio
   - ports 9000:9000 (S3 API) and 9001:9001 (console)
   - environment MINIO_ROOT_USER=minio, MINIO_ROOT_PASSWORD=Pass123$$
   - named volume `minio` mounted at /data
   - command: server /data --console-address ":9001"
   - networks: bf-network, extra_hosts host.docker.internal
   - healthcheck via curl on /minio/health/live
   Add `minio:` to the `volumes:` section of the same compose file.

   Also add a one-shot `minio-init` service that creates the bucket
   `basefaq-sources` (idempotent; uses minio/mc, depends_on minio with
   condition: service_healthy).

2) New shared project — dotnet/BaseFaq.Common.Infrastructure.Storage/
   Create csproj net10.0, namespace BaseFaq.Common.Infrastructure.Storage.
   Add to BaseFaq.sln.
   NuGet dependency: AWSSDK.S3 (the SDK speaks MinIO/R2/S3 with
   ForcePathStyle).

   Folder structure (follow other BaseFaq.Common.Infrastructure.* projects):
   - Abstractions/IObjectStorage.cs
   - Options/ObjectStorageOptions.cs    (Endpoint, Region, AccessKey,
     SecretKey, Bucket, ForcePathStyle, PresignTtlMinutes)
   - Services/S3ObjectStorage.cs
   - Extensions/ServiceCollectionExtensions.cs
                                        (AddObjectStorage(IConfiguration))

   IObjectStorage minimal signatures:
     Task<PresignedPutResult> PresignPutAsync(string key, string contentType,
         long maxBytes, CancellationToken ct);
     Task<Uri> PresignGetAsync(string key, TimeSpan ttl,
         CancellationToken ct);
     Task<ObjectMetadata?> HeadAsync(string key, CancellationToken ct);
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
   - services.AddSingleton<IObjectStorage, S3ObjectStorage>();

3) appsettings.Development.json (QnA Portal Api only for now):
   "ObjectStorage": {
     "Endpoint": "http://localhost:9000",
     "Region": "us-east-1",
     "AccessKey": "minio",
     "SecretKey": "Pass123$$",
     "Bucket": "basefaq-sources",
     "ForcePathStyle": true,
     "PresignTtlMinutes": 15
   }

NON-NEGOTIABLE RULES
- Do not touch BaseFaq.QnA.*, BaseFaq.Tenant.*, BaseFaq.Direct.*, or
  BaseFaq.Broadcast.* — this phase is infrastructure only.
- No comments explaining what code does; XML doc only on public surfaces
  (IObjectStorage, ObjectStorageOptions).
- Do not run EF migrations.
- Do not introduce parallel helpers (BlobService, FileService); IObjectStorage
  is the single entry point.

VALIDATION
- dotnet build dotnet/BaseFaq.Common.Infrastructure.Storage -v minimal
- docker compose -f devops/local/docker/docker-compose.baseservices.yml up -d
    minio minio-init
- curl http://localhost:9000/minio/health/live → 200
- console at http://localhost:9001 opens; bucket basefaq-sources exists

HANDOFF
List of projects that build, bucket creation confirmation, next phase: model
contract for Source upload.
```

---

## Phase 1 prompt — Model contract

```text
BaseFAQ monorepo. Required reading:
- docs/behavior-change-playbook.md (Steps 1–5)
- docs/backend/architecture/qna-domain-boundary.md
- docs/backend/architecture/repository-rules.md (Module model contract
  boundary)

CONTEXT
Source today supports only external sources via `Locator`. We are adding
support for tenant-uploaded files. Storage layer already exists (Phase 0:
BaseFaq.Common.Infrastructure.Storage with IObjectStorage). This phase is
model + DTOs + EF only; no handlers, controllers, or frontend.

DESIGN PRINCIPLES
- Keep `Locator` as the opaque key: external URL for URL sources, storage
  key for uploaded sources.
- Do not introduce a new SourceKind for "uploaded" — Kind describes the
  ARTIFACT (Pdf/Video/...), not the origin (playbook Step 2). Origin is
  derived from the presence of StorageKey.
- Source remains anemic (repository-rules §6).

DELIVERABLES

1) dotnet/BaseFaq.QnA.Common.Domain/Entities/Source.cs
   Add persisted properties (with mandatory XML doc explaining usage,
   playbook Step 3):
   - string? StorageKey { get; set; }   (max 1000; nullable for URL sources)
   - long? SizeBytes { get; set; }      (set after upload completes)
   - SourceUploadStatus UploadStatus { get; set; } = SourceUploadStatus.None;

   New constant: public const int MaxStorageKeyLength = 1000;
   Do not duplicate CreatedDate/UpdatedDate (already in BaseEntity).

2) dotnet/BaseFaq.Models.QnA/Enums/SourceUploadStatus.cs (NEW)
   Enum with BaseFaq numeric allocation (1,6,11,16,21 — playbook Step 3.4),
   each value with an XML summary explaining behavior (not naming):
   - None = 1            (URL/external source; no upload associated)
   - Pending = 6         (intent issued; PUT not yet confirmed)
   - Uploaded = 11       (client PUT succeeded; awaiting async processing)
   - Verified = 16       (worker validated checksum/size/MIME)
   - Failed = 21         (upload rejected; AV scan, invalid MIME, etc.)
   Also add the enum to apps/portal/src/shared/constants/backend-enums.ts in
   the same change (playbook Step 3.4).

3) dotnet/BaseFaq.Models.QnA/Dtos/Source/ — NEW DTOs (feature-folder layout,
   repository-rules §7):
   - SourceUploadIntentRequestDto.cs:
       string FileName, string ContentType, long SizeBytes, SourceKind Kind,
       string Language, VisibilityScope Visibility, string? Label,
       string? ContextNote
   - SourceUploadIntentResponseDto.cs:
       Guid SourceId, string UploadUrl,
       IReadOnlyDictionary<string,string> RequiredHeaders, string StorageKey,
       DateTime ExpiresAtUtc
   - SourceUploadCompleteRequestDto.cs:
       Guid SourceId, string? ClientChecksum (sha256:hex optional)
   - SourceDownloadUrlDto.cs:
       string Url, DateTime ExpiresAtUtc

   ALSO update:
   - SourceDto.cs: add StorageKey, SizeBytes, UploadStatus.
   - SourceCreateRequestDto.cs: do NOT add upload fields; URL-creation flow
     stays flat and separate (write DTOs are flat per repository-rules).

4) dotnet/BaseFaq.QnA.Common.Persistence.QnADb/Configurations/SourceConfiguration.cs
   Add property configuration for the 3 new fields:
   - StorageKey: HasMaxLength(Source.MaxStorageKeyLength), nullable
   - SizeBytes: nullable long
   - UploadStatus: HasDefaultValue(SourceUploadStatus.None).IsRequired()
   Add partial unique index: (TenantId, StorageKey) WHERE
   StorageKey IS NOT NULL (prevents duplicate StorageKey within the tenant).

5) dotnet/BaseFaq.QnA.Common.Domain/BusinessRules/Sources/SourceStorageKey.cs
   (NEW)
   Pure static class (no DbContext dependency, playbook Step 3.16):
     public static string Build(Guid tenantId, Guid sourceId, string fileName)
   Sanitize fileName (strip paths, keep [a-zA-Z0-9._-]). Format:
     "{tenantId}/sources/{sourceId}/{sanitized}"

6) dotnet/BaseFaq.QnA.Common.Domain/BusinessRules/Sources/SourceRules.cs
   Add:
     EnsurePublicVisibilityAllowedForUploadStatus(Source source,
         VisibilityScope visibility)
   Allow Visibility.Public only when UploadStatus == Verified or
   StorageKey == null (URL source with LastVerifiedAtUtc).

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
dotnet build dotnet/BaseFaq.Models.Common -v minimal
dotnet build dotnet/BaseFaq.Models.QnA -v minimal
dotnet build dotnet/BaseFaq.QnA.Common.Domain -v minimal
dotnet build dotnet/BaseFaq.QnA.Common.Persistence.QnADb -v minimal

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
- Next phase: handlers + controller (Phase 2).
```

---

## Phase 2 prompt — Backend behavior (Portal)

```text
BaseFAQ monorepo. Required reading:
- docs/backend/architecture/solution-cqrs-write-rules.md
- docs/backend/architecture/repository-rules.md (CQRS write contract, Folder
  ownership rules, API write response conventions, API error conventions)
- docs/behavior-change-playbook.md (Step 6)

CONTEXT
- Phase 0 delivered BaseFaq.Common.Infrastructure.Storage with IObjectStorage.
- Phase 1 added StorageKey/SizeBytes/UploadStatus on Source plus the new DTOs.
- This phase adds the two-phase flow (intent → direct PUT → complete) to the
  Portal API (BaseFaq.QnA.Portal.Business.Source) only. Public API does NOT
  receive uploads (multi-tenant principle: public is read-only).
- Async event publication (RabbitMQ) is included in this phase; the consumer
  arrives in Phase 3.

DELIVERABLES

In dotnet/BaseFaq.QnA.Portal.Business.Source/, mirror the existing command
folder layout:

1) Queries/RequestUploadIntent/
   - SourcesRequestUploadIntentQuery.cs
       : IRequest<SourceUploadIntentResponseDto>
   - SourcesRequestUploadIntentQueryHandler.cs

   Note on CQRS: this endpoint mutates state (creates a Pending Source) but
   returns a synchronous DTO (the presigned URL). Documented exception to
   the "commands return Guid" rule. Place in Queries/ to keep the contract.

   Handler:
   - tenantId via ISessionService.GetTenantId(ModuleEnum.QnA)
   - validate ContentType allowlist (application/pdf, image/png, image/jpeg,
     video/mp4, text/plain, text/markdown). 422 on mismatch.
   - validate SizeBytes <= MaxUploadBytes (50 MB constant in SourceRules).
   - sourceId = Guid.NewGuid()
   - storageKey = SourceStorageKey.Build(tenantId, sourceId, fileName)
   - new Source { Id = sourceId, UploadStatus = Pending, StorageKey,
       Locator = storageKey,
       Checksum = SourceChecksum.FromLocator(storageKey),
       Kind, Language, Visibility, Label, ContextNote, MediaType =
       request.ContentType, TenantId, CreatedBy, UpdatedBy }
   - dbContext.Sources.Add; SaveChangesAsync
   - presign = await objectStorage.PresignPutAsync(storageKey, contentType,
       maxBytes, ct)
   - return SourceUploadIntentResponseDto.

2) Commands/CompleteUpload/
   - SourcesCompleteUploadCommand.cs   : IRequest<Guid>
       { Guid SourceId, string? ClientChecksum }
   - SourcesCompleteUploadCommandHandler.cs

   Handler (orchestration phases per repository-rules §3):
   - Load Source by (TenantId, Id); 404 if missing.
   - 409 Conflict if UploadStatus != Pending.
   - head = await objectStorage.HeadAsync(entity.StorageKey, ct).
   - 422 if head == null ("Upload not received").
   - entity.SizeBytes = head.SizeBytes
   - entity.MediaType = head.ContentType
   - entity.UploadStatus = SourceUploadStatus.Uploaded
   - entity.UpdatedBy = userId
   - SaveChangesAsync
   - publishEndpoint.Publish(new SourceUploadedIntegrationEvent {
       TenantId, SourceId, StorageKey, ClientChecksum,
       UploadedAtUtc = DateTime.UtcNow })
   - return entity.Id

3) Queries/GetDownloadUrl/
   - SourcesGetDownloadUrlQuery.cs
       : IRequest<SourceDownloadUrlDto> { Guid Id }
   - SourcesGetDownloadUrlQueryHandler.cs
   - AsNoTracking, Select { StorageKey, Visibility, UploadStatus, TenantId }.
   - Filter by TenantId; 404 if missing.
   - 422 if StorageKey == null (URL source: client should use Locator
     directly).
   - presign GET with configurable TTL (5 min default).
   - return { Url, ExpiresAtUtc }.

4) Service/SourceService.cs + Abstractions/ISourceService.cs
   Add thin delegating methods:
     Task<SourceUploadIntentResponseDto> RequestUploadIntent(
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

6) Define the integration event contract — preferred location is
   BaseFaq.Models.QnA.Contracts (new Dtos/IntegrationEvents/ folder if
   absent) OR a new BaseFaq.Models.QnA.Contracts project if the repository
   pattern requires it. The contract:
     SourceUploadedIntegrationEvent
       { Guid TenantId, Guid SourceId, string StorageKey,
         string? ClientChecksum, DateTime UploadedAtUtc }

7) BaseFaq.QnA.Portal.Api/Extensions/ServiceCollectionExtensions.cs:
   Ensure AddObjectStorage(configuration) is called once in the host
   composition root (solution-architecture §1).
   Add project reference: BaseFaq.Common.Infrastructure.Storage in
   BaseFaq.QnA.Portal.Business.Source.csproj.
   Ensure MassTransit is wired in the host with the Publish endpoint.

RULES
- Controller is HTTP-only.
- Service is thin and delegates via MediatR.
- Command returns Guid (CompleteUpload). Queries return DTOs.
- Use ApiErrorException with the appropriate HttpStatusCode.
- AsNoTracking + Select projection in queries.
- Do not touch BaseFaq.QnA.Public.* — upload is portal-only.

VALIDATION
dotnet build dotnet/BaseFaq.QnA.Portal.Business.Source
dotnet build dotnet/BaseFaq.QnA.Portal.Api

HANDOFF
- New endpoints: upload-intent, upload-complete, download-url.
- UploadStatus reaches Uploaded; Verified depends on Phase 3.
- Tests pending (Phase 4).
```

---

## Phase 3 prompt — Worker async verification

```text
BaseFAQ monorepo. Required reading:
- docs/backend/architecture/basefaq-tenant-worker.md
- docs/backend/architecture/solution-architecture.md (§7 control-plane
  workers)
- BaseFaq.Common.Infrastructure.MassTransit/Extensions and Consumers for
  patterns

CONTEXT
After upload-complete (Phase 2), Source is in UploadStatus.Uploaded. We need
async processing to:
1. Recompute SHA-256 from real bytes (not from the locator string).
2. Confirm SizeBytes and MediaType (defense-in-depth against HEAD lying).
3. Set LastVerifiedAtUtc + UploadStatus = Verified.
4. Provide a hook for future AV scan (placeholder for now).

ARCHITECTURAL DECISION
Source belongs to the QnA module, but the playbook
(basefaq-tenant-worker.md) requires that Tenant.Worker.Api host only
control-plane work, not product workflows. Create a new project:
BaseFaq.QnA.Worker.Api, mirroring the tenant worker layout.

If overhead is a concern, a transitional option is to run the consumer as
an IHostedService inside BaseFaq.QnA.Portal.Api. Document the choice in the
handoff but keep the integration event contract identical so a later split
is mechanical.

DELIVERABLES (assuming a dedicated worker)

1) Verify the integration event contract from Phase 2:
   SourceUploadedIntegrationEvent
     { Guid TenantId, Guid SourceId, string StorageKey,
       string? ClientChecksum, DateTime UploadedAtUtc }

2) New project: dotnet/BaseFaq.QnA.Worker.Api/
   - Program.cs: generic IHost, AddMassTransit with consumer registration,
     queue qna.source.uploaded
   - AddObjectStorage, AddDbContext<QnADbContext>
   - tenant connection resolution mirroring BaseFaq.Tenant.Worker.Api
   - appsettings with ObjectStorage + ConnectionStrings + RabbitMQ
   - Dockerfile (mirror BaseFaq.Tenant.Worker.Api/Dockerfile)

3) New project: dotnet/BaseFaq.QnA.Worker.Business.Source/
   - Consumers/SourceUploadedConsumer.cs
       : IConsumer<SourceUploadedIntegrationEvent>

   Consumer flow:
   - resolve QnADbContext for the event's TenantId (use existing
     BaseFaq.Common.EntityFramework.Tenant resolution)
   - load Source; idempotent — return if UploadStatus != Uploaded
   - using IObjectStorage.OpenReadAsync(storageKey), compute SHA-256 in
     stream (do NOT materialize the full blob; use IncrementalHash)
   - re-confirm SizeBytes via HeadAsync
   - if ClientChecksum was provided and does not match: UploadStatus =
     Failed, log warning, persist, return without rethrowing (no retry).
   - otherwise: entity.Checksum = "sha256:..."; entity.SizeBytes =
     head.SizeBytes; entity.LastVerifiedAtUtc = DateTime.UtcNow;
     entity.UploadStatus = SourceUploadStatus.Verified;
     entity.UpdatedBy = "system:qna-worker"
   - SaveChangesAsync

4) Service registration extension in
   BaseFaq.QnA.Worker.Business.Source/Extensions/ServiceCollectionExtensions.cs:
   AddSourceWorker(IConfiguration) registers consumer + dependencies.

5) Add both csprojs to BaseFaq.sln.

6) devops/local/docker/docker-compose.backend.yml — add `bf-qna-worker`
   service following the pattern of other backend services (build context,
   depends_on postgres/rabbitmq/minio, env vars).

RULES
- Worker does not perform read-after-write on entities of other modules.
- Consumer is idempotent (filter by UploadStatus == Uploaded).
- Do not materialize the full blob — streaming SHA-256 only.
- Transient errors (storage 5xx) leave the message for MassTransit retry
  (rethrow). Permanent errors (checksum mismatch, MIME mismatch) flip to
  Failed and ack.
- QnADbContext is tenant-scoped — connection string must be resolved from
  the event TenantId before save.

VALIDATION
dotnet build dotnet/BaseFaq.QnA.Worker.Api
dotnet build dotnet/BaseFaq.QnA.Worker.Business.Source
docker compose up -d bf-qna-worker rabbitmq minio postgres
- Run /api/qna/source/upload-intent → PUT to presigned URL → upload-complete
- Inspect consumer logs and Sources.UploadStatus column.

HANDOFF (known follow-ups, not blockers)
- Transactional outbox so the integration event publishes only when the
  upload-complete transaction commits. Today an event can be lost if the
  process dies after SaveChangesAsync but before Publish.
- AV scan (clamav or equivalent) before Verified.
- Text extraction for AI indexing (depends on AI roadmap).
- Lifecycle policy on the bucket: drop Pending objects older than 24h.
```

---

## Phase 4 prompt — Seed and integration tests

```text
BaseFAQ monorepo. Required reading:
- docs/behavior-change-playbook.md (Steps 7–8)
- docs/backend/testing/integration-testing-strategy.md
- docs/backend/tools/seed-tool.md

CONTEXT
Phases 1–3 delivered the full upload flow. This phase adds seed examples
and integration coverage.

DELIVERABLES

1) dotnet/BaseFaq.Tools.Seed/Application/QnASeedCatalog.*.cs
   Locate the existing Source catalog file. Add 2 deterministic Source
   examples with simulated upload state:
   - "Manual de produto.pdf" — Kind=Pdf, StorageKey populated, SizeBytes,
     UploadStatus=Verified, deterministic SHA-256 Checksum
   - "Política de privacidade.pdf" — second example
   The seed must NOT actually upload to MinIO; it only populates the entity
   with synthetic StorageKey and Checksum. (Seed is data-only — it does not
   touch external systems.)

2) dotnet/BaseFaq.QnA.Portal.Test.IntegrationTests/Tests/Source/
   SourceUploadCommandQueryTests.cs (real PostgreSQL, mirroring the
   project's existing test patterns):
   - RequestUploadIntent_ValidPdf_CreatesPendingSourceAndReturnsPresignedUrl
   - RequestUploadIntent_InvalidContentType_Returns422
   - RequestUploadIntent_OversizeBytes_Returns422
   - CompleteUpload_NoBlobInStorage_Returns422
   - CompleteUpload_StatusNotPending_Returns409
   - CompleteUpload_HappyPath_TransitionsToUploaded
   - GetDownloadUrl_UrlSource_Returns422
   - GetDownloadUrl_UploadedSource_ReturnsPresignedGet
   - CrossTenant_SourceVisibility_NotLeaked  (cross-tenant regression)

   IObjectStorage is replaced by an in-memory fake ONLY because storage is
   not the boundary under test. Document this as an explicit exception. For
   real end-to-end MinIO tests, gate with [Trait("Category",
   "RequiresMinio")].

3) dotnet/BaseFaq.QnA.Portal.Test.IntegrationTests/Common/Factories/
   SourceFactory.cs (update or create):
   CreateUploadedSource(Guid tenantId, ...) producing entity with
   StorageKey + UploadStatus=Verified + valid Checksum.

4) dotnet/BaseFaq.QnA.Worker.Test.IntegrationTests/ (NEW project mirroring
   BaseFaq.Tenant.Worker.Test.IntegrationTests):
   Tests for SourceUploadedConsumer:
   - HappyPath_VerifiesAndTransitionsToVerified
   - ChecksumMismatch_TransitionsToFailed
   - StatusNotUploaded_IsIdempotent

RULES
- Use real DB and real EF migrations (testing-strategy).
- Do not mock SessionService for tenant context — use the existing context
  builder.
- Each test independent (use the established WebApplicationFactory pattern).

VALIDATION
dotnet test dotnet/BaseFaq.QnA.Portal.Test.IntegrationTests
dotnet test dotnet/BaseFaq.QnA.Worker.Test.IntegrationTests
dotnet run --project dotnet/BaseFaq.Tools.Seed
- After seed, list /api/qna/source on the Portal API and verify both
  examples carry StorageKey + UploadStatus=Verified.

HANDOFF
- Coverage: Portal command/query covered. Worker consumer covered.
- Pending: i18n, frontend.
```

---

## Phase 5 prompt — Portal frontend (sources domain)

```text
BaseFAQ monorepo. Required reading:
- docs/frontend/architecture/portal-app.md
- docs/frontend/architecture/portal-app-ui-prompt-guidance.md
- docs/behavior-change-playbook.md (Step 9)

CONTEXT
Backend (Phases 1–3) exposes:
  POST /api/qna/source/upload-intent           → SourceUploadIntentResponseDto
  POST /api/qna/source/{id}/upload-complete    → Guid
  GET  /api/qna/source/{id}/download-url       → SourceDownloadUrlDto

Source has new fields: StorageKey, SizeBytes, UploadStatus.

Existing frontend domain: apps/portal/src/domains/sources/

DELIVERABLES

1) apps/portal/src/domains/sources/types.ts
   - Add types: SourceUploadIntentRequestDto,
     SourceUploadIntentResponseDto, SourceUploadCompleteRequestDto,
     SourceDownloadUrlDto, SourceUploadStatus (mirror BaseFaq enum).
   - Update SourceDto with StorageKey, SizeBytes, UploadStatus.

2) apps/portal/src/domains/sources/api.ts
   Three new functions following the existing portalRequest pattern:
   - requestSourceUploadIntent(accessToken, tenantId, body)
   - completeSourceUpload(accessToken, tenantId, id, body)
   - getSourceDownloadUrl(accessToken, tenantId, id)

3) apps/portal/src/domains/sources/hooks.ts
   - useRequestSourceUploadIntent (mutation)
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

5) apps/portal/src/domains/sources/source-form-page.tsx
   Add an "upload" mode alongside the existing "external URL" mode in the
   create form:
   - segmented control: "External URL" | "File upload"
   - upload mode: <input type="file"> + name/size/MIME preview
   - on submit: requestSourceUploadIntent → uploadSourceFile (with progress
     state) → completeSourceUpload → redirect to detail.
   - Follow portal-app-ui-prompt-guidance:
     - shared/ui components (Button, Card, Input, Select)
     - states: loading/empty/error/pending/success/destructive
     - mobile: card stack below xl
     - light + dark themes

6) apps/portal/src/domains/sources/source-detail-page.tsx
   - Show UploadStatus as a badge (use shared/constants/enum-ui.ts).
   - If StorageKey != null: a "Download" button that calls
     useSourceDownloadUrl and navigates to the returned URL.
   - Show SizeBytes formatted (KB/MB) when present.

7) apps/portal/src/domains/sources/source-list-page.tsx
   - New "Origin" column: "URL" | "File" based on StorageKey != null.
   - New filter on UploadStatus (Select backed by the enum).

8) apps/portal/src/shared/constants/backend-enums.ts
   Add SourceUploadStatus mapped to the same numeric values as the .NET
   enum (1, 6, 11, 16, 21).

9) apps/portal/src/shared/constants/enum-ui.ts
   Add presentation metadata for SourceUploadStatus:
   None=ghost, Pending=warning, Uploaded=info, Verified=success,
   Failed=destructive.

RULES
- TypeScript types mirror .NET DTOs exactly.
- Do not concatenate translated copy in components (i18n in next phase).
- Every editable field must have `description`, `hint`, or visible label
  with ContextHint.
- No hardcoded English copy — use t() with placeholder keys and add the key
  to en-US.json. Phase 6 propagates to other locales.
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

## Phase 6 prompt — Localization (20 locales)

```text
BaseFAQ monorepo. Required reading:
- docs/frontend/architecture/portal-localization.md
- docs/behavior-change-playbook.md (Step 10)

CONTEXT
Phase 5 introduced new copy keys for the sources domain, all with
en-US placeholder values. This phase propagates them to all 20 locales:
en-US, ar-SA, bn-BD, de-DE, es-ES, fr-FR, he-IL, hi-IN, id-ID, it-IT,
ja-JP, ko-KR, pl-PL, pt-BR, ru-RU, th-TH, tr-TR, ur-PK, vi-VN, zh-CN.

DELIVERABLES

1) Audit apps/portal/src/shared/lib/i18n/locales/en-US.json and list every
   new key added in Phase 5 (look for sources.upload.*, sources.download.*,
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
   enums.SourceUploadStatus.Failed

5) Validate RTL languages (ar-SA, he-IL, ur-PK): the Phase 5 UI must work
   in RTL — verify dropzone and progress layout (no hardcoded directional
   margin/padding).

RULES
- Preserve placeholders exactly: {fileName}, {sizeMb}, {percent}.
- Do not introduce keys that are not present in en-US.
- Do not leave orphan keys (copy removed during Phase 5).

VALIDATION
cd apps/portal && npm run lint && npm run build
Visual: switch language in the portal and open the upload form in pt-BR,
ar-SA, ja-JP, ko-KR, ur-PK.

FEATURE-LEVEL HANDOFF (final consolidated PR/release notes)
- Summary of what shipped (intent → PUT → complete → verify async).
- Manual EF migration to apply before deploy:
    ALTER TABLE Sources ADD StorageKey nvarchar(1000) NULL;
    ALTER TABLE Sources ADD SizeBytes bigint NULL;
    ALTER TABLE Sources ADD UploadStatus int NOT NULL DEFAULT 1;
    CREATE UNIQUE INDEX IX_Sources_TenantId_StorageKey
      ON Sources(TenantId, StorageKey) WHERE StorageKey IS NOT NULL;
- ObjectStorage:* environment variables required in production.
- S3 bucket to provision with multi-tenant prefix-based policy.
- New worker BaseFaq.QnA.Worker.Api to add to the deployment pipeline.
- Known follow-ups: transactional outbox, AV scan, text extraction,
  per-tenant quotas, bucket lifecycle policy for orphan Pending objects
  older than 24h.
```

---

## Known follow-ups (not in scope for the initial roadmap)

1. **Transactional outbox** — the Phase 2 handler publishes the integration event after
   `SaveChangesAsync`. A crash between the two leaves the row in `Uploaded` without ever firing
   the event. The fix is to write the event into an outbox table inside the same transaction and
   publish from a separate dispatcher. MassTransit has built-in outbox support.
2. **AV scan** — Phase 3 verifies checksum and size but does not scan for malware. Adding a
   clamav/Cloudmersive step in front of `Verified` is a self-contained follow-up that does not
   change the public contract.
3. **Text extraction** — for AI-driven matching/generation (see
   [`../integrations/mcp-source-to-qna.md`](../integrations/mcp-source-to-qna.md)), the worker
   should populate `MetadataJson` with extracted text and structural metadata. Coupled to the AI
   roadmap.
4. **Per-tenant quotas** — bytes total, file count, max single file. Owned by
   `Tenant.BackOffice.Business.Billing`, enforced in `RequestUploadIntent` before the presign.
5. **Bucket lifecycle policy** — auto-expire Pending blobs older than 24h. Pure infrastructure;
   add to the bucket Terraform/IaC when production storage is provisioned.
6. **Direct download streaming option** — for cases where presigned GET is undesirable (audit
   logging, watermarking), expose a `GET /api/qna/source/{id}/download` that streams through the
   API. Default remains presigned for cost and scalability.
