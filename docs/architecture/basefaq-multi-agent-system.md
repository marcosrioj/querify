# BaseFAQ Multi-Agent System

## Purpose

This document explains the engineering-side multi-agent runtime that lives under `agents/`. It is not the product AI runtime. It is the repository automation layer used to plan, implement, document, and review engineering work.

## Why it is separate from `dotnet/BaseFaq.AI.*`

The repository contains two different AI concerns:

- `dotnet/BaseFaq.AI.*`: product-facing generation and matching workers used by BaseFAQ itself
- `agents/`: engineering execution helpers used to work on the repository

Keeping them separate avoids mixing product runtime requirements with developer automation concerns.

## Scope in this repository

The multi-agent system is designed around the same repository boundaries as the rest of BaseFAQ:

- `apps/` for frontend work
- `dotnet/` for APIs, business modules, and persistence
- `docker/`, `local/`, `azure/`, and `.github/` for platform and delivery
- `docs/` for architecture, operations, and release material

## Operating model

### Lead agent

The lead agent is expected to:

- read the request
- decompose the work
- route tasks to specialists when appropriate
- consolidate validation, blockers, and final changes

### Specialist roles

The specialist split mirrors the repository:

- frontend and design-system work
- backend and API work
- multitenancy and data work
- DevOps and release work
- QA and documentation work

## Guardrails

The multi-agent runtime is intentionally governed.

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
