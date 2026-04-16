# Agent Ecosystem Guide

This folder documents how the repository agent ecosystem is structured, how to choose the right artifact type, and how to use each orchestrator, subagent, skill, and worker consistently.

## Taxonomy

The ecosystem is standardized into five artifact families:

1. Orchestrators
   - Path: `.agents/*.agent.md` and `.agents/privacy/*.agent.md`
   - Purpose: high-level coordination, routing, aggregation, and final response composition.
2. Markdown subagents
   - Path: `.agents/privacy/*.subagent.md` and `.agents/subagents/**/*.subagent.md`
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

## How Users Ask For Work

Users normally ask in natural language in the chat. They do not need to know the agent names in advance.

The usual flow is:

1. The user describes the goal.
2. The routing rules decide which orchestrator or skill should own the task.
3. That orchestrator calls the required subagents and skills.
4. If the work is large, the parent agent may delegate execution to a worker.

### What A Good Request Looks Like

A good request usually contains:

- the goal
- the context
- the input
- any constraints

Simple template:

```text
I want [goal].
Context: [system, law, screen, service, or domain].
Input: [code, diff, scenario, file, or error].
Constraints: [optional restrictions].
```

### Examples

Security review:

```text
Analyze this code for XSS, SQL injection, and hardcoded secrets.
```

Code review:

```text
Review this diff and focus on architecture, performance, and maintainability risks.
```

Privacy workflow:

```text
An EU user asked to delete all personal data. Tell me which flow applies and what steps are required.
```

Backend feature:

```text
Create a CQRS endpoint to list FAQs by tenant and add the right integration tests.
```

Frontend feature:

```text
Add loading, empty, and error states to this Portal page, and keep the existing layout pattern.
```

Architecture or strategy:

```text
Explain whether this should use GDPR, LGPD, or CCPA rules based on the scenario below.
```

### When To Name An Agent Explicitly

Most of the time, users should ask for the outcome, not the agent.

Explicit agent naming is useful when the user wants to force a specific path, for example:

```text
Use the privacy-orchestrator for this case.
```

```text
Run the security-orchestrator on this snippet.
```

```text
Use dsar.skill for this request flow.
```

### Tips For New Users

- Ask for the result you want, not the implementation path.
- Include the relevant code, diff, file, or business scenario when possible.
- Mention the boundary when you know it: Portal, backend, AI workflow, privacy, data, or integrations.
- State restrictions early, such as "docs only", "no code changes", or "minimal fix".

### Weak Vs Strong Requests

Weak request:

```text
Check this.
```

Strong request:

```text
Review this diff for security and performance problems. Do not suggest style-only changes.
```

## Catalog

Use [`catalog.md`](catalog.md) for the full list of artifacts and when to use each one.
