# Repository Rules

## Objective

Use existing Querify architecture standards. Do not introduce parallel patterns.
When Commands or Queries become large, slice them into small, cohesive parts so behavior stays maintainable, testable, and reusable under SOLID + DDD.

## Authoritative Sources

1. [`solution-cqrs-write-rules.md`](solution-cqrs-write-rules.md)
2. [`../../../README.md`](../../../README.md)
3. [`../testing/integration-testing-strategy.md`](../testing/integration-testing-strategy.md)

If this file conflicts with those sources, those sources win.

## Scope

Apply these rules to:

- `Querify.QnA.*`
- `Querify.Direct.*`
- `Querify.Broadcast.*`
- `Querify.Trust.*`
- `Querify.Tenant.*`
- `Querify.Common.*`
- `Querify.Models.Common`
- `Querify.Models.QnA`
- `Querify.Models.Direct`
- `Querify.Models.Broadcast`
- `Querify.Models.Tenant`
- `Querify.Models.User`
- `Querify.Tools.*`

## Non-Negotiable Architecture Rules

### 1. CQRS write contract

- Command handlers return only simple values: `Guid`, `bool`, `string`, or `void`.
- Complex response types belong to query handlers only.
- Commands must never return DTOs, lists, paged results, or wrapper response objects.
- Command handlers must never implement `IRequestHandler<TCommand, TComplex>` where `TComplex` is a DTO, list, paged result, or wrapper object.
- No read-after-write inside command flow.
- Query DTOs are read-side only (`GET` plus query handlers).

### 2. Write path behavior

- `POST`, `PUT`, and `PATCH` endpoints return simple write outcomes only.
- Services and controllers stay orchestration-thin.
- For async write processing, return correlation `Guid` and use `202 Accepted`.

### 3. Read path performance

- Treat `GET` endpoints and query handlers as hot paths. Assume they may be called by millions of users.
- Query handlers must default to `AsNoTracking()` and project directly to the response DTO before materialization.
- Do not materialize EF entities only to call a mapper when the same shape can be expressed with `Select(...)`.
- Avoid `Include(...)` in query handlers. Use DTO projection, filtered child subqueries, or separate explicit queries for optional collections.
- Apply filters and sorting on `IQueryable` before `CountAsync`, `Skip`, and `Take`; page parent rows before loading expensive child details.
- Public query handlers must honor include flags by not loading omitted collections.
- For reads by primary key, unique slug, or another constrained unique key, use `FirstOrDefaultAsync` and rely on database constraints for uniqueness.
- When adding a query filter, search field, sort field, or high-cardinality relationship lookup, add or update the matching EF index and migration in the owning persistence project.
- Keep returned columns limited to the DTO contract; never return internal fields because they are easy to load.

### 4. Handler quality

- `Handle(...)` remains short, explicit, and orchestration-focused.
- Split large logic into focused methods or classes without changing behavior.
- Preserve transaction boundaries and side-effect order during refactoring.

### 5. Test contract discipline

- Tests must adapt to production contracts; do not weaken production design for tests.
- If dependencies change, update unit test doubles and integration fixtures.
- Keep integration-first behavior with real DB and real migrations where defined.

### 6. Querify module physical boundary

- Tenant, QnA, Direct, Broadcast, and Trust are the current Querify modules. Every module must keep the established feature-scoped physical decomposition style used by the solution.
- Each entity or surface concern gets its own business project inside the owning module, for example `Querify.QnA.Portal.Business.Question` or `Querify.QnA.Public.Business.Vote`.
- Do not introduce monolithic aggregation projects such as `Querify.<Module>.Portal.Business` or `Querify.<Module>.Public.Business`.
- Keep source files physically inside the owning module feature project directory.
- Do not use linked source items such as `<Compile Include="..\\..." Link="...">` for module business feature projects.
- API hosts compose module features through feature-level `Add*Business()` registrations.
- Integration tests reference the owning feature projects directly and follow feature folders such as `Tests/Question/QuestionCommandQueryTests.cs`.
- Command handlers and query handlers own the use-case logic directly, matching the solution's physical project pattern.
- Do not introduce generic module helper files such as `*Operations.cs`, `PagedQuery.cs`, `<Module>ProjectionMapper.cs`, `<Module>ActivityMetadata.cs`, or `SignalRequestContext.cs`.
- The allowed helper exception is a feature-specific request-context helper such as `FeedbackRequestContext.cs` or `VoteRequestContext.cs`.
- Module persistence entities must stay anemic.
- Do not add command-like methods, factory methods, behavior methods, or convenience projection properties to module persistence entities.
- Keep state transitions, relation management, validation, and projection shaping inside commands, queries, and feature-local private methods.
- Behavior must live in its owning feature and persistence projects.
- `Querify.Direct.Common.Persistence.DirectDb` and `Querify.Broadcast.Common.Persistence.BroadcastDb` contain their current entity models; extend them only for concrete module behavior, not placeholder coverage.

### 7. Module model contract boundary

- Module contract projects such as `Querify.Models.QnA`, `Querify.Models.Direct`, `Querify.Models.Broadcast`, and `Querify.Models.Tenant` must keep the same feature-folder DTO layout used across the solution.
- Keep DTOs in real feature folders such as `Dtos/Question/QuestionDto.cs` or `Dtos/Answer/AnswerCreateRequestDto.cs`.
- Do not keep aggregate files such as `Dtos/QuestionDtos.cs` or any other `*Dtos.cs` catch-all file in module contract projects.
- Keep namespaces and file ownership coherent with the folder that owns the DTO.
- Do not introduce pseudo-entity folders such as `Dtos/Link`; link DTOs belong to the owning feature folders like `Dtos/Answer`, `Dtos/Question`, or `Dtos/Space`.
- Write-side `*RequestDto` types must be flat and must not inherit from other request DTO types.
- Query request DTOs for paged or sorted list reads may inherit the shared pagination base used by the project pattern.
- Each write-side request DTO must declare its own properties explicitly.

### 8. Module DbContext and tenant integrity defaults

- Module `DbContext` classes live under `DbContext/<Module>DbContext.cs`.
- Save-time persistence concerns live under focused `DbContext/<Concern>` folders.
- `Extensions` folders are reserved for service collection registration and composition wiring.
- These rules are the default pattern for tenant module persistence.
- Module invariants and tenant-integrity checks that must run before audit/history belong in `OnBeforeSaveChangesRules()`.
- Auto-history capture belongs in `OnBeforeSaveChanges()` so it runs after soft-delete and audit rules.
- Tenant integrity is enforced by the owning module `DbContext`, not repeated in every command handler.
- When an `IMustHaveTenant` entity references another tenant-owned record, add or update a focused extension under `DbContext/TenantIntegrity/<Entity>TenantIntegrityExtension.cs`.
- Use one tenant-integrity extension per checked entity or relationship.
- Use `TenantIntegrityGuard` plus `TenantIntegrityLookupCacheBase` or a module-specific lookup cache for referenced tenant ids.
- Tenant lookup code must read referenced rows with `IgnoreQueryFilters()` so soft-delete and tenant filters do not hide invalid or missing relationships.
- Do not add empty tenant-integrity extensions for entities with no tenant-owned relationships; record that no rule is needed in the change notes or tests instead.

## Folder Ownership Rules

| Location | Allowed | Not allowed |
|---|---|---|
| `Commands/<Action>` | write request plus handler orchestration | read DTO shaping, query concerns |
| `Queries/<Action>` | read handlers plus projection mapping | write mutations |
| `Controllers` | HTTP mapping such as status code, routing, request and response transport | business rules, data access logic |
| `Service` | use-case orchestration and coordination | HTTP concerns, DTO projection for reads after write |
| `Abstractions` | interfaces and contracts only | implementations or behavior |
| `Extensions` | DI and composition wiring only | business logic |
| `DbContext` | context classes and save-time persistence concerns such as tenant integrity, audit, soft delete, and auto history | command/query orchestration or feature business workflow |
| `DbContext/TenantIntegrity` | one focused tenant relationship guard per entity or relationship | generic helpers unrelated to tenant-owned relationships |
| `Dtos`, `Enums`, `Models` | data shape and types only | domain or orchestration behavior |
| `Helpers` | generic pure helpers | use-case workflows or feature business rules |

Hard rule: do not place new behavior in folders that already exist for a different concern.

## Universal Command and Query Slicing Rules

1. One action, one intent boundary.
   - A command or query represents one use case.
   - Do not mix unrelated goals in one action, for example create plus report plus export.

2. Handler is orchestration, not business engine.
   - `Handle(...)` coordinates phases and delegates behavior.
   - Keep behavior in the owning action boundary and feature-local collaborators.
   - Do not move use-case behavior into controllers, generic helpers, transport services, or persistence entities.

3. Slice by explicit phases.
   - Use this order as default: validate -> authorize -> load -> apply domain behavior -> persist -> publish or integrate.
   - Query actions use: validate -> authorize -> load -> project or map -> return.

4. Keep collaborators cohesive.
   - Each extracted class has one reason to change.
   - Prefer small feature-local collaborators such as `Validator`, `Policy`, `Loader`, `Executor`, or `Projector` over one large utility class.

5. Apply decomposition triggers consistently.
   - Split when `Handle(...)` is roughly over 40 to 60 lines, has more than three branch families, or has more than two side-effect categories.
   - Split when a method name needs "and" to describe its responsibility.

6. Reuse with the right ownership boundary.
   - If used by one action only, keep it inside that action folder.
   - If reused by multiple actions, move to shared abstraction plus implementation in the correct layer.
   - Never move business behavior into `Controllers`, `Extensions`, or generic `Helpers` to reduce size.

## Standard Internal Structure for Large Actions

Use this sequence in order:

1. Keep the command or query contract unchanged.
2. Split `Handle(...)` into clearly named private phases first.
3. If still too large, extract feature-local collaborators inside the same action boundary:
   - `Commands/<Action>/`: `Command`, `Handler`, `Validator`, `Policy`, `Executor`
   - `Queries/<Action>/`: `Query`, `Handler`, `Filter`, `Projector` or `Mapper`, `ReadModelFactory` as needed
4. Keep orchestration order explicit in handler code so transaction and side-effect order is obvious.
5. If logic is reused across multiple actions, move it to the correct shared behavior location, not to `Extensions` or `Controllers`.
6. If flow is long-running or crosses external systems, convert to async orchestration and return correlation `Guid` with `202 Accepted`.

Never solve a big command by moving behavior to unrelated folders.

## API Write Response Conventions

- `POST` create -> `201` plus `Guid`
- `PUT` or `PATCH` update -> `200` plus `Guid` or `bool`
- async command request -> `202` plus correlation `Guid`

## API Error Conventions

Use the existing `ApiErrorException` from `Querify.Common.Infrastructure.ApiErrorHandling.Exception`
for every error that can be caused by an API request and should be returned to the caller as a
structured API response.

- missing resource -> `ApiErrorException(..., (int)HttpStatusCode.NotFound)`
- malformed, missing, or invalid request context -> `BadRequest`, `Unauthorized`, or `Forbidden`
  according to the existing middleware and endpoint pattern
- domain or workflow rule rejected by user input -> `UnprocessableEntity`
- duplicate or conflicting resource state -> `Conflict`

Do not throw `InvalidOperationException` from controllers, services, command handlers, or query
handlers for user-correctable API errors. The API error middleware only serializes
`ApiErrorException` into `{ errorCode, messageError, data }`; other exceptions can leak as developer
exception pages or generic server errors during local and hosted API runs.

Treat the `ApiErrorException` message as a frontend localization contract. Prefer stable,
user-facing English messages that can be used as Portal translation keys. If the message must
include dynamic values such as ids, client keys, tenant ids, or header names, make sure the Portal
maps it in `apps/portal/src/platform/api/api-error.ts` to a stable localized message before it is
shown in a toast or popup. Do not add feature-local exception helpers for this; use the shared
`ApiErrorException` and update the frontend error catalog when the API surface changes.

`InvalidOperationException` remains acceptable for internal invariants that indicate a programming,
configuration, or persistence corruption problem and are not expected to be recovered by changing
the request payload. When a persistence invariant mirrors a request-time rule, validate it first in
the owning API handler with `ApiErrorException` and keep the persistence rule only as a defensive
fallback.

## Required Review Checklist

- command return type is a simple value only
- no read-after-write query in command paths
- write endpoints and services do not return read DTOs
- `Handle(...)` is bounded and oversized logic was decomposed using the slicing rules
- every extracted class has a single responsibility and clear name
- behavior stays in the owning command or query flow or feature-local collaborators, not controllers, generic helpers, or module persistence entities
- query handlers use no-tracking DTO projections and avoid `Include` unless a documented exception proves it is the cheaper read path
- new query filters, sorts, and relationship lookups have matching indexes and migrations
- action structure follows the standard slicing pattern for commands and queries
- new behavior was added only to the correct folder ownership boundary
- tenant-owned relationship changes update the owning `DbContext/TenantIntegrity` rule
- tests were updated for dependency and contract changes
- no command response wrapper DTO was introduced
- API-facing validation errors use `ApiErrorException`, not `InvalidOperationException`
