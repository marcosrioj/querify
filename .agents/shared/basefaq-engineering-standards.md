# BaseFAQ Engineering Standards

## Non-Negotiable Backend Rules

- command handlers return only simple values: `Guid`, `bool`, `string`, or `void`
- no read-after-write response shaping in command flows
- controllers stay HTTP-thin
- services stay orchestration-thin
- commands and queries live in explicit ownership boundaries
- action route segments use lowercase kebab-case when named

## Backend Architecture Shape

- host projects are composition roots
- business modules are split by bounded context
- MediatR commands and queries are the standard application pattern
- public read flows resolve tenant context before touching tenant data
- AI flows are asynchronous and event-driven

## Frontend Rules

- keep transport in `api.ts`
- keep cache and mutation logic in `hooks.ts`
- use shared layouts and placeholder states
- route copy through frontend-owned localization
- preserve RTL/LTR behavior and direction-aware rendering

## Data And Testing Rules

- prefer real PostgreSQL-backed integration tests over mocked repository tests
- use seed and migration tools instead of ad hoc SQL
- treat `TenantDbContext` and `FaqDbContext` as separate ownership boundaries
- control-plane worker processors need lease-based claiming and retry handling

## Agent-System Rules

- skill selection happens before delegation
- subagents execute; they do not choose strategy
- every major change should state the selected primary skill and any supporting skills
- BaseFAQ language outranks generic platform language
