---
name: code-parser
description: Parse and normalize code-like, diff-like, config-like, or text-like input into evidence that other agents can analyze conservatively.
type: shared-skill
scope: cross-domain
priority: high
consumers:
  - code-review-orchestrator.agent.md
  - security-orchestrator.agent.md
  - .agents/subagents/code-review/readability-reviewer.subagent.md
  - .agents/subagents/code-review/architecture-reviewer.subagent.md
  - .agents/subagents/code-review/performance-reviewer.subagent.md
  - .agents/subagents/code-review/best-practices-reviewer.subagent.md
  - .agents/subagents/security/injection-detector.subagent.md
  - .agents/subagents/security/xss-detector.subagent.md
  - .agents/subagents/security/deserialization-detector.subagent.md
  - .agents/subagents/security/secrets-detector.subagent.md
---

# Code Parser Skill

## Purpose

Normalize code-like, diff-like, config-like, or mixed input into evidence blocks that downstream agents can analyze conservatively.

## When to Use

- The input may contain source code, diffs, config, shell commands, templates, or mixed text with embedded code.
- Another agent needs structured evidence instead of raw text scanning.

## Responsibilities

- Classify input as `code`, `diff`, `config`, `text`, or `mixed`.
- Infer likely language or format conservatively.
- Split input into analyzable regions such as executable statements, string literals, HTML sinks, file-path operations, query builders, config keys, and credential-like values.

## Inputs

- raw code snippets
- config files such as `.env`, YAML, JSON, XML, `appsettings.json`
- text with embedded code or configuration fragments

## Outputs

Return normalized evidence blocks such as:

```json
{
  "input_type": "code | diff | config | text | mixed",
  "languages": ["ts", "js", "csharp", "yaml", "json"],
  "regions": [
    {
      "kind": "call | string | template | config-key | html-sink | sql-fragment | path-operation | serializer | secret-like-literal",
      "evidence": "exact snippet",
      "reason": "why this region matters"
    }
  ]
}
```

## Behavior

1. Detect whether the input is relevant to static code analysis, code review, or security analysis.
2. Infer language or format only from explicit syntax, filenames, or well-known config structure.
3. Extract only evidence-backed regions.
4. Preserve exact snippets so downstream detectors can cite real input.

## Guardrails

- Do not invent AST structure that is not visible in the input.
- Prefer `mixed` over an overconfident language guess.
- If the input is not code/config-like, say so explicitly.
- Do not include business logic or BaseFAQ-specific assumptions.

## Example Usage

```yaml
input:
  snippet: "const sql = \"SELECT * FROM users WHERE email = '\" + req.body.email + \"'\";"
```

Expected result:

```json
{
  "input_type": "code",
  "languages": ["js"],
  "regions": [
    {
      "kind": "sql-fragment",
      "evidence": "const sql = \"SELECT * FROM users WHERE email = '\" + req.body.email + \"'\";",
      "reason": "dynamic SQL string construction is visible"
    }
  ]
}
```
