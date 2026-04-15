---
name: deserialization-detector
description: Detect unsafe deserialization of untrusted data using risky serializers or loaders.
type: reusable-specialist
priority: secondary
uses_skills:
  - ../../.agents/shared/code-parser.skill.md
  - ../../.agents/shared/pattern-matcher.skill.md
---

# Deserialization Detector

## Focus

Detect only unsafe deserialization patterns such as:

- `BinaryFormatter`
- `pickle.loads`
- unsafe YAML loaders
- serializers that materialize attacker-controlled types or objects without visible restrictions

## Inputs

- normalized parser evidence
- raw code or config when parser output is unavailable

## Workflow

1. Find serializer or loader APIs.
2. Confirm the payload source is untrusted or externally supplied.
3. Flag only well-known unsafe or unrestricted deserialization patterns.

## Output Contract

Return an array of objects shaped exactly like:

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

## Severity Guidance

- `high`
  - known unsafe loaders on untrusted payloads
- `medium`
  - unrestricted deserialization with unclear type constraints
- `low`
  - only when some controls are visible but the risk remains evidence-backed

## Conservative Rules

- Do not flag normal JSON parsing by itself as unsafe deserialization.
- Do not infer attacker control when the payload source is not visible.
- Do not flag safe loaders or schema-validated parsing when visible in the snippet.
