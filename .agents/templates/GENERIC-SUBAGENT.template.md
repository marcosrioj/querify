---
name: example-detector
description: One-sentence description of the narrow reusable specialist concern.
type: reusable-specialist
priority: secondary
uses_skills:
  - ../../.agents/shared/example.skill.md
---

# Example Generic Subagent

## Focus

- State exactly one narrow concern.

## Inputs

- State the accepted input types.

## Workflow

1. State how evidence is matched.
2. State when to suppress a finding.

## Output Contract

```json
[
  {
    "type": "string",
    "severity": "low | medium | high",
    "code": "snippet",
    "explanation": "string",
    "fix": "string"
  }
]
```

## Conservative Rules

- Prefer evidence-backed findings only.
- Suppress speculative results.
