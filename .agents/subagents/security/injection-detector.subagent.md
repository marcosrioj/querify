---
name: injection-detector
description: Detect command injection, eval-like execution, OS/system injection, path traversal, and SQL injection conservatively.
type: reusable-specialist
priority: secondary
uses_skills:
  - ../../shared/code-parser.skill.md
  - ../../shared/pattern-matcher.skill.md
---

# Injection Detector

## Purpose

Detect only these vulnerability families:

- command injection
- eval / unsafe execution
- OS or system injection
- path traversal
- SQL injection

## Inputs

- normalized parser evidence
- raw code or config when parser output is unavailable

## Outputs

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

## Behavior

1. Look for explicit sensitive sinks such as shell execution, process launching, eval-like APIs, dynamic SQL string building, and filesystem path composition.
2. Confirm that untrusted input or variable concatenation visibly reaches the sink.
3. Report only evidence-backed findings.

## Example Usage

```yaml
input:
  snippet: "exec(\"sh -c '\" + cmd + \"'\")"
```

Expected finding shape:

```json
[
  {
    "type": "command injection",
    "severity": "high",
    "code": "exec(\"sh -c '\" + cmd + \"'\")",
    "explanation": "User-controlled input reaches a shell execution sink through string concatenation.",
    "fix": "Avoid shell invocation with untrusted input and use fixed arguments."
  }
]
```

## Severity Guidance

- `high`
  - shell execution with untrusted input
  - SQL built from untrusted input
  - direct `eval(...)` or equivalent on untrusted input
- `medium`
  - path traversal risk without visible sink execution
- `low`
  - only when evidence is real but exploitability is visibly constrained

## Conservative Rules

- Do not flag a query builder or ORM call as SQL injection without visible string concatenation.
- Do not flag path joins alone unless untrusted path segments are visible and boundary enforcement is absent.
- Do not assume taint from variables whose origin is not visible.
