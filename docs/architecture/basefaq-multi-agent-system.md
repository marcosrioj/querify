# BaseFAQ Multi-Agent System

## Purpose

This document explains the intended role of the `agents/` area in the repository and clarifies its current state.

## Why it is separate from `dotnet/BaseFaq.AI.*`

The repository contains two different AI concerns:

- `dotnet/BaseFaq.AI.*`: product-facing generation and matching workers used by BaseFAQ itself
- `agents/`: a reserved space for engineering-side automation and related documentation

Keeping them separate avoids mixing product runtime requirements with developer automation concerns.

## Current state

The `agents/` folder is documentation-only today. It does not contain a runnable multi-agent implementation yet.

What exists now:

- [`agents/README.md`](../../agents/README.md)

What does not exist yet:

- a checked-in agent runtime
- specialist agent source files
- repository-local orchestration code under `agents/`

## Scope in this repository

If the engineering-side automation model is implemented in the future, it should align with the same repository boundaries as the rest of BaseFAQ:

- `apps/` for frontend work
- `dotnet/` for APIs, business modules, and persistence
- `docker/`, `local/`, `azure/`, and `.github/` for platform and delivery
- `docs/` for architecture, operations, and release material

## Operating model

### Lead agent

If implemented, the lead agent should:

- read the request
- decompose the work
- route tasks to specialists when appropriate
- consolidate validation, blockers, and final changes

### Specialist roles

If implemented, the specialist split should mirror the repository:

- frontend and design-system work
- backend and API work
- multitenancy and data work
- DevOps and release work
- QA and documentation work

## Guardrails

Any future multi-agent runtime should be intentionally governed.

### Human review is still required for high-risk changes

Examples:

- breaking API or event contract changes
- multitenant persistence and migration changes
- Azure, CI/CD, Docker, or secret-management changes
- security-sensitive auth or authorization changes

### Repository rules still apply

- code changes must remain inside the repository boundaries
- operational scripts and deployment flows remain explicit and reviewable
- documentation must stay in English and be committed like any other code artifact

## Relation to the rest of the documentation

Use this document only when the `agents/` directory or the engineering automation model is part of the discussion. For product architecture, start with [`solution-architecture.md`](solution-architecture.md) instead.
