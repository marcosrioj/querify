# BaseFAQ FAQ Solution Q&A Upgrade Plan

## Purpose

This document lists the solution-wide work required to move the current FAQ-oriented solution into the broader question-and-answer model described in the repositioning report.

The sample domain in [README.md](./README.md) is the architectural target for language and behavior. The rest of the solution still uses legacy naming and legacy assumptions.

## Upgrade areas

### 1. Domain and persistence

Targets:

- current FAQ persistence model
- migrations
- seed data

Required updates:

- keep `BaseEntity`, `AuditableEntity`, and `IMustHaveTenant` as the shared persistence contract instead of replacing them with a second base hierarchy
- map the responsibility expressed by `DomainEntity` onto those existing abstractions and the current `BaseDbContext` pipeline
- map legacy collection entities to the new concept of spaces
- map legacy item entities to question threads
- support accepted answer semantics instead of first-answer semantics
- support answer variants by context
- introduce explicit provenance at question and answer level
- introduce append-only workflow history
- reject cross-tenant attachments on the write side instead of relying only on query filters
- keep migrations additive to avoid rewriting every tenant database at once

### 2. Shared contracts

Targets:

- `dotnet/BaseFaq.Models.Faq`

Required updates:

- rename transport concepts over time from FAQ wording to question-and-answer wording
- expose question status, moderation, visibility, channel, context, duplicate, and accepted-answer fields
- expose answer kind, answer status, context applicability, evidence summary, and trust fields
- add contracts for source links and thread activity
- keep compatibility shims while old APIs still exist

### 3. Portal business layer

Targets:

- FAQ portal business projects

Required updates:

- replace FAQ-first workflows with question-thread workflows
- add question review, answer review, accept answer, validate answer, mark duplicate, and escalate flows
- update projections and ranking to prefer accepted or validated answers
- write thread activity on every relevant workflow transition
- validate cross-tenant relationships before persistence when commands attach questions, answers, sources, or activity records
- support directional voting if community ranking is required

### 4. Public business layer and APIs

Targets:

- FAQ public business projects
- FAQ portal/public APIs

Required updates:

- expose public question pages by stable key
- expose accepted answer and alternative answers
- expose source-backed trust data where appropriate
- allow incoming question submission through public surfaces with moderation gates
- filter by visibility and lifecycle state
- keep public source citation and excerpt behavior fail-closed unless the backing source explicitly allows it
- keep backward compatibility for consumers that still expect legacy FAQ payloads

### 5. Portal frontend

Targets:

- `apps/portal`

Required updates:

- redesign collection management into space management
- redesign item management into question-thread management
- add moderation queues
- add accepted-answer controls
- add duplicate handling
- add evidence panels
- add activity timeline views
- add contextual answer variant management

### 6. Public UI and embed surfaces

Targets:

- current public rendering logic
- widget and embed flows
- future integrations

Required updates:

- suggest similar questions before creating a new one
- route canonical question pages by stable keys
- show accepted answer first
- show confidence, freshness, and evidence where appropriate
- support both curated-list and per-question rendering modes

### 7. Search and structured data

Targets:

- integrations architecture
- any renderer that emits structured data

Required updates:

- move from list-only structured data assumptions to mixed curated-list plus question-page behavior
- define ownership rules for page-level search markup
- support individual question pages as first-class SEO objects
- suppress duplicate markup across embeds and integrations

### 8. AI generation and matching

Targets:

- AI generation
- AI matching
- AI shared contracts

Required updates:

- generate answer drafts instead of static content blocks
- attach evidence and confidence to every draft
- cluster duplicate questions
- propose accepted-answer candidates, not only generated text
- route AI output through moderation before it becomes official
- prevent AI draft output from becoming public or official until the expected review and validation path has completed

### 9. Analytics and reporting

Targets:

- dashboard queries
- telemetry
- future reporting

Required updates:

- track unanswered questions
- track accepted-answer rate
- track validation rate
- track duplicate rate
- track stale answers
- track moderation backlog
- track source coverage and AI draft approval rate

### 10. Integrations and connectors

Targets:

- help center integrations
- support integrations
- engineering/community integrations
- social ingestion

Required updates:

- normalize external questions into the new question-thread model
- preserve source identity and references
- push canonical answers back to external systems where useful
- map external status and moderation signals into internal workflow events

### 11. Security and governance

Targets:

- role model
- moderation rules
- audit access

Required updates:

- keep tenant query filters and tenant indexes from `BaseDbContext`
- define write-side guards for cross-tenant relationships instead of depending only on read-side filters
- define who can publish official answers
- define who can accept answers
- define who can validate answers
- define how AI drafts are labeled and approved
- define which source kinds may be cited publicly, which may be excerpted publicly, and which remain internal-only by default
- define validation rules for flexible metadata such as source metadata JSON, snapshots, and external locators
- define retention policy for thread activity and evidence history
- define append-only handling for thread activity so normal workflows never edit or soft-delete journal entries

### 12. Tooling

Targets:

- migration tool
- seed tool
- local development docs

Required updates:

- apply new migrations safely across tenant databases
- seed realistic spaces, questions, answers, sources, and activity history
- document rollout order and rollback expectations

### 13. Tests

Targets:

- portal integration tests
- public integration tests
- architecture and tooling tests

Required updates:

- cover accepted-answer selection
- cover moderation workflows
- cover duplicate handling
- cover visibility filtering
- cover source-link persistence
- cover public question-page behavior
- cover cross-tenant attachment rejection on writes
- cover public citation and excerpt restrictions for internal or unverified sources
- cover append-only handling of thread activity
- cover compatibility with old FAQ-oriented flows during transition

## Recommended order

1. shared contracts and projections
2. portal write-side workflows
3. public read-side behavior
4. portal UI
5. AI and connector alignment
6. analytics and governance hardening
7. seed and test expansion

## Immediate next milestone

The cleanest next move is:

1. align shared DTOs with the new question-and-answer language
2. keep the existing persistence base abstractions and define the Q&A-specific write-side security rules around them
3. change public projections to use accepted-answer semantics
4. add moderation and provenance fields to portal workflows
