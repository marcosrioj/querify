---
name: audit
description: Record immutable privacy-processing traces, decision basis, and execution evidence for compliance and review.
type: shared-skill
scope: privacy-domain
priority: high
consumers:
  - privacy/privacy-orchestrator.agent.md
  - privacy/privacy-engine.subagent.md
  - privacy/gdpr.subagent.md
  - privacy/lgpd.subagent.md
  - privacy/ccpa.subagent.md
  - privacy/pipl.subagent.md
---

# Audit Skill

## Purpose

Provide logging and traceability for every privacy workflow. This skill creates a structured audit payload that captures who acted, what decision was made, which law was invoked, which skills ran, and what evidence supported the result.

This skill is reusable and law-agnostic.

## Inputs

- `trace_id` when already present
- `actor`
- `request_summary`
- `applicable_laws`
- `decision`
- `executed_skills`
- `evidence_refs`
- `timestamp`
- `retention_policy`

## Outputs

- `audit_trace_id`
- `audit_event`
- `follow_up_controls`

Example contract:

```json
{
  "audit_trace_id": "privacy-2026-04-15-001",
  "audit_event": {
    "decision": "approve",
    "laws": ["GDPR"],
    "skills": ["dsar", "audit"]
  },
  "follow_up_controls": ["store event in immutable log"]
}
```

## Behavior

1. Normalize the privacy event into a stable structure.
2. Capture request, law, decision, and evidence references.
3. Link the event to every skill and system touched by the workflow.
4. Return the event and trace id for external persistence.
5. Flag missing evidence or missing timestamps as audit defects.

## Example Usage

```yaml
actor: privacy-orchestrator
request_summary:
  type: delete
  subject_id: usr-77
applicable_laws: [GDPR]
decision: partial
executed_skills: [dsar, audit]
evidence_refs: [req-77, id-proof-77, legal-hold-4]
timestamp: 2026-04-15T20:04:00Z
```

Expected result:

```json
{
  "audit_trace_id": "privacy-2026-04-15-001",
  "follow_up_controls": [
    "persist immutable audit event",
    "attach evidence references to case record"
  ]
}
```
