# QnA Domain Boundary

This document records where QnA entity state and entity-related business rules live.

## Ownership

- `dotnet/BaseFaq.QnA.Common.Domain` owns QnA domain state and domain rules shared by Portal, Public, seed, tests, and persistence.
- `BaseFaq.QnA.Common.Domain.Entities` contains the anemic QnA entity model: `Space`, `Question`, `Answer`, `Source`, `Tag`, links, and `Activity`.
- `BaseFaq.QnA.Common.Domain.BusinessRules` contains reusable entity-related rules that do not require infrastructure. Activity rules live under `BaseFaq.QnA.Common.Domain.BusinessRules.Activities`.
- Source checksum rules live under `BaseFaq.QnA.Common.Domain.BusinessRules.Sources`.
- Space slug generation rules live under `BaseFaq.QnA.Common.Domain.BusinessRules.Spaces`; slug uniqueness checks stay outside the domain when they query `QnADbContext`.
- `dotnet/BaseFaq.QnA.Common.Persistence.QnADb` owns EF Core infrastructure only: `QnADbContext`, model configurations, tenant-integrity rules, migrations, and service registration.

Do not add new QnA entities under `BaseFaq.QnA.Common.Persistence.QnADb/Entities`. Do not add reusable QnA domain rules under `BaseFaq.QnA.Common.Persistence.QnADb/Activities` or a helper project.

## Migration Process

Use this process when moving existing QnA entity-adjacent behavior:

1. Follow [`../../behavior-change-playbook.md`](../../behavior-change-playbook.md) Step 1 before editing. Inventory entity state, DTOs, handlers, seed code, tests, and Portal/Public usage.
2. Put persisted QnA state in `BaseFaq.QnA.Common.Domain.Entities`. Keep entities state-only: no factories, transition methods, computed projection helpers, or persistence-aware methods.
3. Put reusable entity business rules in `BaseFaq.QnA.Common.Domain.BusinessRules.<Feature>`. Keep these rules free of `QnADbContext`, EF queries, service registration, controllers, and response shaping. Request/session adapters may read HTTP abstractions only to construct domain activity values.
4. Keep EF configuration, indexes, relationships, tenant-integrity checks, and migrations in `BaseFaq.QnA.Common.Persistence.QnADb`.
5. Update callers to reference `BaseFaq.QnA.Common.Domain.Entities` and the relevant `BaseFaq.QnA.Common.Domain.BusinessRules` namespace.
6. Do not edit historical EF migrations or generate a new migration unless the user explicitly asks for migration work.
7. Verify the stage with targeted builds for `BaseFaq.QnA.Common.Domain`, `BaseFaq.QnA.Common.Persistence.QnADb`, affected QnA business projects, seed/test projects when touched, and then broaden validation as needed.

## Activity Rules

Activity helpers and signal calculations are domain rules because they operate on QnA entity state and QnA enum semantics. They must remain infrastructure-free:

- `ActivityAppender` creates `Activity` entities and attaches them to the tracked aggregate collection. It must not take `QnADbContext` or call `DbSet.Add`.
- `ActivityActorResolver` may read request/session abstractions needed to build a domain actor value, but it must not query persistence.
- `ActivitySignals`, identity resolution, metadata shaping, and status-to-activity mapping stay under `BusinessRules.Activities`.

Persistence decides how those attached entities are saved through normal EF change tracking.
