# BaseFaq Multi-Agent System

## Purpose

This document applies the multi-agent operating model to the current BaseFaq repository. The goal is to give BaseFaq an internal AI team that can plan, implement, review, document, and release work with human approvals kept at the right gates.

## Why The System Lives In `agents/`

The new multi-agent runtime is intentionally isolated in `agents/` so it can evolve independently from the existing `.NET 10` AI worker runtime in `dotnet/BaseFaq.AI.*`.

This separation keeps two concerns distinct:

- `dotnet/BaseFaq.AI.*`: product-facing asynchronous AI generation and matching runtime
- `agents/`: engineering execution runtime based on OpenAI Agents SDK, handoffs, tools, and direct-implementation governance

## Current BaseFaq Mapping

### UI/UX

- Delivery root: `uiux/`
- Reference source: the Demo6 Next.js TypeScript layout in `apps/demos/.../demo6`

### Frontend / Micro-frontends

- Delivery root: `apps/`
- Baseline: `apps/demos/metronic-tailwind-react-demos/typescript/nextjs`
- Direction: API-driven micro-frontends that can consume BaseFaq backends through explicit adapters

### Backend / Microservices / APIs

- Delivery root: `dotnet/`
- Primary stack: `.NET 10`
- Existing boundaries: API hosts plus business modules, with `BaseFaq.AI.Api` already separated from FAQ and tenant APIs

### Multitenancy / Data

- Delivery roots: `dotnet/BaseFaq.Common.EntityFramework.Tenant`, `dotnet/BaseFaq.Faq.Common.Persistence.FaqDb`, migration and seed tooling
- Core rule: maintain tenant isolation and explicit connection ownership

### Platform / DevOps / SRE

- Delivery roots: `azure/`, `.github/`, `docker/`, `local/env/`
- Core rule: local and Azure capacity are managed declaratively and remain human-gated for high-risk changes

### Security / QA / Supply Chain

- Delivery roots: `docs/testing/`, test projects, CI quality gates
- Core rule: no release without explicit validation and risk review

### Docs / Release Manager

- Delivery root: `docs/`
- Core rule: publish architecture, rollout, release, and evidence artifacts in English

## Runtime Shape

### Agent Lead

- Reads the request
- Decomposes the work
- Routes to specialists through handoffs
- Consolidates changed paths, validation, blockers, and follow-up review guidance
- Prepares the final direct-implementation response

### Specialists

- Design System / UI-UX
- Frontend / Micro-frontends
- Backend / Microservices / APIs
- Multitenancy / Data
- Platform / DevOps / SRE
- Security / QA / Supply Chain
- Docs / Release Manager

## Tooling

The runtime uses local repository tools with specialist write scopes:

- file reads and repository search
- bounded shell commands
- file creation and replacement in owned scopes
- delivery summary generation under `agents/.state/`

High-risk shell actions require approval interruptions. Final code approval remains outside the runtime.

## Review And Rollout Model

### Where review happens

- High-risk review: the team's normal human-controlled merge flow
- Deployment approval: protected GitHub Environments and Azure promotion

### High-risk changes that always require human review

- breaking API or event contract changes
- multitenant persistence or migration changes
- Azure, CI/CD, container, or secret-management changes
- security-sensitive authentication, authorization, or supply-chain changes

## OpenAI Alignment

The implementation in `agents/` follows the current OpenAI direction for agentic work:

- Responses API direction: <https://platform.openai.com/docs/guides/responses-vs-chat-completions>
- Reasoning model guidance: <https://platform.openai.com/docs/guides/reasoning>
- Agents SDK TypeScript/JavaScript reference: <https://openai.github.io/openai-agents-js/>

## Operational Rule

The agents are allowed to behave like a team, but not like an ungoverned autonomous system. BaseFaq remains human-led at the merge and deployment boundaries.
