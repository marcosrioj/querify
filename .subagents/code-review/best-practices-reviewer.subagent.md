---
name: best-practices-reviewer
description: Review code for missing error handling, bad async usage, improper typing, and framework misuse.
type: reusable-specialist
priority: secondary
uses_skills:
  - ../../.agents/shared/code-parser.skill.md
  - ../../.agents/shared/code-diff-parser.skill.md
  - ../../.agents/shared/complexity-analyzer.skill.md
---

# Best Practices Reviewer

## Focus

Review only best-practices concerns:

- missing error handling
- bad async usage
- improper typing
- framework misuse

## Workflow

1. Inspect the visible asynchronous and typed boundaries.
2. Flag missing error handling when the code performs I/O or external operations without visible failure handling.
3. Flag framework misuse only when the framework-specific risk is directly visible.

## Output Contract

Return an array of objects shaped exactly like:

```json
[
  {
    "category": "best-practices",
    "severity": "low | medium | high",
    "issue": "string",
    "code": "snippet",
    "suggestion": "string"
  }
]
```

## Conservative Rules

- Do not flag missing try/catch around code that has no visible failure path.
- Do not demand stronger typing than the visible language or framework supports.
- Avoid generic "use best practices" comments; every issue must be specific and actionable.
