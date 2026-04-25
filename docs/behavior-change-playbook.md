# Behavior Change Playbook

## Purpose

Use this playbook when a change adds, updates, deletes, or consolidates BaseFaq module behavior across the BaseFAQ solution.

This is the workflow for changes that start in a module model and affect persistence, contracts, commands, queries, services, APIs, seed data, tests, Portal screens, and translations.

The goal is not to add layers. The goal is to keep the model simple, preserve supported behavior, and remove duplicated concepts before they spread through the rest of the system.

The current BaseFaq module split is documented in [`business/value_proposition.md`](business/value_proposition.md). The current modules are Tenant, QnA, Direct, Broadcast, and Trust. Tenant owns the control plane. QnA, Direct, Broadcast, and Trust own product behavior. Treat each module persistence boundary as the owner of its own behavior; do not park behavior in QnA because the QnA model already has a channel, source, or activity enum value that sounds close.

## Authority And Precedence

Start with the existing documentation before making code changes:

1. Work routing: [`execution-guide.md`](execution-guide.md).
2. Product boundaries and value model: [`business/value_proposition.md`](business/value_proposition.md).
3. Backend architecture: [`backend/architecture/solution-architecture.md`](backend/architecture/solution-architecture.md) and [`backend/architecture/dotnet-backend-overview.md`](backend/architecture/dotnet-backend-overview.md).
4. CQRS and repository rules: [`backend/architecture/solution-cqrs-write-rules.md`](backend/architecture/solution-cqrs-write-rules.md) and [`backend/architecture/repository-rules.md`](backend/architecture/repository-rules.md).
5. Backend tests: [`backend/testing/integration-testing-strategy.md`](backend/testing/integration-testing-strategy.md).
6. Seed and migration tools: [`backend/tools/seed-tool.md`](backend/tools/seed-tool.md) and [`backend/tools/migration-tool.md`](backend/tools/migration-tool.md).
7. Portal architecture and UX: [`frontend/architecture/portal-app.md`](frontend/architecture/portal-app.md) and [`frontend/architecture/portal-app-ui-prompt-guidance.md`](frontend/architecture/portal-app-ui-prompt-guidance.md).
8. Portal localization: [`frontend/architecture/portal-localization.md`](frontend/architecture/portal-localization.md).
9. Frontend validation: [`frontend/testing/validation-guide.md`](frontend/testing/validation-guide.md).

If those documents do not describe the behavior you are changing, inspect the closest existing implementation and follow its current pattern. If the new behavior establishes a reusable pattern, update the most specific owning document and link it from [`docs/README.md`](README.md) when needed.

## Non-Negotiable Rules

- Do not run or generate EF migrations during this workflow unless the user explicitly asks for migration work. Schema changes must be left as a clear manual migration follow-up.
- Do not remove `required` modifiers or add default values to hide object construction errors unless the user explicitly asks for that compatibility tradeoff.
- Do not add a new enum, property, DTO field, or UI concept if an existing one already represents the same business dimension.
- Do not keep deprecated behavior alive by retaining duplicate fields. Preserve behavior by mapping it to the canonical concept.
- Product persistence entities stay anemic. They contain state only, not behavior methods, factory methods, transition methods, or computed projection helpers.
- `TenantDbContext`, `QnADbContext`, `DirectDbContext`, and `BroadcastDbContext` are separate module ownership boundaries.
- Do not add Direct conversation, handoff, ticket-resolution, or agent-assist workflow state to QnA entities.
- Do not add Broadcast social, public-comment, mention, community-thread, or campaign interaction workflow state to QnA entities.
- Do not add Trust validation, governance, decision-history, or auditability state to QnA, Direct, Broadcast, or Tenant entities.
- `BaseFaq.Direct.Common.Persistence.DirectDb` and `BaseFaq.Broadcast.Common.Persistence.BroadcastDb` contain their current module entity models. Write or update those entities only for concrete module behavior; do not add placeholder entities or empty folders only to satisfy a split.
- Command handlers return simple values only. Complex DTOs belong to queries.
- Portal UI copy is frontend-owned. Backend DTOs should not return translated labels.

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
rg --files dotnet/BaseFaq.Models.Tenant dotnet/BaseFaq.Models.QnA dotnet/BaseFaq.Models.Direct dotnet/BaseFaq.Models.Broadcast dotnet/BaseFaq.Common.EntityFramework.Tenant dotnet/BaseFaq.QnA.Common.Persistence.QnADb dotnet/BaseFaq.Direct.Common.Persistence.DirectDb dotnet/BaseFaq.Broadcast.Common.Persistence.BroadcastDb apps/portal/src/domains
```

Capture these facts before editing:

- which entity owns the persisted state
- which BaseFaq module owns the behavior: Tenant, QnA, Direct, Broadcast, or Trust
- which enum expresses lifecycle, channel, audience, mode, role, actor, or event history
- which DTOs expose the state
- which handlers mutate or query it
- which seed files create examples
- which tests assert the old behavior
- which Portal screens and translations expose it
- which documentation already describes the pattern
- whether the owning module already has the owning entity; if it does not, record a staged follow-up instead of storing that behavior in another module

If two names describe the same business dimension, consolidate them before propagating the model.

## Step 2: Normalize The Business Concept

Classify each field or enum into one business dimension. There should be one canonical representation per dimension.

Use these dimensions for module behavior:

| Dimension | Canonical location | Meaning |
|---|---|---|
| Module boundary | owning persistence project | Which BaseFaq module owns the behavior: Tenant, QnA, Direct, Broadcast, or Trust. |
| Operating mode | `SpaceKind` | How participation works: controlled publication, moderated collaboration, or public validation. |
| Lifecycle state | `QuestionStatus`, `AnswerStatus` | Where a question or answer is in workflow. |
| Audience exposure | `VisibilityScope` | Who can see the item. This is not status and not moderation. |
| Channel | `ChannelKind` | Where a question, vote, feedback, or signal entered the system. |
| Answer provenance | `AnswerKind` | Whether an answer is official, community-provided, or imported. |
| Source material type | `SourceKind` | What artifact is linked: article, page, ticket, thread, audit record, and so on. |
| Source relationship role | `SourceRole` | Why a source is attached: origin, evidence, citation, canonical reference, audit proof. |
| Actor type | `ActorKind` | Who or what caused an activity event. |
| Event journal | `ActivityKind` | What happened historically. This is not current state. |
| Search rendering | `SearchMarkupMode` | How search-facing markup behaves for a space. |

Common consolidation rules:

- A mode enum should not be duplicated by policy enums plus boolean review flags.
- A question type enum should not duplicate origin channel, lifecycle status, or the parent space mode.
- A source kind should describe the artifact. A source role should describe why it is linked.
- An activity kind should describe an event that happened, not a field that can be edited directly.
- A visibility value should not imply moderation, approval, or indexing beyond audience exposure.
- Capability booleans are acceptable only when they represent an independent switch, such as whether a space accepts questions or answers.
- Channel, source, or activity values may record where a module asset came from, but they must not become the persistence home for another module's workflow.
- If a module behavior has no owning entity yet, stage the entity model before adding module-specific fields to another module as a shortcut.

## Step 3: Update Entities And Enums

Relevant locations:

- Common module enum: `dotnet/BaseFaq.Models.Common/Enums/ModuleEnum.cs`
- Tenant contracts and entities when the behavior belongs to tenant control plane: `dotnet/BaseFaq.Models.Tenant`, `dotnet/BaseFaq.Common.EntityFramework.Tenant`
- QnA contracts: `dotnet/BaseFaq.Models.QnA/Enums`
- QnA persistence entities: `dotnet/BaseFaq.QnA.Common.Persistence.QnADb/Entities`
- Direct contracts and persistence entities: `dotnet/BaseFaq.Models.Direct`, `dotnet/BaseFaq.Direct.Common.Persistence.DirectDb/Entities`
- Broadcast contracts and persistence entities: `dotnet/BaseFaq.Models.Broadcast`, `dotnet/BaseFaq.Broadcast.Common.Persistence.BroadcastDb/Entities`
- Trust contracts and persistence entities use `BaseFaq.Models.Trust` and a Trust persistence boundary when those projects are in scope for the change

Process:

1. Add, update, or delete enum values only after Step 2 confirms there is no duplicate concept.
2. Add an XML summary to every enum option in the enum file. The summary must explain the behavior or decision represented by the option, not just restate the option name.
3. Update the owning entity with the smallest persisted shape that can execute the behavior.
4. Add an XML summary to every persisted property and navigation in the entity file. The summary must explain how the behavior uses the property, including when a timestamp or actor differs from `BaseEntity`/`AuditableEntity` state.
5. Before adding a persisted property, check whether `BaseEntity` or `AuditableEntity` already provides the needed state: `Id`, `CreatedDate`, `CreatedBy`, `UpdatedDate`, `UpdatedBy`, `DeletedDate`, `DeletedBy`, or `IsDeleted`.
6. Do not duplicate or shadow base entity state with module-specific fields such as `CreatedAtUtc`, `UpdatedAtUtc`, `DeletedAtUtc`, `ExternalCreatedBy`, or separate soft-delete flags unless the new field represents a distinct domain timestamp or actor.
7. Remove properties that duplicate the new canonical field.
8. Preserve existing behavior semantics by moving callers to the canonical field.
9. Keep entities state-only.
10. Keep `required` semantics explicit. Do not set silent defaults to make construction easier.
11. Do not create placeholder module entities. If a needed owning entity is still missing and the stage does not explicitly introduce it, leave a handoff note instead.
12. When a new or changed entity implements `IMustHaveTenant` and references another tenant-owned entity, update the owning `DbContext` to enforce tenant integrity before save. Follow the existing `EnsureTenantIntegrity` pattern: validate added and modified relationship rows, look up referenced tenants with `IgnoreQueryFilters()`, and throw on cross-tenant links or missing references.
13. If a tenant-owned entity has no tenant-owned relationships, record that no additional `EnsureTenantIntegrity` rule is needed instead of adding empty validation code.

When deleting a property or enum, immediately search for all compile-time and string references:

```bash
rg -n "DeletedProperty|DeletedEnum|DeletedValue" dotnet apps docs
```

Historical EF migration files may still mention old schema. Do not edit or regenerate migrations in this workflow unless migration work was explicitly requested.

## Step 4: Update EF Configurations And Read Mappings

Relevant locations:

- QnA configurations: `dotnet/BaseFaq.QnA.Common.Persistence.QnADb/Configurations`
- QnA DbContext: `dotnet/BaseFaq.QnA.Common.Persistence.QnADb/QnADbContext.cs`
- QnA read mappings: `dotnet/BaseFaq.QnA.Common.Persistence.QnADb/Projections/QnAReadModelMappings.cs`
- Direct configurations, DbContext, and mappings under `dotnet/BaseFaq.Direct.Common.Persistence.DirectDb`
- Broadcast configurations, DbContext, and mappings under `dotnet/BaseFaq.Broadcast.Common.Persistence.BroadcastDb`
- Tenant persistence equivalents when the behavior is control-plane-owned.

Process:

1. Add property configuration for new persisted fields.
2. Remove configuration for deleted fields.
3. Update indexes only when the behavior needs lookup, uniqueness, or query filtering.
4. Update relationships only when ownership changed.
5. Update projection mappings so DTOs and query handlers receive the canonical shape.
6. Update `EnsureTenantIntegrity` when a new or changed relationship can connect two tenant-owned records.
7. Run a targeted build of the persistence project when the stage is meant to compile.

Do not run migration commands here. Leave a manual migration note that lists the intended schema operations, for example:

- remove `Spaces.ProductSurface`
- remove `Spaces.ModerationPolicy`
- remove `Spaces.RequiresQuestionReview`
- remove `Questions.Kind`

The operational migration tool is documented in [`backend/tools/migration-tool.md`](backend/tools/migration-tool.md), but migration execution is a separate manual step. Do not create or run migrations for a module unless that module is the explicit schema target.

## Step 5: Update DTO Contracts

Relevant locations:

- Tenant DTOs: `dotnet/BaseFaq.Models.Tenant`
- QnA DTOs: `dotnet/BaseFaq.Models.QnA/Dtos/<Feature>`
- Direct DTOs: `dotnet/BaseFaq.Models.Direct`
- Broadcast DTOs: `dotnet/BaseFaq.Models.Broadcast`
- Trust DTOs: `dotnet/BaseFaq.Models.Trust` when Trust contracts are in scope
- User DTOs: `dotnet/BaseFaq.Models.User`

Use the DTO structure rules in [`backend/architecture/repository-rules.md`](backend/architecture/repository-rules.md).

Process:

1. Update read DTOs for query responses.
2. Update create and update request DTOs.
3. Update list request DTOs when filters changed.
4. Remove obsolete DTO fields instead of keeping compatibility aliases that duplicate model meaning.
5. Keep module write-side request DTOs flat and explicit.
6. Keep link DTOs under the owning feature folder, such as `Dtos/Question`, `Dtos/Answer`, or `Dtos/Space`.
7. Do not add one module's DTO fields to another module's contracts unless the field describes a reusable asset owned by that target module.

Do not create catch-all DTO files.

## Step 6: Update Backend Behavior

Relevant documents:

- [`backend/architecture/dotnet-backend-overview.md`](backend/architecture/dotnet-backend-overview.md)
- [`backend/architecture/solution-cqrs-write-rules.md`](backend/architecture/solution-cqrs-write-rules.md)
- [`backend/architecture/repository-rules.md`](backend/architecture/repository-rules.md)

Relevant module locations:

- Tenant API hosts: `dotnet/BaseFaq.Tenant.BackOffice.Api`, `dotnet/BaseFaq.Tenant.Portal.Api`, `dotnet/BaseFaq.Tenant.Public.Api`
- Tenant worker host: `dotnet/BaseFaq.Tenant.Worker.Api`
- Tenant business modules: `dotnet/BaseFaq.Tenant.<Surface>.Business.<Feature>`
- QnA API hosts: `dotnet/BaseFaq.QnA.Portal.Api`, `dotnet/BaseFaq.QnA.Public.Api`
- QnA business modules: `dotnet/BaseFaq.QnA.<Surface>.Business.<Feature>`
- Direct persistence module: `dotnet/BaseFaq.Direct.Common.Persistence.DirectDb`
- Broadcast persistence module: `dotnet/BaseFaq.Broadcast.Common.Persistence.BroadcastDb`

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
- Command handlers orchestrate validation, loading, state mutation, persistence, and side effects.
- Query handlers load and shape read DTOs.
- Write endpoints return simple write results.
- GET endpoints return read DTOs.
- Action route segments use lowercase kebab-case.
- Portal flows use authenticated tenant context, usually `X-Tenant-Id`.
- Public QnA flows resolve tenant through `X-Client-Key`.

When deleting behavior, remove the obsolete endpoint, service method, command, query, handler, and tests together. Do not leave dead API surface that writes ignored fields.

## Step 7: Update Seed Data

Relevant document:

- [`backend/tools/seed-tool.md`](backend/tools/seed-tool.md)

Relevant locations:

- `dotnet/BaseFaq.Tools.Seed/Application/TenantSeedService.cs`
- `dotnet/BaseFaq.Tools.Seed/Application/QnASeedService.cs`
- `dotnet/BaseFaq.Tools.Seed/Application/QnASeedCatalog.cs`
- `dotnet/BaseFaq.Tools.Seed/Application/QnASeedCatalog.*.cs`
- `dotnet/BaseFaq.Tools.Seed/Configuration`
- `dotnet/BaseFaq.Tools.Seed/Infrastructure`

Process:

1. Update seed entities so they compile against the canonical model.
2. Remove obsolete fields from seed object construction.
3. Add realistic examples for every new behavior that needs to be visible in local development.
4. Keep sample data deterministic where tests or demos depend on it.
5. Ensure seed scenarios cover the product modes that matter for the value proposition.

For the QnA operating model, seed examples demonstrate:

- controlled QnA spaces and canonical questions
- questions with approved answers and resolution-ready metadata
- source links that explain origin, evidence, citation, and canonical references
- moderated contribution and accepted-answer behavior when it belongs to QnA
- auditable validation records where they are part of answer trust

The seed tool may apply EF migrations when it is executed, as described in [`backend/tools/seed-tool.md`](backend/tools/seed-tool.md). Do not use that runtime behavior as a substitute for the manual migration step during model work.

Direct and Broadcast seed data belongs in their own seed services and persistence contexts. Do not seed their behavior through QnA sample data.

## Step 8: Update Tests

Relevant document:

- [`backend/testing/integration-testing-strategy.md`](backend/testing/integration-testing-strategy.md)

Relevant locations:

- `dotnet/BaseFaq.QnA.Portal.Test.IntegrationTests`
- `dotnet/BaseFaq.QnA.Public.Test.IntegrationTests`
- `dotnet/BaseFaq.Tenant.BackOffice.Test.IntegrationTests`
- `dotnet/BaseFaq.Tenant.Portal.Test.IntegrationTests`
- `dotnet/BaseFaq.Tenant.Public.Test.IntegrationTests`
- `dotnet/BaseFaq.Tenant.Worker.Test.IntegrationTests`
- `dotnet/BaseFaq.Common.Architecture.Test.IntegrationTest`

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
8. Verify loading, empty, error, pending, success, and destructive-action states.

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
2. Translate every changed key in each remaining locale one by one.
3. Preserve placeholders exactly, such as `{name}` or `{value}`.
4. Do not concatenate translated fragments in components.
5. Do not put frontend presentation strings in backend DTOs.
6. Check RTL languages for direction-sensitive layout issues.

When a behavior removes UI copy, remove obsolete keys from every locale file in the same change.

## Step 11: Verify The Stage

Use targeted validation first, then broaden.

Backend model and persistence stage:

```bash
dotnet build dotnet/BaseFaq.Models.Common/BaseFaq.Models.Common.csproj -v minimal --no-restore
dotnet build dotnet/BaseFaq.Models.Tenant/BaseFaq.Models.Tenant.csproj -v minimal --no-restore
dotnet build dotnet/BaseFaq.QnA.Common.Persistence.QnADb/BaseFaq.QnA.Common.Persistence.QnADb.csproj -v minimal --no-restore
dotnet build dotnet/BaseFaq.Models.QnA/BaseFaq.Models.QnA.csproj -v minimal --no-restore
```

When the stage touches module-specific persistence projects:

```bash
dotnet build dotnet/BaseFaq.Direct.Common.Persistence.DirectDb/BaseFaq.Direct.Common.Persistence.DirectDb.csproj -v minimal --no-restore
dotnet build dotnet/BaseFaq.Broadcast.Common.Persistence.BroadcastDb/BaseFaq.Broadcast.Common.Persistence.BroadcastDb.csproj -v minimal --no-restore
```

Backend feature stage:

```bash
dotnet build dotnet/BaseFaq.QnA.Portal.Business.Question/BaseFaq.QnA.Portal.Business.Question.csproj -v minimal --no-restore
dotnet build dotnet/BaseFaq.QnA.Public.Business.Question/BaseFaq.QnA.Public.Business.Question.csproj -v minimal --no-restore
```

Backend test stage:

```bash
dotnet test dotnet/BaseFaq.QnA.Portal.Test.IntegrationTests/BaseFaq.QnA.Portal.Test.IntegrationTests.csproj
dotnet test dotnet/BaseFaq.QnA.Public.Test.IntegrationTests/BaseFaq.QnA.Public.Test.IntegrationTests.csproj
dotnet test dotnet/BaseFaq.Common.Architecture.Test.IntegrationTest/BaseFaq.Common.Architecture.Test.IntegrationTest.csproj
```

Frontend stage:

```bash
cd apps/portal
npm run lint
npm run build
```

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
- missing Direct or Broadcast entity models that intentionally remain a follow-up

This makes staged work safe even when the full solution is expected to fail between stages.
