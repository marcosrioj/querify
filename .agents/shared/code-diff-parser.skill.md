---
name: code-diff-parser
description: Extract changed code blocks from diff-like input, identify likely languages, and normalize the review scope.
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

# Code Diff Parser Skill

## Purpose

Normalize diff-like input into file and hunk blocks so reviewers can reason about logical changes instead of raw patch text.

## When to Use

- The input may be a patch, PR diff, unified diff, or mixed review content containing changed code blocks.
- A reviewer needs to focus on changed lines plus the minimum context needed for accurate findings.

## Responsibilities

- Detect diff-like input.
- Extract changed blocks and nearby context.
- Infer likely language from filenames, syntax, and patch headers.
- Normalize formatting so reviewers inspect logical changes instead of raw patch noise.

## Inputs

- unified diffs
- `diff --git` patches
- hunk-based snippets using `@@`
- mixed text containing patch blocks

## Outputs

Return normalized diff evidence such as:

```json
{
  "input_shape": "diff | mixed",
  "files": [
    {
      "path": "string",
      "language": "string",
      "blocks": [
        {
          "header": "@@ ... @@",
          "added": ["line"],
          "removed": ["line"],
          "context": ["line"],
          "normalized_snippet": "string"
        }
      ]
    }
  ]
}
```

## Behavior

1. Confirm the input is diff-like from patch headers or hunk markers.
2. Split by file and hunk.
3. Preserve exact added and removed lines.
4. Produce a normalized snippet with enough context for review.

## Guardrails

- Do not invent surrounding code that is not present.
- Keep removed and added lines distinct.
- If the input is not a diff, say so explicitly rather than coercing it into diff output.
- Do not include business logic.

## Example Usage

```yaml
input:
  diff: |
    @@
    -const a = 1;
    +const a = userInput;
```

Expected result:

```json
{
  "input_shape": "diff",
  "files": [
    {
      "path": "unknown",
      "language": "unknown",
      "blocks": [
        {
          "header": "@@",
          "added": ["const a = userInput;"],
          "removed": ["const a = 1;"],
          "context": [],
          "normalized_snippet": "const a = userInput;"
        }
      ]
    }
  ]
}
```
