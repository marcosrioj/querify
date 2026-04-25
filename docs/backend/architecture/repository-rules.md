# Repository Rules

## Objective

Use existing BaseFaq architecture standards. Do not introduce parallel patterns.
When Commands or Queries become large, slice them into small, cohesive parts so behavior stays maintainable, testable, and reusable under SOLID + DDD.

## Authoritative Sources

1. [`solution-cqrs-write-rules.md`](solution-cqrs-write-rules.md)
2. [`../../../README.md`](../../../README.md)
3. [`../testing/integration-testing-strategy.md`](../testing/integration-testing-strategy.md)

If this file conflicts with those sources, those sources win.

## Scope

Apply these rules to:

- `BaseFaq.QnA.*`
- `BaseFaq.SupportCopilot.*`
- `BaseFaq.EngagementHub.*`
- `BaseFaq.Tenant.*`
- `BaseFaq.Common.*`
- `BaseFaq.Models.Common`
- `BaseFaq.Models.QnA`
- `BaseFaq.Models.Tenant`
- `BaseFaq.Models.User`
- `BaseFaq.Tools.*`

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

### 3. Handler quality

- `Handle(...)` remains short, explicit, and orchestration-focused.
- Split large logic into focused methods or classes without changing behavior.
- Preserve transaction boundaries and side-effect order during refactoring.

### 4. Test contract discipline

- Tests must adapt to production contracts; do not weaken production design for tests.
- If dependencies change, update unit test doubles and integration fixtures.
- Keep integration-first behavior with real DB and real migrations where defined.

### 5. Product physical module boundary

- Answer Hub backend modules currently use the QnA namespace and must keep the established feature-scoped physical decomposition style already used in the solution.
- Each Answer Hub/QnA entity or surface concern gets its own business project, for example `BaseFaq.QnA.Portal.Business.Question` or `BaseFaq.QnA.Public.Business.Vote`.
- Do not introduce monolithic aggregation projects such as `BaseFaq.QnA.Portal.Business` or `BaseFaq.QnA.Public.Business`.
- Keep QnA source files physically inside the owning feature project directory.
- Do not use linked source items such as `<Compile Include="..\\..." Link="...">` for QnA business feature projects.
- API hosts compose QnA modules through feature-level `Add*Business()` registrations.
- Integration tests reference the owning feature projects directly and follow feature folders such as `Tests/Question/QuestionCommandQueryTests.cs`.
- QnA command handlers and query handlers own the use-case logic directly, matching the solution's physical project pattern.
- Do not introduce generic QnA helper files such as `*Operations.cs`, `PagedQuery.cs`, `QnAProjectionMapper.cs`, `QnAActivityMetadata.cs`, or `SignalRequestContext.cs`.
- The only allowed QnA helper exception is a feature-specific request-context helper such as `FeedbackRequestContext.cs` or `VoteRequestContext.cs`.
- QnA persistence entities in `QnADb/Entities` must stay anemic.
- Do not add command-like methods, factory methods, behavior methods, or convenience projection properties to QnA persistence entities.
- Keep QnA state transitions, relation management, validation, and projection shaping inside commands, queries, and feature-local private methods.
- Support Copilot and Engagement Hub behavior must live in their own feature and persistence projects.
- `BaseFaq.SupportCopilot.Common.Persistence.SupportCopilotDb` and `BaseFaq.EngagementHub.Common.Persistence.EngagementHubDb` contain their initial entity models; extend them only for concrete product behavior, not placeholder coverage.

### 6. QnA model contract boundary

- `BaseFaq.Models.QnA` must keep the same feature-folder DTO layout used across the solution.
- Keep DTOs in real feature folders such as `Dtos/Question/QuestionDto.cs` or `Dtos/Answer/AnswerCreateRequestDto.cs`.
- Do not keep aggregate files such as `Dtos/QuestionDtos.cs` or any other `*Dtos.cs` catch-all file in `BaseFaq.Models.QnA`.
- Keep namespaces and file ownership coherent with the folder that owns the DTO.
- Do not introduce pseudo-entity folders such as `Dtos/Link`; link DTOs belong to the owning feature folders like `Dtos/Answer`, `Dtos/Question`, or `Dtos/Space`.
- QnA write-side `*RequestDto` types must be flat and must not inherit from other request DTO types.
- QnA query request DTOs for paged or sorted list reads may inherit the shared pagination base used by the project pattern.
- Each QnA write-side request DTO must declare its own properties explicitly.

## Folder Ownership Rules

| Location | Allowed | Not allowed |
|---|---|---|
| `Commands/<Action>` | write request plus handler orchestration | read DTO shaping, query concerns |
| `Queries/<Action>` | read handlers plus projection mapping | write mutations |
| `Controllers` | HTTP mapping such as status code, routing, request and response transport | business rules, data access logic |
| `Service` | use-case orchestration and coordination | HTTP concerns, DTO projection for reads after write |
| `Abstractions` | interfaces and contracts only | implementations or behavior |
| `Extensions` | DI and composition wiring only | business logic |
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

## Required Review Checklist

- command return type is a simple value only
- no read-after-write query in command paths
- write endpoints and services do not return read DTOs
- `Handle(...)` is bounded and oversized logic was decomposed using the slicing rules
- every extracted class has a single responsibility and clear name
- behavior stays in the owning command or query flow or feature-local collaborators, not controllers, generic helpers, or product persistence entities
- action structure follows the standard slicing pattern for commands and queries
- new behavior was added only to the correct folder ownership boundary
- tests were updated for dependency and contract changes
- no command response wrapper DTO was introduced
