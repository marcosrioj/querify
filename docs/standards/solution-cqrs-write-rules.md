# BaseFAQ CQRS Write Rules

## Purpose

This document defines the mandatory write-side rules for BaseFAQ. The goal is to keep command flows predictable, thin, and aligned across all business modules.

## Scope

These rules apply to:

- `BaseFaq.Faq.*`
- `BaseFaq.QnA.*`
- `BaseFaq.Tenant.*`
- any future module using ASP.NET Core controllers plus MediatR

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

## Rule 8: QnA uses FAQ-style feature projects

When the backend work belongs to QnA:

- do not aggregate Portal behavior into `BaseFaq.QnA.Portal.Business`
- do not aggregate Public behavior into `BaseFaq.QnA.Public.Business`
- create or extend the smallest entity or surface project that owns the use case
- compose QnA APIs from multiple feature registrations such as `AddQuestionBusiness()` and `AddVoteBusiness()`
- point QnA integration tests at the owning feature projects rather than a monolithic business assembly
- keep source files physically inside the owning QnA feature project directory
- do not use linked source entries such as `<Compile Include="..\\..." Link="...">` in QnA business feature projects
- mirror FAQ-style feature test folders and file names, for example `Tests/Question/QuestionCommandQueryTests.cs` or `Tests/Question/QuestionQueryTests.cs`
- keep use-case logic in the QnA command and query handlers instead of generic helper classes
- do not introduce `*Operations.cs`, `PagedQuery.cs`, `QnAProjectionMapper.cs`, `QnAActivityMetadata.cs`, or `SignalRequestContext.cs` in QnA business projects
- the only allowed QnA helper exception is a feature-specific request-context helper with FAQ-style naming such as `FeedbackRequestContext.cs` or `VoteRequestContext.cs`

## Rule 9: BaseFaq.Models.QnA mirrors BaseFaq.Models.Faq DTO layout

When the work belongs to `BaseFaq.Models.QnA`:

- mirror the FAQ DTO folder pattern such as `Dtos/Question/QuestionDto.cs`
- keep DTOs in real feature folders rather than aggregate `*Dtos.cs` files
- do not place catch-all DTO files directly under `dotnet/BaseFaq.Models.QnA/Dtos`
- do not create pseudo-entity folders such as `dotnet/BaseFaq.Models.QnA/Dtos/Link`
- place link DTOs under the owning feature folders like `Dtos/Answer`, `Dtos/Question`, and `Dtos/Space`
- keep write-side handler request DTOs flat
- do not inherit one write-side `*RequestDto` from another `*RequestDto`
- let paged or sorted query request DTOs inherit the shared pagination base used by the project pattern
- declare request DTO properties explicitly on each write-side QnA request type

## Rule 10: QnA persistence entities stay anemic

When the work belongs to `BaseFaq.QnA.Common.Persistence.QnADb/Entities`:

- keep entities as state-only persistence models
- do not add behavior methods, factory methods, or transition methods
- do not add convenience projection properties such as computed tag or source collections
- do not use `[NotMapped]` computed properties to hide query shaping inside entities
- keep relation creation, validation, status transitions, and DTO shaping inside commands, queries, and feature-local private methods

## HTTP mapping guidance

- `POST create` -> `201` with the created `Guid`
- `PUT update` -> `200` with the updated `Guid` or a meaningful `bool`
- async request command -> `202` with a correlation `Guid`
- When a write endpoint uses an explicit action segment, the route segment should be lowercase kebab-case, for example `add-tenant-member` instead of `AddTenantMember`

## Review checklist

- command returns only a simple value
- command handler returns only a simple value
- no read-after-write response shaping
- write controller returns a write result, not a read DTO
- service write methods stay thin
- tests were updated for the real dependencies
- large handler logic was extracted where necessary
- QnA persistence entities stayed state-only

## Anti-patterns

- command handler returning `*Dto`
- command handler implementing `IRequestHandler<TCommand, TComplex>`
- command returning `List<T>`
- controller returning `CreatedAtAction(..., dto)` for a write flow that only needs an id
- services querying the database after the command only to return a richer payload
- making production dependencies nullable to satisfy tests
- introducing monolithic QnA business projects instead of FAQ-style feature modules
- using linked source files in QnA feature projects instead of real files in the owning project folder
- using aggregate `*Dtos.cs` files in `BaseFaq.Models.QnA` instead of FAQ-style DTO folders and files
- keeping generic QnA helper files instead of placing logic in command/query handlers
- introducing a pseudo-entity DTO folder such as `BaseFaq.Models.QnA/Dtos/Link`
- adding behavior or computed projection properties to QnA persistence entities
