---
name: complexity-analyzer
description: Detect long methods, deep nesting, and high cognitive complexity from visible code structure.
type: shared-skill
scope: cross-domain
priority: high
consumers:
  - code-review-orchestrator.agent.md
  - .subagents/code-review/readability-reviewer.subagent.md
  - .subagents/code-review/architecture-reviewer.subagent.md
  - .subagents/code-review/performance-reviewer.subagent.md
  - .subagents/code-review/best-practices-reviewer.subagent.md
---

# Complexity Analyzer Skill

## Purpose

Return conservative structural complexity signals from visible code so reviewers can reason about maintainability and likely hot spots.

## When to Use

- A review task needs structural signals about code difficulty, maintainability, or likely error-proneness.

## Responsibilities

- Detect long methods or functions.
- Detect deep nesting and loop stacking.
- Estimate visible cognitive complexity conservatively.

## Inputs

- code snippets
- normalized diff blocks
- parser output from `code-parser.skill.md` or `code-diff-parser.skill.md`

## Outputs

Return structural observations such as:

```json
{
  "functions": [
    {
      "name": "string",
      "snippet": "string",
      "signals": ["long-method", "deep-nesting", "high-cognitive-complexity"],
      "metrics": {
        "line_count": 0,
        "max_nesting_depth": 0,
        "loop_depth": 0
      },
      "reason": "string"
    }
  ]
}
```

## Behavior

1. Identify visible function, method, or block boundaries.
2. Count structural signals only when they are explicit in the input.
3. Prefer simple, explainable heuristics over pseudo-precise scoring.

## Guardrails

- Do not compute exact complexity from incomplete snippets.
- Do not flag small functions simply for containing one conditional or one loop.
- Use conservative thresholds and explain why a block appears complex.

## Example Usage

```yaml
input:
  snippet: |
    function f(ids) {
      for (const a of ids) {
        for (const b of ids) {
          for (const c of ids) {
            work(a, b, c);
          }
        }
      }
    }
```

Expected result:

```json
{
  "functions": [
    {
      "name": "f",
      "signals": ["deep-nesting", "high-cognitive-complexity"],
      "metrics": {
        "line_count": 7,
        "max_nesting_depth": 3,
        "loop_depth": 3
      }
    }
  ]
}
```
