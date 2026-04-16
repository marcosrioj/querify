---
name: example-detector
description: One-sentence description of the narrow reusable specialist concern.
type: reusable-specialist
priority: secondary
uses_skills:
  - ../../shared/example.skill.md
---

# Example Generic Subagent

## Purpose

- State the narrow reusable concern.

## Inputs

- State the accepted input types.

## Outputs

- State the returned decision or finding shape.

## Behavior

1. State how evidence is matched.
2. State when to suppress a finding.

## Example Usage

- Give one short example of the specialist in use.

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

## Guardrails

- Prefer evidence-backed findings only.
- Suppress speculative results.
