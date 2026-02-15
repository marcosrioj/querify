# BaseFAQ Solution Rules: CQRS Write Flows

## Purpose

This document defines mandatory architectural rules for write flows (`Command` side) across the entire BaseFAQ solution.

These rules are intended to prevent:

- command handlers returning read models
- write services/controllers doing read-after-write queries
- coupling command flows to query DTOs
- regressions where tests force production behavior changes

## Scope

Applies to all projects in the solution, including:

- `BaseFaq.Faq.*`
- `BaseFaq.Tenant.*`
- `BaseFaq.AI.*`
- any future module using MediatR + Controllers

## Core Rules

### 1. Commands must return only simple types

Allowed command return types:

- `Guid` (preferred for created/requested entity/correlation id)
- `bool`
- `string`
- `void` (`IRequest`) when no result is needed

Not allowed for `Command` return types:

- DTOs (`*Dto`)
- lists (`List<T>`)
- paged/query result models
- "accepted response" objects that wrap simple values

## 2. No read-after-write in command flows

After `await mediator.Send(command, token)`, do **not** call query methods (for example `GetById`) just to build command response.

### Not allowed

```csharp
await mediator.Send(command, token);
return await GetById(id, token);
```

### Allowed

```csharp
await mediator.Send(command, token);
return id;
```

## 3. Controllers for write operations must return simple values

For `POST`/`PUT`/`PATCH` command endpoints:

- return simple values (`Guid`, `bool`, `string`)
- do not return read DTOs generated from a follow-up query

For read endpoints (`GET`), query DTO responses are still correct.

## 4. Services must stay thin for writes

Application services may orchestrate command dispatch, but must not reconstruct read models after command execution.

- `Create/Update/Vote/Request...` methods should return simple values
- read model retrieval remains in query methods only

## 5. Tests must adapt to production contracts

Tests must provide required dependencies for command handlers.

Do not make production dependencies optional only to satisfy tests.  
If a new dependency is required (for example `IPublishEndpoint`), inject a test double/mock in tests.

## 6. Events and request commands

When command intent is "request async processing":

- return `Guid` correlation id
- publish event with required metadata
- do not return composite response DTO for the command

## 7. Keep `Handle` methods small and readable

Mediator handler `Handle(...)` methods must stay orchestration-focused.

- if `Handle` grows beyond simple orchestration, split logic into private methods
- keep each private method focused on one step (validation, loading, persistence, publish, mapping)
- avoid deeply nested branches in `Handle`; move branching details to private helpers
- preserve behavior; this rule is for readability and maintainability, not changing contracts

## Implementation Guidance

## Write endpoint response mapping

- `POST create` -> `201` with `Guid` created id
- `PUT update` -> `200` with `Guid` updated id (or `bool` if operation semantics require it)
- async request command (queue/event trigger) -> `202` with `Guid` correlation id

## Interface conventions

Service interfaces should follow:

- `Task<Guid> Create(...)`
- `Task<Guid> Update(...)`
- `Task<bool> <action>(...)` only when true/false semantic is meaningful

## Query separation

Read shape is owned by queries:

- `GetById`, `GetAll`, `Search` may return DTOs/paged models
- commands must never return those query DTOs

## PR Checklist (Mandatory)

- [ ] Every `Command` returns only simple type (`Guid`/`bool`/`string`/`void`)
- [ ] No `await mediator.Send(command)` followed by `GetById`/query for command response
- [ ] Write controllers return simple types, not read DTOs
- [ ] Service write methods return simple types
- [ ] Tests updated to match command dependencies and contracts
- [ ] No new "AcceptedResponse"/command response wrapper DTO introduced for write commands
- [ ] Oversized `Handle` logic extracted into private methods for readability

## Anti-Patterns to Reject in Review

- command handler returning `*Dto`
- command returning `List<T>`
- command service doing query lookup only to return rich object
- controller returning `CreatedAtAction(..., dto)` for command result when only id is needed
- making dependencies nullable/optional in production code to satisfy tests

## Notes

- This rule set is solution-wide and supersedes local preferences in individual modules.
- If a write flow truly requires a rich response, model it as:
  1. execute command and return simple id/correlation id
  2. client performs explicit query to fetch read model
