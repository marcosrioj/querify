# Querify CQRS Write Rules

## Purpose

This document defines the mandatory write-side rules for Querify. The goal is to keep command flows predictable, thin, and aligned across all business modules.

For the extended companion — folder ownership rules, handler slicing guidelines, DTO contract rules, DbContext patterns, and the full review checklist — see [`repository-rules.md`](repository-rules.md).

## Scope

These rules apply to:

- `Querify.QnA.*`
- `Querify.Direct.*`
- `Querify.Broadcast.*`
- `Querify.Tenant.*`
- `Querify.Trust.*`

## Rule 1: commands return simple values

Preferred command return types:

- `Guid`
- `bool`
- `string`
- `void` / `IRequest`

Command handlers may return only those same simple values.
Complex response types belong to query handlers only.

Do not return:

- DTOs
- lists
- paged results
- wrapper objects that only hide a simple value
- complex handler response types such as `IRequestHandler<TCommand, TDto>`

## Rule 2: no read-after-write just to shape the response

Do not execute a command and then run a read query only to build a richer response for the same HTTP call.

Not allowed:

```csharp
await mediator.Send(command, token);
return await GetById(id, token);
```

Allowed:

```csharp
await mediator.Send(command, token);
return id;
```

## Rule 3: write controllers return write results

For `POST`, `PUT`, and `PATCH` endpoints:

- return a simple value that represents the write outcome
- do not return a read DTO built from a follow-up query

Read DTOs belong to `GET` endpoints and query handlers.

## Rule 3a: GET/query handlers are optimized read paths

For `GET` endpoints and query handlers:

- use `AsNoTracking()` by default
- project directly to the response DTO before materialization
- avoid `Include(...)` unless a measured, documented exception proves it is cheaper
- apply filters and sorting before `CountAsync`, `Skip`, and `Take`
- page parent rows before loading expensive child collections
- honor include flags by not loading omitted data
- add or update indexes and migrations when introducing new filters, sorts, or high-cardinality relationship lookups

## Rule 4: write services stay thin

Application services may orchestrate command dispatch, but they should not rebuild read models after the command completes.

Good examples:

- `Task<Guid> Create(...)`
- `Task<Guid> Update(...)`
- `Task<bool> Delete(...)`

## Rule 5: tests adapt to production behavior

If production code now requires a dependency, the tests must supply it.

Do not weaken production code by making dependencies optional just to satisfy tests.

## Rule 6: async request commands return correlation ids

When a command starts asynchronous work:

- return a `Guid` correlation id
- publish the event with the required metadata
- do not invent a composite "accepted response" DTO unless there is a real cross-cutting standard behind it

## Rule 7: handler methods stay orchestration-focused

`Handle(...)` methods should remain readable:

- split loading, validation, persistence, and publication into focused private methods when necessary
- avoid large nested branches
- keep the command contract unchanged while improving readability
- throw `ApiErrorException` for API-facing validation failures, missing resources, and domain
  workflow rejections; reserve `InvalidOperationException` for internal invariants that indicate a
  bug, invalid configuration, or persistence corruption
- use `HttpStatusCode.UnprocessableEntity` for user-correctable business rule failures such as an
  invalid status and visibility combination
- keep `ApiErrorException` messages stable enough for frontend localization, or coordinate a Portal
  alias in `apps/portal/src/platform/api/api-error.ts` when the message contains dynamic ids or
  operational details

## Rule 8: modules use feature-scoped projects

When backend work belongs to a Querify module:

- do not aggregate Portal behavior into a monolithic `<Module>.Portal.Business` project
- do not aggregate Public behavior into a monolithic `<Module>.Public.Business` project
- create or extend the smallest entity or surface project that owns the use case
- compose APIs from multiple feature registrations such as `AddQuestionBusiness()` and `AddVoteBusiness()`
- point integration tests at the owning feature projects rather than a monolithic business assembly
- keep source files physically inside the owning module feature project directory
- do not use linked source entries such as `<Compile Include="..\\..." Link="...">` in module business feature projects
- mirror the existing feature-scoped test folders and file names, for example `Tests/Question/QuestionCommandQueryTests.cs` or `Tests/Question/QuestionQueryTests.cs`
- keep use-case logic in the owning command and query handlers instead of generic helper classes
- do not introduce aggregate helpers such as `*Operations.cs`, `PagedQuery.cs`, `<Module>ProjectionMapper.cs`, `<Module>ActivityMetadata.cs`, or `SignalRequestContext.cs` in module business projects
- the allowed helper exception is a feature-specific request-context helper such as `FeedbackRequestContext.cs` or `VoteRequestContext.cs`

## Rule 9: module contract projects stay feature-organized

When the work belongs to a module contract project such as `Querify.Models.QnA`, `Querify.Models.Direct`, `Querify.Models.Broadcast`, or `Querify.Models.Tenant`:

- keep DTO folders feature-scoped such as `Dtos/Question/QuestionDto.cs`
- keep DTOs in real feature folders rather than aggregate `*Dtos.cs` files
- do not place catch-all DTO files directly under `dotnet/Querify.Models.<Module>/Dtos`
- do not create pseudo-entity folders such as `dotnet/Querify.Models.QnA/Dtos/Link`
- place link DTOs under the owning feature folders like `Dtos/Answer`, `Dtos/Question`, and `Dtos/Space`
- keep write-side handler request DTOs flat
- do not inherit one write-side `*RequestDto` from another `*RequestDto`
- let paged or sorted query request DTOs inherit the shared pagination base used by the project pattern
- declare request DTO properties explicitly on each write-side module request type

## Rule 10: Product persistence entities stay anemic

When the work belongs to a module entity folder such as `Querify.QnA.Common.Domain/Entities`, `DirectDb/Entities`, or `BroadcastDb/Entities`:

- keep entities as state-only persistence models
- do not add behavior methods, factory methods, or transition methods
- do not add convenience projection properties such as computed tag or source collections
- do not use `[NotMapped]` computed properties to hide query shaping inside entities
- keep relation creation, validation, status transitions, and DTO shaping inside commands, queries, feature-local private methods, or infrastructure-free domain business rules when they are reused across QnA surfaces
- keep tenant-owned relationship integrity in the owning module `DbContext/TenantIntegrity` rules, not in entities or repeated command-handler checks

Extend module entity models only for concrete behavior owned by that module; do not create placeholder entities or move one module's workflow into another module's entities as a shortcut.

## HTTP mapping guidance

- `POST create` -> `201` with the created `Guid`
- `PUT update` -> `200` with the updated `Guid` or a meaningful `bool`
- async request command -> `202` with a correlation `Guid`
- when a write endpoint uses an explicit action segment, the route segment should be lowercase kebab-case, for example `add-tenant-member` instead of `AddTenantMember`

## Review checklist

Use the full checklist in [`repository-rules.md`](repository-rules.md) → **Required Review Checklist**. That list is a superset of these CQRS rules and includes handler decomposition, folder ownership, and API error convention checks.

## Anti-patterns

- command handler returning `*Dto`
- command handler implementing `IRequestHandler<TCommand, TComplex>`
- command returning `List<T>`
- controller returning `CreatedAtAction(..., dto)` for a write flow that only needs an id
- services querying the database after the command only to return a richer payload
- making production dependencies nullable to satisfy tests
- introducing monolithic module business projects instead of feature-scoped modules
- using linked source files in module feature projects instead of real files in the owning project folder
- using aggregate `*Dtos.cs` files in module contract projects instead of feature-scoped DTO folders and files
- keeping generic module helper files instead of placing logic in command/query handlers
- introducing a pseudo-entity DTO folder such as `Querify.Models.QnA/Dtos/Link`
- adding behavior or computed projection properties to module persistence entities
- enforcing tenant-owned relationship integrity only in command handlers instead of the owning module `DbContext`
