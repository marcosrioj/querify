# QnA Domain Boundary

This document records where QnA entity state and entity-related business rules live.

## Ownership

- `dotnet/Querify.QnA.Common.Domain` owns QnA domain state and domain rules shared by Portal, Public, seed, tests, and persistence.
- `Querify.QnA.Common.Domain.Entities` contains the anemic QnA entity model: `Space`, `Question`, `Answer`, `Source`, `Tag`, links, and `Activity`.
- `Querify.QnA.Common.Domain.BusinessRules` contains reusable entity-related rules that do not require persistence or response shaping.
- `Querify.QnA.Common.Domain.BusinessRules.Activities` owns activity append, actor/request value creation, entity snapshots, activity context metadata, signal entries, and signal score calculations.
- `Querify.QnA.Common.Domain.BusinessRules.Answers` owns answer status validation/transitions, visibility constraints, archive/activate semantics, and answer-source link creation rules.
- `Querify.QnA.Common.Domain.BusinessRules.Questions` owns question status validation, visibility constraints, accepted-answer rules, and question tag/source link creation rules.
- `Querify.QnA.Common.Domain.BusinessRules.Sources` owns source checksum rules, public source constraints, and public reference compatibility rules.
- `Querify.QnA.Common.Domain.BusinessRules.Spaces` owns space visibility constraints, question/answer acceptance gates, tag/source link creation rules, and slug generation rules. Slug uniqueness checks stay outside the domain when they query `QnADbContext`.
- `dotnet/Querify.QnA.Common.Persistence.QnADb` owns EF Core infrastructure only: `QnADbContext`, model configurations, tenant-integrity rules, migrations, and service registration.

Do not add new QnA entities under `Querify.QnA.Common.Persistence.QnADb/Entities`. Do not add reusable QnA domain rules under `Querify.QnA.Common.Persistence.QnADb/Activities` or a helper project.

## Migration Process

Use this process when moving existing QnA entity-adjacent behavior:

1. Follow [`../../behavior-change-playbook.md`](../../behavior-change-playbook.md) Step 1 before editing. Inventory entity state, DTOs, handlers, seed code, tests, and Portal/Public usage.
2. Put persisted QnA state in `Querify.QnA.Common.Domain.Entities`. Keep entities state-only: no factories, transition methods, computed projection helpers, or persistence-aware methods.
3. Put reusable entity business rules in `Querify.QnA.Common.Domain.BusinessRules.<Feature>`. Keep these rules free of `QnADbContext`, EF queries, service registration, controllers, and response shaping. Request/session adapters may read HTTP abstractions only to construct domain activity values.
4. Keep EF configuration, indexes, relationships, tenant-integrity checks, and migrations in `Querify.QnA.Common.Persistence.QnADb`.
5. Update callers to reference `Querify.QnA.Common.Domain.Entities` and the relevant `Querify.QnA.Common.Domain.BusinessRules` namespace.
6. Do not edit historical EF migrations or generate a new migration unless the user explicitly asks for migration work.
7. Verify the stage with targeted builds for `Querify.QnA.Common.Domain`, `Querify.QnA.Common.Persistence.QnADb`, affected QnA business projects, seed/test projects when touched, and then broaden validation as needed.

## Activity Rules

Activity helpers and signal calculations are domain rules because they operate on QnA entity state and QnA enum semantics. They must remain infrastructure-free:

- `ActivityAppender` creates `Activity` entities and attaches them to the tracked aggregate collection. It must not take `QnADbContext` or call `DbSet.Add`.
- `ActivityActorResolver` may read request/session abstractions needed to build a domain actor value, but it must not query persistence.
- `ActivitySignals`, identity resolution, metadata shaping, and status-to-activity mapping stay under `BusinessRules.Activities`.

Persistence decides how those attached entities are saved through normal EF change tracking.

## Business Rule Audit Checklist

Use this checklist when reviewing whether behavior belongs under `BusinessRules`:

- Status and visibility invariants for `Space`, `Question`, `Answer`, and `Source` belong in the matching `BusinessRules.<Feature>` folder.
- Cross-entity entity rules such as accepted-answer eligibility, public reference compatibility, and idempotent tag/source link creation belong in domain rules when they operate only on already-loaded entities.
- Activity snapshot/context metadata and signal conversion belong in `BusinessRules.Activities` because the values are tied to QnA entity state and activity semantics.
- DTO projection, filtering, sorting, and response-specific read shaping stay in query handlers.
- EF uniqueness checks, tenant-integrity checks, model configuration, migrations, and `QnADbContext` queries stay in persistence or command orchestration, not in domain rules.
- Seed files may call domain rules, but seed-specific catalog construction and raw bulk SQL remain seed-tool behavior.
