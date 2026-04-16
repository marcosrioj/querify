# PROJECT_RULES.md

## Objective
Use existing BaseFaq architecture standards. Do not introduce parallel patterns.
When Commands or Queries become large, slice them into small, cohesive parts so behavior stays maintainable, testable, and reusable under SOLID + DDD.

## Authoritative Sources (Read First)
1. `docs/standards/solution-cqrs-write-rules.md`
2. `README.md`
3. `docs/testing/integration-testing-strategy.md`

If this file conflicts with those sources, those sources win.

## Scope
Apply these rules to:
- `BaseFaq.Faq.*`
- `BaseFaq.QnA.*`
- `BaseFaq.Tenant.*`
- `BaseFaq.Common.*`
- `BaseFaq.Models.Common`
- `BaseFaq.Models.Faq`
- `BaseFaq.Models.QnA`
- `BaseFaq.Models.Tenant`
- `BaseFaq.Models.User`
- `BaseFaq.Tools.*`

## Non-Negotiable Architecture Rules

### 1) CQRS Write Contract
- Command handlers return only simple values: `Guid`, `bool`, `string`, or `void`.
- Commands must never return DTOs, lists, paged results, or wrapper response objects.
- No read-after-write inside command flow.
- Query DTOs are read-side only (`GET` + query handlers).

### 2) Write Path Behavior
- `POST`/`PUT`/`PATCH` endpoints return simple write outcomes only.
- Services and controllers stay orchestration-thin.
- For async write processing, return correlation `Guid` and use `202 Accepted`.

### 3) Handler Quality
- `Handle(...)` remains short, explicit, and orchestration-focused.
- Split large logic into focused methods/classes without changing behavior.
- Preserve transaction boundaries and side-effect order during refactoring.

### 4) Test Contract Discipline
- Tests must adapt to production contracts; do not weaken production design for tests.
- If dependencies change, update unit test doubles and integration fixtures.
- Keep integration-first behavior (real DB + real migrations where defined).

### 5) QnA Physical Module Boundary
- QnA backend modules must mirror the FAQ physical decomposition style.
- Each QnA entity or surface concern gets its own business project, for example `BaseFaq.QnA.Portal.Business.Question` or `BaseFaq.QnA.Public.Business.Vote`.
- Do not introduce monolithic aggregation projects such as `BaseFaq.QnA.Portal.Business` or `BaseFaq.QnA.Public.Business`.
- Keep QnA source files physically inside the owning feature project directory.
- Do not use linked source items such as `<Compile Include="..\\..." Link="...">` for QnA business feature projects.
- API hosts compose QnA modules through feature-level `Add*Business()` registrations.
- Integration tests reference the owning feature projects directly and follow FAQ-style feature folders such as `Tests/Question/QuestionCommandQueryTests.cs`.
- QnA command handlers and query handlers own the use-case logic directly, matching the FAQ physical pattern.
- Do not introduce generic QnA helper files such as `*Operations.cs`, `PagedQuery.cs`, `QnAProjectionMapper.cs`, `QnAActivityMetadata.cs`, or `SignalRequestContext.cs`.
- The only allowed QnA helper exception is a feature-specific request-context helper that mirrors FAQ naming, currently `FeedbackRequestContext.cs` and `VoteRequestContext.cs`.
- QnA persistence entities in `QnADb/Entities` must stay anemic.
- Do not add command-like methods, factory methods, behavior methods, or convenience projection properties to QnA persistence entities.
- Keep QnA state transitions, relation management, validation, and projection shaping inside commands, queries, and feature-local private methods.

### 6) QnA Model Contract Boundary
- `BaseFaq.Models.QnA` must mirror the physical DTO layout style already used by `BaseFaq.Models.Faq`.
- Keep DTOs in real feature folders such as `Dtos/Question/QuestionDto.cs` or `Dtos/Answer/AnswerCreateRequestDto.cs`.
- Do not keep aggregate files such as `Dtos/QuestionDtos.cs` or any other `*Dtos.cs` catch-all file in `BaseFaq.Models.QnA`.
- Keep namespaces and file ownership coherent with the folder that owns the DTO.
- Do not introduce pseudo-entity folders such as `Dtos/Link`; link DTOs belong to the owning feature folders like `Dtos/Answer`, `Dtos/Question`, or `Dtos/QuestionSpace`.

## Folder Ownership Rules (Do Not Mix Responsibilities)

| Location | Allowed | Not Allowed |
|---|---|---|
| `Commands/<Action>` | Write request + handler orchestration | Read DTO shaping, query concerns |
| `Queries/<Action>` | Read handlers + projection mapping | Write mutations |
| `Controllers` | HTTP mapping (`status code`, routing, request/response transport) | Business rules, data access logic |
| `Service` | Use-case orchestration and coordination | HTTP concerns, DTO projection for reads after write |
| `Abstractions` | Interfaces/contracts only | Implementations or behavior |
| `Extensions` | DI and composition wiring only | Business logic |
| `Dtos`, `Enums`, `Models` | Data shape/types only | Domain or orchestration behavior |
| `Helpers` | Generic pure helpers | Use-case workflows or feature business rules |

Hard rule: do not place new behavior in folders that already exist for a different concern.

## Universal Command/Query Slicing Rules (Apply to Every Action)

1. One action, one intent boundary.
   - A Command/Query represents one use-case.
   - Do not mix unrelated goals in one action (for example create + report + export).

2. Handler is orchestration, not business engine.
   - `Handle(...)` coordinates phases and delegates behavior.
   - Keep behavior in the owning action boundary and feature-local collaborators.
   - Do not move use-case behavior into controllers, generic helpers, transport services, or persistence entities.

3. Slice by explicit phases.
   - Use this order as default: validate -> authorize -> load -> apply domain behavior -> persist -> publish/integrate.
   - Query actions use: validate -> authorize -> load -> project/map -> return.

4. Keep collaborators cohesive (SRP).
   - Each extracted class has one reason to change.
   - Prefer small feature-local collaborators (`Validator`, `Policy`, `Loader`, `Executor`, `Projector`) over one large utility class.

5. Apply decomposition triggers consistently.
   - Split when `Handle(...)` is roughly over 40-60 lines, has more than 3 branch families, or has more than 2 side-effect categories.
   - Split when a method name needs "and" to describe its responsibility.

6. Reuse with the right ownership boundary.
   - If used by one action only, keep it inside that action folder.
   - If reused by multiple actions, move to shared abstraction + implementation in the correct layer.
   - Never move business behavior into `Controllers`, `Extensions`, or generic `Helpers` to reduce size.

## Standard Internal Structure for Large Actions

Use this sequence in order:

1. Keep the command/query contract unchanged.
2. Split `Handle(...)` into clearly named private phases first.
3. If still too large, extract feature-local collaborators inside the same action boundary:
   - `Commands/<Action>/`: `Command`, `Handler`, `Validator`, `Policy`, `Executor` (or equivalent phase names).
   - `Queries/<Action>/`: `Query`, `Handler`, `Filter`, `Projector/Mapper`, `ReadModelFactory` (as needed).
4. Keep orchestration order explicit in handler code so transaction/side-effect order is obvious.
5. If logic is reused across multiple actions, move it to the correct shared behavior location (`Abstractions` + `Service` implementation), not to `Extensions` or `Controllers`.
6. If flow is long-running or crosses external systems, convert to async orchestration and return correlation `Guid` (`202 Accepted`).

Never solve "big command" by moving behavior to unrelated folders.

## API Write Response Conventions
- `POST` create -> `201` + `Guid`
- `PUT/PATCH` update -> `200` + `Guid` (or `bool` when semantically correct)
- Async command request -> `202` + correlation `Guid`

## Required Review Checklist
- [ ] Command return type is a simple value only.
- [ ] No read-after-write query in command paths.
- [ ] Write endpoints/services do not return read DTOs.
- [ ] `Handle(...)` is bounded; oversized logic was decomposed using the slicing rules.
- [ ] Every extracted class has a single responsibility and clear name.
- [ ] Behavior stays in the owning command/query flow or feature-local collaborators, not controllers, generic helpers, or QnA persistence entities.
- [ ] Action structure follows the standard slicing pattern for Commands/Queries.
- [ ] New behavior was added only to the correct folder ownership boundary.
- [ ] Tests were updated for dependency/contract changes.
- [ ] No command response wrapper DTO was introduced.
