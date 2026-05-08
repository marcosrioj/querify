# Behavior Change Playbook

## Purpose

Use this playbook when a change adds, updates, deletes, or consolidates Querify module behavior across the Querify solution.

This is the workflow for changes that start in a module model and affect persistence, contracts, commands, queries, services, APIs, seed data, tests, Portal screens, and translations.

The goal is not to add layers. The goal is to keep the model simple, preserve supported behavior, and remove duplicated concepts before they spread through the rest of the system.

The current Querify module split is documented in [`business/value_proposition.md`](business/value_proposition.md). The current modules are Tenant, QnA, Direct, Broadcast, and Trust. Tenant owns the control plane. QnA, Direct, Broadcast, and Trust own product behavior. Treat each module persistence boundary as the owner of its own behavior; do not park behavior in QnA because the QnA model already has a channel, source, or activity enum value that sounds close.

## Before Starting

Read existing documentation before making code changes. Follow the **Product behavior change** reading order in [`execution-guide.md`](execution-guide.md) → Standard workstreams.

If those documents do not describe the behavior you are changing, inspect the closest existing implementation and follow its current pattern. If the new behavior establishes a reusable pattern, update the most specific owning document and link it from [`docs/README.md`](README.md).

## Non-Negotiable Rules

- Do not run or generate EF migrations during this workflow unless the user explicitly asks for migration work. Schema changes must be left as a clear manual migration follow-up.
- Do not remove `required` modifiers or add default values to hide object construction errors unless the user explicitly asks for that compatibility tradeoff.
- Do not add a new enum, property, DTO field, or UI concept if an existing one already represents the same business dimension.
- Do not keep deprecated behavior alive by retaining duplicate fields. Preserve behavior by mapping it to the canonical concept.
- Product persistence entities stay anemic. They contain state only, not behavior methods, factory methods, transition methods, or computed projection helpers.
- Tenant and module `DbContext` implementations are separate ownership boundaries.
- Do not add Direct conversation, handoff, ticket-resolution, or agent-assist workflow state to QnA entities.
- Do not add Broadcast social, public-comment, mention, community-thread, or campaign interaction workflow state to QnA entities.
- Do not add Trust validation, governance, decision-history, or auditability state to QnA, Direct, Broadcast, or Tenant entities.
- `Querify.Direct.Common.Persistence.DirectDb` and `Querify.Broadcast.Common.Persistence.BroadcastDb` contain their current module entity models. Write or update those entities only for concrete module behavior; do not add placeholder entities or empty folders only to satisfy a split.
- Repository artifacts must be written in English even when the prompt, chat, or review request is in another language. This includes code identifiers, comments, tests, seed data labels, documentation, Markdown, `en-US` UI copy keys, and handoff notes. The only expected exceptions are non-`en-US` locale values and external quoted/source material that must stay in its original language.
- Command handlers return simple values only. Complex DTOs belong to queries.
- Portal UI copy is frontend-owned. Backend DTOs should not return translated labels.
- Portal translation updates must be complete in the same frontend change. Do not add English fallback values to non-`en-US` locale files, and do not leave stale keys for UI copy that was removed or renamed.
- Portal guidance must treat `Active` lifecycle status as availability for use, reuse, public exposure when visible, and automation. Do not interpret Active Space, Question, or Answer records as unresolved demand or required next actions.

## Step 0: Decide Whether The Change Must Be Staged

Split the work into stages when doing everything in one pass would reduce quality, create broad unclear diffs, or make validation unreliable.

Prefer this staging model for large behavior changes:

1. Model contract: entities, enums, DTOs, EF configuration, read mappings.
2. Backend behavior: commands, queries, handlers, services, controllers, feature registrations.
3. Seed and tests: factories, seed catalogs, scenario coverage, obsolete-test removal.
4. Frontend and i18n: API contracts, domain screens, shared enum labels, all locale files.
5. Full validation and migration handoff: builds, tests, frontend checks, and manual migration notes.

Each stage should leave a clear status:

- what builds now
- what is intentionally still broken
- which follow-up stage owns the breakage
- whether a manual migration is required

## Step 1: Inventory Existing Behavior

Before adding anything, search for the current behavior and adjacent concepts:

```bash
rg -n "ConceptName|RelatedEnum|RelatedProperty" dotnet apps docs
rg --files dotnet/Querify.Models.Tenant dotnet/Querify.Models.QnA dotnet/Querify.Models.Direct dotnet/Querify.Models.Broadcast dotnet/Querify.Common.EntityFramework.Tenant dotnet/Querify.QnA.Common.Domain dotnet/Querify.QnA.Common.Persistence.QnADb dotnet/Querify.Direct.Common.Persistence.DirectDb dotnet/Querify.Broadcast.Common.Persistence.BroadcastDb apps/portal/src/domains
```

Capture these facts before editing:

- which entity owns the persisted state
- which Querify module owns the behavior: Tenant, QnA, Direct, Broadcast, or Trust
- which enum expresses lifecycle, channel, audience, mode, role, actor, or event history
- which DTOs expose the state
- which handlers mutate or query it
- which seed files create examples
- which tests assert the affected behavior
- which Portal screens and translations expose it
- which feature helpers, business extensions, service registrations, route actions, and workflow classes exist only to support the behavior being removed or consolidated
- which documentation already describes the pattern
- whether the owning module already has the owning entity; if it does not, record a staged follow-up instead of storing that behavior in another module

If two names describe the same business dimension, consolidate them before propagating the model.

When the change removes or consolidates behavior, treat the inventory as a deletion search, not just a reference search. Check for obsolete command/query folders, handlers, service methods, controller actions, helper classes, extension methods, validators, factories, seed builders, UI hooks, API clients, presentation metadata, and tests that no longer have a live caller after the canonical behavior is changed.

## Step 2: Normalize The Business Concept

Classify each field or enum into one business dimension. There should be one canonical representation per dimension.

Use these dimensions for module behavior:

| Dimension | Canonical location | Meaning |
|---|---|---|
| Module boundary | owning persistence project | Which Querify module owns the behavior: Tenant, QnA, Direct, Broadcast, or Trust. |
| Space lifecycle state | `SpaceStatus` | Whether a QnA space is draft, active, or archived. Public spaces must be active. |
| Lifecycle state | `QuestionStatus`, `AnswerStatus` | Where a question or answer is in workflow. Active means available for use/reuse, not unresolved demand. |
| Audience exposure | `VisibilityScope` | Who can see the item: internal portal users, authenticated external users, or the public. This is not status and not moderation. |
| Channel | `ChannelKind` | Where a question, vote, feedback, or signal entered the system. |
| Answer provenance | `AnswerKind` | Whether an answer is official, community-provided, or imported. |
| Source material type | `SourceKind` | What artifact is linked: article, page, ticket, transcript, audit record, and so on. |
| Source relationship role | `SourceRole` | Why a source is attached: origin, context, evidence, or reference. |
| Actor type | `ActorKind` | Who or what caused an activity event. |
| Event journal | `ActivityKind` | Question status events, answer status events, feedback signals, and vote signals. This is not current state. |
| Search rendering | `SearchMarkupMode` | How search-facing markup behaves for a space. |

Common consolidation rules:

- A space status enum should not be duplicated by kind/mode enums plus publication timestamps.
- A question type enum should not duplicate origin channel, lifecycle status, or the parent space mode.
- Duplicate question detection is not persisted as QnA question state. Intake processes such as Direct, Broadcast, imports, or QnA-owned creation flows may use vector search to resolve an existing question before creating a new canonical question.
- A source kind should describe the artifact. A source role should describe why it is linked. Visibility should describe whether the source is internal-only, authenticated external, or public.
- Source trust or validation behavior belongs to relationship context or Trust-owned validation, not a source-wide shortcut in QnA.
- An activity kind should describe a supported status event or public signal, not a field that can be edited directly.
- Generic edits, accepted-answer selection, reports, and creation shortcuts should rely on entity state, relationship state, or audit fields instead of broad activity values.
- A visibility value should not imply moderation, approval, citation permission, or indexing beyond audience exposure.
- Capability booleans are acceptable only when they represent an independent switch, such as whether a space accepts questions or answers.
- Channel, source, or activity values may record where a module asset came from, but they must not become the persistence home for another module's workflow.
- If a module behavior has no owning entity yet, stage the entity model before adding module-specific fields to another module as a shortcut.

## Step 3: Update Entities And Enums

Relevant locations:

- Common module enum: `dotnet/Querify.Models.Common/Enums/ModuleEnum.cs`
- Tenant contracts and entities when the behavior belongs to tenant control plane: `dotnet/Querify.Models.Tenant`, `dotnet/Querify.Common.EntityFramework.Tenant`
- QnA contracts: `dotnet/Querify.Models.QnA/Enums`
- QnA domain entities and entity-related business rules: `dotnet/Querify.QnA.Common.Domain/Entities` and `dotnet/Querify.QnA.Common.Domain/BusinessRules`
- QnA persistence infrastructure: `dotnet/Querify.QnA.Common.Persistence.QnADb`
- Direct contracts and persistence entities: `dotnet/Querify.Models.Direct`, `dotnet/Querify.Direct.Common.Persistence.DirectDb/Entities`
- Broadcast contracts and persistence entities: `dotnet/Querify.Models.Broadcast`, `dotnet/Querify.Broadcast.Common.Persistence.BroadcastDb/Entities`
- Trust contracts and persistence entities use `Querify.Models.Trust` and a Trust persistence boundary when those projects are in scope for the change

Process:

1. Add, update, or delete enum values only after Step 2 confirms there is no duplicate concept.
2. Add an XML summary to every enum option in the enum file. The summary must explain the behavior or decision represented by the option, not just restate the option name.
3. Every enum member must declare an explicit numeric value. Do not use `0`, and do not rely on implicit enum numbering; `default(<Enum>)` must remain an invalid persisted/API value.
4. Use the Querify enum allocation sequence in declaration order: `1, 6, 11, 16, 21`, continuing by `+5` for every additional member. When a matching Portal enum exists in `apps/portal/src/shared/constants/backend-enums.ts`, update it in the same change.
5. If an existing persisted/API enum needs a value inserted between stable values, use an unused number inside the existing five-value gap only when the current numeric contract must be preserved. Renumber the whole enum only when the user explicitly accepts a data reset, data migration, or contract break.
6. Update the owning entity with the smallest persisted shape that can execute the behavior.
7. Add an XML summary to every persisted property and navigation in the entity file. The summary must explain how the behavior uses the property, including when a timestamp or actor differs from `BaseEntity`/`AuditableEntity` state.
8. Before adding a persisted property, check whether `BaseEntity` or `AuditableEntity` already provides the needed state: `Id`, `CreatedDate`, `CreatedBy`, `UpdatedDate`, `UpdatedBy`, `DeletedDate`, `DeletedBy`, or `IsDeleted`.
9. Do not duplicate or shadow base entity state with module-specific fields such as `CreatedAtUtc`, `UpdatedAtUtc`, `DeletedAtUtc`, `ExternalCreatedBy`, or separate soft-delete flags unless the new field represents a distinct domain timestamp or actor.
10. Remove properties that duplicate the new canonical field.
11. Preserve existing behavior semantics by moving callers to the canonical field.
12. Keep entities state-only.
13. Keep `required` semantics explicit. Do not set silent defaults to make construction easier.
14. For `Source`, keep artifact identity, audience exposure, and relationship context separate: `Kind` and locator fields identify the material, `Visibility` controls who can see it, and `SourceRole` explains why it is attached.
15. Do not create placeholder module entities. If a needed owning entity is still missing and the stage does not explicitly introduce it, leave a handoff note instead.
16. For QnA, put reusable entity-related rules under `Querify.QnA.Common.Domain.BusinessRules`; these rules must not depend on `QnADbContext`, EF queries, service registration, or HTTP controllers.
17. When a new or changed entity implements `IMustHaveTenant` and references another tenant-owned entity, update the owning `DbContext` to enforce tenant integrity before save.
18. Follow the module `DbContext` pattern: call `EnsureTenantIntegrity()` from `OnBeforeSaveChangesRules()`, place one focused rule per checked entity or relationship under `DbContext/TenantIntegrity/<Entity>TenantIntegrityExtension.cs`, and keep `Extensions` folders for service registration only.
19. Resolve referenced tenant ids with `TenantIntegrityLookupCacheBase` or a module-specific `TenantIntegrityLookupCache`, using `IgnoreQueryFilters()` so tenant filters and soft-delete do not hide invalid relationships.
20. Throw on cross-tenant links or missing references. If a tenant-owned entity has no tenant-owned relationships, record that no additional tenant-integrity rule is needed instead of adding empty validation code.

When deleting a property or enum, immediately search for all compile-time and string references:

```bash
rg -n "DeletedProperty|DeletedEnum|DeletedValue" dotnet apps docs
```

Historical EF migration files may still mention old schema. Do not edit or regenerate migrations in this workflow unless migration work was explicitly requested.

## Step 4: Update EF Configurations And Query Projections

Relevant locations:

- QnA configurations: `dotnet/Querify.QnA.Common.Persistence.QnADb/Configurations`
- Module DbContext folder: `dotnet/Querify.<Module>.Common.Persistence.<Module>Db/DbContext`
- Module tenant-integrity rules: `dotnet/Querify.<Module>.Common.Persistence.<Module>Db/DbContext/TenantIntegrity`
- Query handlers under the owning feature project: `dotnet/Querify.<Module>.<Surface>.Business.<Feature>/Queries`
- Direct configurations and DbContext under `dotnet/Querify.Direct.Common.Persistence.DirectDb`
- Broadcast configurations and DbContext under `dotnet/Querify.Broadcast.Common.Persistence.BroadcastDb`
- Tenant persistence equivalents when the behavior is control-plane-owned.

Process:

1. Add property configuration for new persisted fields.
2. Remove configuration for deleted fields.
3. Update indexes only when the behavior needs lookup, uniqueness, or query filtering.
4. Update relationships only when ownership changed.
5. Update query handler DTO projections so read models receive the canonical shape.
6. Update `EnsureTenantIntegrity` when a new or changed relationship can connect two tenant-owned records.
7. Keep the tenant-integrity rule in the owning module `DbContext/TenantIntegrity` folder, not in command handlers or service-registration extensions.
8. Use `OnBeforeSaveChangesRules()` for tenant integrity and other pre-audit invariants.
9. Use `OnBeforeSaveChanges()` for auto history after audit fields are applied.
10. Run a targeted build of the persistence project when the stage is meant to compile.

Query projection structure:

- Keep DTO projection in the query handler that owns the read use case.
- Use `AsNoTracking()` and project directly to the DTO with `Select(...)`.
- Avoid shared read-shaping helpers for GET paths; they encourage entity graph materialization before shaping the response.
- Load optional or high-cardinality child collections only when the request needs them.

Do not run migration commands here. Leave a manual migration note that lists the intended schema operations, for example:

- add, remove, or rename columns changed by the current stage
- backfill required values when a new non-null column is introduced
- drop indexes or constraints that belong only to deleted fields

The operational migration tool is documented in [`backend/tools/migration-tool.md`](backend/tools/migration-tool.md), but migration execution is a separate manual step. Do not create or run migrations for a module unless that module is the explicit schema target.

## Step 5: Update DTO Contracts

Relevant locations:

- Tenant DTOs: `dotnet/Querify.Models.Tenant`
- QnA DTOs: `dotnet/Querify.Models.QnA/Dtos/<Feature>`
- Direct DTOs: `dotnet/Querify.Models.Direct`
- Broadcast DTOs: `dotnet/Querify.Models.Broadcast`
- Trust DTOs: `dotnet/Querify.Models.Trust` when Trust contracts are in scope
- User DTOs: `dotnet/Querify.Models.User`

Use the DTO structure rules in [`backend/architecture/repository-rules.md`](backend/architecture/repository-rules.md).

Process:

1. Update read DTOs for query responses.
2. Update create and update request DTOs.
3. Update list request DTOs when filters changed.
4. Remove obsolete DTO fields instead of keeping compatibility aliases that duplicate model meaning.
5. Keep module write-side request DTOs flat and explicit.
6. Keep link DTOs under the owning feature folder, such as `Dtos/Question`, `Dtos/Answer`, or `Dtos/Space`.
7. Do not add one module's DTO fields to another module's contracts unless the field describes a reusable asset owned by that target module.
8. Source DTOs should not expose duplicate citation permission fields. Use `Visibility` for audience exposure and link-level `SourceRole` for relationship meaning.

Do not create catch-all DTO files.

## Step 6: Update Backend Behavior

Relevant documents:

- [`backend/architecture/dotnet-backend-overview.md`](backend/architecture/dotnet-backend-overview.md)
- [`backend/architecture/solution-cqrs-write-rules.md`](backend/architecture/solution-cqrs-write-rules.md)
- [`backend/architecture/repository-rules.md`](backend/architecture/repository-rules.md)

Relevant module locations:

- Tenant API hosts: `dotnet/Querify.Tenant.BackOffice.Api`, `dotnet/Querify.Tenant.Portal.Api`, `dotnet/Querify.Tenant.Public.Api`
- Tenant worker host: `dotnet/Querify.Tenant.Worker.Api`
- Tenant business modules: `dotnet/Querify.Tenant.<Surface>.Business.<Feature>`
- QnA API hosts: `dotnet/Querify.QnA.Portal.Api`, `dotnet/Querify.QnA.Public.Api`
- QnA worker host: `dotnet/Querify.QnA.Worker.Api`
- QnA business modules: `dotnet/Querify.QnA.<Surface>.Business.<Feature>`
- Direct persistence module: `dotnet/Querify.Direct.Common.Persistence.DirectDb`
- Broadcast persistence module: `dotnet/Querify.Broadcast.Common.Persistence.BroadcastDb`

Every module uses the same feature-scoped module pattern. Keep behavior out of another module's handlers unless the use case is explicitly reading or writing an asset owned by that other module.

For each affected feature, update or remove:

- `Commands/<Action>/<Action>Command.cs`
- `Commands/<Action>/<Action>CommandHandler.cs`
- `Queries/<Action>/<Action>Query.cs`
- `Queries/<Action>/<Action>QueryHandler.cs`
- `Service/<Feature>Service.cs`
- `Abstractions/I<Feature>Service.cs`
- `Controllers/<Feature>Controller.cs`
- `Extensions/ServiceCollectionExtensions.cs`
- API host `Extensions/ServiceCollectionExtensions.cs` when a feature module is added or removed

Rules:

- Controllers map HTTP only.
- Services stay thin and delegate to MediatR.
- Telemetry spans belong in the service layer by default. The standard flow is
  `Controller -> Service (Telemetry) -> Command/Query`,
  `Consumer -> Service (Telemetry) -> Consumers (Only folder) -> Command/Query`,
  `HostedService -> ProcessorService (Telemetry) -> Hosted (Only folder) -> Command/Query`,
  `Hangfire BackgroundService -> Service (Telemetry) -> BackgroundServices (Only folder) -> Command/Query`,
  and `Event -> NotificationService (Telemetry) -> Command/Query`. Do not put feature
  telemetry first in controllers, consumers, hosted services, Hangfire background job classes,
  command handlers, or query handlers unless the implementation prompt explicitly asks for that
  exception.
- `Consumers`, `Hosted`, and `BackgroundServices` folders are adapter-only folders. They call a
  service layer that owns telemetry and then dispatches command/query behavior.
- Hosted-service services use the `ProcessorService` suffix. A `ProcessorService`
  coordinates only: open telemetry, set tags, and call one MediatR command or query.
  It must not own EF queries, broker publish/consume behavior, storage operations,
  retry/finalization rules, or domain decisions.
- Hangfire background job classes are schedulers/adapters only. They call a service that owns
  telemetry and dispatches commands/queries. Use persisted Hangfire storage for production jobs
  whose loss would leave user-visible state stuck or unsafe.
- Command handlers orchestrate validation, loading, state mutation, persistence, and side effects.
- Query handlers load and shape read DTOs.
- Domain decisions and pure business rules live under the owning common domain
  `BusinessRules/<Feature>` folder. Do not keep domain behavior in worker or API
  `Services`; move it to the common domain rules and call it from commands/queries.
- Write endpoints return simple write results.
- GET endpoints return read DTOs.
- Action route segments use lowercase kebab-case.
- Portal flows use authenticated tenant context, usually `X-Tenant-Id`.
- Public QnA flows resolve tenant through `X-Client-Key`.

When deleting behavior, remove the obsolete endpoint, service method, command, query, handler, helper, extension method, registration, factory path, seed branch, UI hook, API client method, and tests together. Do not leave dead API surface or support classes that write ignored fields, wrap deleted enum values, or preserve workflows that no longer exist.

## Step 7: Update Seed Data

Relevant document:

- [`backend/tools/seed-tool.md`](backend/tools/seed-tool.md)

Relevant locations:

- `dotnet/Querify.Tools.Seed/Application/TenantSeedService.cs`
- `dotnet/Querify.Tools.Seed/Application/QnASeedService.cs`
- `dotnet/Querify.Tools.Seed/Application/QnASeedCatalog.cs`
- `dotnet/Querify.Tools.Seed/Application/QnASeedCatalog.*.cs`
- `dotnet/Querify.Tools.Seed/Configuration`
- `dotnet/Querify.Tools.Seed/Infrastructure`

Process:

1. Update seed entities so they compile against the canonical model.
2. Remove obsolete fields from seed object construction.
3. Add realistic examples for every new behavior that needs to be visible in local development.
4. Keep sample data deterministic where tests or demos depend on it.
5. Ensure seed scenarios cover the product modes that matter for the value proposition.

For the QnA operating model, seed examples demonstrate:

- active QnA spaces and canonical questions
- questions and answers with active lifecycle, optional accepted-answer metadata, and reuse-ready metadata
- source links that explain origin, context, evidence, and reusable references
- source records with artifact identity, visibility, valid metadata JSON, and verification metadata
- moderated contribution and accepted-answer behavior when it belongs to QnA
- answer activation and archival behavior where it is part of QnA lifecycle

The seed tool may apply EF migrations when it is executed, as described in [`backend/tools/seed-tool.md`](backend/tools/seed-tool.md). Do not use that runtime behavior as a substitute for the manual migration step during model work.

Direct and Broadcast seed data belongs in their own seed services and persistence contexts. Do not seed their behavior through QnA sample data.

Worker API hosts must register `Querify.Common.Infrastructure.Telemetry` with `AddTelemetry(...)`. Each worker business feature should expose a feature-specific `ActivitySourceName`, pass it to the host registration, and start feature spans inside `ProcessorService`, notification services, or Hangfire-called services rather than directly inside hosted services, Hangfire background job classes, consumers, commands, or queries. Hosted services and Hangfire background job classes must be schedulers only: they resolve/call a service; that service owns telemetry and MediatR dispatch; the command/query owns workflow behavior.

## Step 8: Update Tests

Relevant document:

- [`backend/testing/integration-testing-strategy.md`](backend/testing/integration-testing-strategy.md)

Relevant locations:

- `dotnet/Querify.QnA.Portal.Test.IntegrationTests`
- `dotnet/Querify.QnA.Public.Test.IntegrationTests`
- `dotnet/Querify.Tenant.BackOffice.Test.IntegrationTests`
- `dotnet/Querify.Tenant.Portal.Test.IntegrationTests`
- `dotnet/Querify.Tenant.Public.Test.IntegrationTests`
- `dotnet/Querify.Tenant.Worker.Test.IntegrationTests`
- `dotnet/Querify.Common.Architecture.Test.IntegrationTest`

Process:

1. Update test data factories first, especially required entity construction.
2. Update command/query tests for changed request DTOs and expected statuses.
3. Update public API tests when tenant resolution, client-key behavior, votes, feedback, or public questions changed.
4. Update architecture tests only when a new cross-cutting rule is introduced.
5. Delete tests for behavior that no longer exists.
6. Add regression tests for consolidation points so duplicate concepts do not reappear.

Use real infrastructure for integration behavior unless the dependency is intentionally outside the test boundary.

## Step 9: Update Portal Frontend

Relevant documents:

- [`frontend/architecture/portal-app.md`](frontend/architecture/portal-app.md)
- [`frontend/architecture/portal-app-ui-prompt-guidance.md`](frontend/architecture/portal-app-ui-prompt-guidance.md)
- [`frontend/testing/validation-guide.md`](frontend/testing/validation-guide.md)

Relevant locations:

- Domain API clients: `apps/portal/src/domains/<domain>/api.ts`
- Domain hooks: `apps/portal/src/domains/<domain>/hooks.ts`
- Domain types: `apps/portal/src/domains/<domain>/types.ts`
- Domain schemas: `apps/portal/src/domains/<domain>/schemas.ts`
- Domain routes: `apps/portal/src/domains/<domain>/routes.tsx`
- Domain pages: `apps/portal/src/domains/<domain>/*-page.tsx`
- Shared backend enum labels: `apps/portal/src/shared/constants/backend-enums.ts`
- Shared enum presentation metadata: `apps/portal/src/shared/constants/enum-ui.ts`
- Shared UI primitives: `apps/portal/src/shared/ui`
- Layout primitives: `apps/portal/src/shared/layout/page-layouts.tsx`

Process:

1. Update TypeScript types to match backend DTOs.
2. Update form schemas for new, removed, or consolidated fields.
3. Update API request builders and query keys.
4. Update list/detail/form pages with the smallest UX surface that supports the behavior.
5. Remove UI controls for deleted backend fields.
6. Prefer existing shared layouts, state components, field components, confirms, and status badges.
7. Keep workflows task-oriented and avoid explanatory copy that compensates for unclear UX.
8. Keep frontend source text, component names, helper names, comments, and `en-US` copy keys in English. Localize only through the locale catalogs; do not write Portuguese or other non-English source strings directly into components.
9. Keep the tenant/workspace switcher in the sidebar header and keep the top toolbar focused on route context and global utilities.
10. Use `ActionPanel` and `ActionButton` for screen-level and right-rail actions.
11. Use relationship tabs for child and related records that should be managed in the current screen context.
12. Keep relationship lists scoped to the current parent entity, including related tags and sources.
13. Use `ChildListPagination` for local child lists with more than five items; keep top-level list pagination governed by the page API contract.
14. Use `SearchSelect` or `SearchSelectField` for any select or dropdown backed by a backend list endpoint, including single-selection and relationship-linking flows.
15. Keep enum-only controls on the normal `Select` primitive and route labels, descriptions, and badge variants through the centralized enum presentation layer.
16. Give every editable field a concise field-level explanation. Use the shared field `description` prop whenever possible, add `hint` only for secondary caveats, and pair native inputs with a visible label plus `ContextHint`.
17. For dedicated Add/Edit forms, consider whether `FormSetupProgressCard` needs to be added or updated. Keep it below the main form card, derive steps from current form values, keep the default hide-at-100% behavior, and update [`frontend/architecture/portal-getting-started-guidance.md`](frontend/architecture/portal-getting-started-guidance.md).
18. When changing dashboard activation criteria, update dashboard setup progress from real workspace data, ensure no setup-progress or completion surface renders after progress reaches 100%, and update [`frontend/architecture/portal-getting-started-guidance.md`](frontend/architecture/portal-getting-started-guidance.md).
19. Keep top-level list pages usable from 320 CSS pixels through tablet and desktop. Below `xl`, list records should use stacked card rows and the shell should use the header/drawer instead of the fixed sidebar.
20. Prevent horizontal page overflow at the root, shell, page, card, filter, table, dialog, sheet, popover, pagination, and action levels. Use `min-w-0`, viewport constraints, and word breaking for long URLs, ids, checksums, user agents, and generated tokens.
21. For Source screens, keep UI controls, filters, badges, metrics, and setup-progress steps aligned to canonical fields such as `visibility`, verification state, usage counts, and relationship role. If `MetadataJson` is editable, validate JSON before submit and provide a formatting affordance.
22. Verify loading, empty, error, pending, success, and destructive-action states.
23. Verify light and dark mode whenever the change touches layout, colors, cards, tables, forms, actions, or badges.
24. For lifecycle-driven guidance, keep Draft as the review state, Active as the usable/reusable state, and Archived as removed from active use. Do not route users to create answers, attach sources, or choose accepted answers solely because a Question is Active.

For large frontend changes, update one domain at a time and keep each domain buildable before moving to the next.

## Step 10: Update All Portal Translations

Relevant document:

- [`frontend/architecture/portal-localization.md`](frontend/architecture/portal-localization.md)

Locale catalogs live in:

- `apps/portal/src/shared/lib/i18n/locales/*.json`

`en-US` is the source of truth. All user-facing frontend text must have an `en-US` key first, and every other locale must carry the same key set.

Current Portal locale files include:

- `en-US`
- `ar-SA`
- `bn-BD`
- `de-DE`
- `es-ES`
- `fr-FR`
- `he-IL`
- `hi-IN`
- `id-ID`
- `it-IT`
- `ja-JP`
- `ko-KR`
- `pl-PL`
- `pt-BR`
- `ru-RU`
- `th-TH`
- `tr-TR`
- `ur-PK`
- `vi-VN`
- `zh-CN`

Process:

1. Add or update `en-US.json` first.
2. Treat changed copy as a key lifecycle: list every added, renamed, and removed user-facing string before editing locale files.
3. Translate every added or changed key in each remaining locale one by one. Non-`en-US` values must be actual locale text, not copied English placeholders, unless the string is intentionally identical in that language.
4. Preserve placeholders exactly, such as `{name}` or `{value}`.
5. Do not concatenate translated fragments in components.
6. Do not put frontend presentation strings in backend DTOs.
7. Check RTL languages for direction-sensitive layout issues.
8. When a key is renamed or replaces another concept, translate the new value in every locale and delete the replaced key from every locale after confirming it has no live references.
9. Validate that every locale has exactly the same key set as `en-US`; missing or extra keys must be fixed in the same change.
10. Validate that removed UI copy no longer leaves unused locale keys behind. Search for obsolete keys after deleting fields, enum values, pages, filters, or labels, and remove stale entries from every locale file.
11. When behavior is consolidated, add keys for the canonical field and remove obsolete copy from every locale file.

When a behavior removes UI copy, remove obsolete keys from every locale file in the same change. This includes legacy copy for renamed fields, deleted filters, removed enum labels, and deleted routes.

Before finishing a Portal copy change, run exact searches for every removed or replaced key outside the locale catalogs:

```bash
rg -n --fixed-strings "Removed or replaced UI string" apps/portal/src docs --glob '!apps/portal/src/shared/lib/i18n/locales/*.json'
```

If the search returns no live references, remove that key from every locale file. If it still returns live references, either keep the key or update the remaining caller to the canonical copy before deleting it.

Also check the new or changed keys in every non-`en-US` locale. A changed key whose value still exactly equals the English key is a localization defect unless the text is a proper noun, code token, route, or deliberately untranslated product term.

## Step 11: Verify The Stage

Use targeted validation first, then broaden.

Backend model and persistence stage:

```bash
dotnet build dotnet/Querify.Models.Common/Querify.Models.Common.csproj -v minimal --no-restore
dotnet build dotnet/Querify.Models.Tenant/Querify.Models.Tenant.csproj -v minimal --no-restore
dotnet build dotnet/Querify.QnA.Common.Domain/Querify.QnA.Common.Domain.csproj -v minimal --no-restore
dotnet build dotnet/Querify.QnA.Common.Persistence.QnADb/Querify.QnA.Common.Persistence.QnADb.csproj -v minimal --no-restore
dotnet build dotnet/Querify.Models.QnA/Querify.Models.QnA.csproj -v minimal --no-restore
```

When the stage touches module-specific persistence projects:

```bash
dotnet build dotnet/Querify.Direct.Common.Persistence.DirectDb/Querify.Direct.Common.Persistence.DirectDb.csproj -v minimal --no-restore
dotnet build dotnet/Querify.Broadcast.Common.Persistence.BroadcastDb/Querify.Broadcast.Common.Persistence.BroadcastDb.csproj -v minimal --no-restore
```

Backend feature stage:

```bash
dotnet build dotnet/Querify.QnA.Portal.Business.Question/Querify.QnA.Portal.Business.Question.csproj -v minimal --no-restore
dotnet build dotnet/Querify.QnA.Public.Business.Question/Querify.QnA.Public.Business.Question.csproj -v minimal --no-restore
```

Backend test stage:

```bash
dotnet test dotnet/Querify.QnA.Portal.Test.IntegrationTests/Querify.QnA.Portal.Test.IntegrationTests.csproj
dotnet test dotnet/Querify.QnA.Public.Test.IntegrationTests/Querify.QnA.Public.Test.IntegrationTests.csproj
dotnet test dotnet/Querify.Common.Architecture.Test.IntegrationTest/Querify.Common.Architecture.Test.IntegrationTest.csproj
```

Frontend stage:

```bash
cd apps/portal
npm run lint
npm run build
```

Frontend manual validation should also confirm:

- the workspace switcher remains in the sidebar header
- the fixed sidebar appears only at desktop widths and the mobile/tablet header plus drawer remains active below `xl`
- relationship sections behave as local tabs, not anchors or redirects to global list pages
- child lists with more than five items expose 5, 10, and 20 item pagination
- backend-list selects use the searchable select pattern
- editable fields have both a clear label and field-level explanation through `description`, `hint`, or a visible label paired with `ContextHint`
- action panels use the shared square, lightly rounded action identity
- affected top-level list pages do not create horizontal page overflow at 320, 360, 375, 414, 768, 1024, 1279, 1280, and a wider desktop width
- list records, filters, toolbar actions, pagination, cards, tables, dialogs, sheets, and `SearchSelect` surfaces can shrink or wrap without making the page wider than the viewport
- affected screens remain readable in light and dark themes

Localization validation should also confirm:

- all locale key sets match `apps/portal/src/shared/lib/i18n/locales/en-US.json`
- changed keys are translated in every non-`en-US` locale
- removed UI concepts do not leave unused locale keys behind
- exact searches for removed or replaced copy return no live references outside locale catalogs before the stale keys are deleted
- newly added non-`en-US` values are reviewed for accidental English fallback values

Documentation validation should also confirm:

- new or changed repository documentation is written in English even when the task discussion happened in another language
- documentation names only current fields, enum values, routes, and behaviors
- exact searches for deleted names return no documentation or locale hits outside historical migrations
- temporary migration notes are removed once the schema change lands

Do not run migration commands as part of this validation unless migration work is explicitly in scope.

## Step 12: Handoff Notes

End each staged behavior change with a short handoff:

- canonical concepts added, changed, or deleted
- module boundary chosen for the behavior
- old duplicate concepts removed
- projects that build
- projects that are intentionally not fixed yet
- tests run
- tests not run and why
- manual migration operations required
- frontend/i18n work remaining
- documentation cleanup performed
- missing Direct or Broadcast entity models that intentionally remain a follow-up

This makes staged work safe even when the full solution is expected to fail between stages.
