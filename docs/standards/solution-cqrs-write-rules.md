# BaseFAQ CQRS Write Rules

## Purpose

This document defines the mandatory write-side rules for BaseFAQ. The goal is to keep command flows predictable, thin, and aligned across all business modules.

## Scope

These rules apply to:

- `BaseFaq.Faq.*`
- `BaseFaq.Tenant.*`
- `BaseFaq.AI.*`
- any future module using ASP.NET Core controllers plus MediatR

## Rule 1: commands return simple values

Preferred command return types:

- `Guid`
- `bool`
- `string`
- `void` / `IRequest`

Do not return:

- DTOs
- lists
- paged results
- wrapper objects that only hide a simple value

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

## Rule 4: write services stay thin

Application services may orchestrate command dispatch, but they should not rebuild read models after the command completes.

Good examples:

- `Task<Guid> Create(...)`
- `Task<Guid> Update(...)`
- `Task<bool> Delete(...)`
- `Task<Guid> RequestGeneration(...)`

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

## HTTP mapping guidance

- `POST create` -> `201` with the created `Guid`
- `PUT update` -> `200` with the updated `Guid` or a meaningful `bool`
- async request command -> `202` with a correlation `Guid`
- When a write endpoint uses an explicit action segment, the route segment should be lowercase kebab-case, for example `add-tenant-member` instead of `AddTenantMember`

## Review checklist

- command returns only a simple value
- no read-after-write response shaping
- write controller returns a write result, not a read DTO
- service write methods stay thin
- tests were updated for the real dependencies
- large handler logic was extracted where necessary

## Anti-patterns

- command handler returning `*Dto`
- command returning `List<T>`
- controller returning `CreatedAtAction(..., dto)` for a write flow that only needs an id
- services querying the database after the command only to return a richer payload
- making production dependencies nullable to satisfy tests
