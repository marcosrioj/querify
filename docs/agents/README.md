# Agent Ecosystem Guide

This folder documents how the repository agent ecosystem is structured, how to choose the right artifact type, and how to use each orchestrator, subagent, skill, and worker consistently.

## Taxonomy

The ecosystem is standardized into five artifact families:

1. Orchestrators
   - Path: `.agents/*.agent.md` and `.agents/privacy/*.agent.md`
   - Purpose: high-level coordination, routing, aggregation, and final response composition.
2. Markdown subagents
   - Path: `.agents/privacy/*.subagent.md` and `.subagents/**/*.subagent.md`
   - Purpose: narrow decision logic, jurisdiction interpretation, or generic analysis.
3. Flat operational skills
   - Path: `.agents/shared/*.skill.md` and `.agents/privacy/*.skill.md`
   - Purpose: reusable atomic actions such as parsing, pattern matching, consent handling, DSAR handling, audit, and data classification.
4. Repository-owned skills
   - Path: `.agents/skills/<category>/<skill-name>/SKILL.md`
   - Purpose: reusable BaseFAQ capability guides tied to one repository boundary.
5. Execution workers
   - Path: `.agents/subagents/*-worker.toml`
   - Purpose: bounded implementation or review workers used only after the parent agent has already framed strategy.

## Naming Standards

- Orchestrators use `kebab-case-orchestrator.agent.md`.
- Markdown subagents use `kebab-case.subagent.md`.
- Flat operational skills use `kebab-case.skill.md`.
- Repository-owned skills use `skills/<category>/<skill-name>/SKILL.md`.
- Execution workers use `kebab-case-worker.toml`.

Metadata rules:

- `name` must match the artifact identifier exactly.
- Orchestrators use `type: primary-agent`.
- Markdown specialists use `type: reusable-specialist`.
- Flat operational skills use `type: shared-skill`.
- Repository-owned skills use `type: repository-skill`.
- Workers use `role = "execution-worker"`.

## Standard Shapes

### Orchestrators

Every orchestrator should expose:

- `Purpose`
- `Inputs`
- `Outputs`
- `Behavior`
- `Example Usage`

Optional sections such as `When to Use`, `Decision Logic`, `Interaction Model`, `Guardrails`, or validation details may be added when they materially help operators.

### Markdown Subagents

Every markdown subagent should expose:

- `Purpose`
- `Inputs`
- `Outputs`
- `Behavior`
- `Example Usage`

Optional sections such as `Scope`, `Decision Logic`, `Severity Guidance`, or `Guardrails` may be added for specialist behavior.

### Flat Operational Skills

Every flat skill should expose:

- `Purpose`
- `Inputs`
- `Outputs`
- `Behavior`
- `Example Usage`

These skills should remain stateless where practical and should return an operational package rather than making orchestration decisions.

### Repository-Owned Skills

Repository-owned skills keep the existing BaseFAQ implementation-guide shape:

- `When to Use`
- `Responsibilities`
- `Workflow`
- `BaseFAQ Domain Alignment`
- `Collaborates With`
- `Done When`

## How To Use The Ecosystem

1. Start with the smallest correct entry point.
   - Use an orchestrator when the task spans multiple specialists or skills.
   - Use a repository-owned skill when the user intent maps directly to one BaseFAQ boundary.
2. Add flat skills only for reusable atomic actions.
3. Call markdown subagents only after the task is framed.
4. Use workers only when the parent agent has already chosen strategy and the owned-path boundary is clear.
5. Update the catalog and the relevant `.agents/` indexes when a reusable capability, naming rule, or boundary changes.

## Catalog

Use [`catalog.md`](catalog.md) for the full list of artifacts and when to use each one.
