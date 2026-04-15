---
name: code-parser
description: Parse and normalize code-like, config-like, or text-like input into evidence that other agents can analyze conservatively.
type: shared-skill
scope: cross-domain
priority: high
consumers:
  - security-orchestrator.agent.md
  - .subagents/security/injection-detector.subagent.md
  - .subagents/security/xss-detector.subagent.md
  - .subagents/security/deserialization-detector.subagent.md
  - .subagents/security/secrets-detector.subagent.md
---

# Code Parser Skill

## When to Use

- The input may contain source code, config, shell commands, templates, or mixed text with embedded code.
- Another agent needs structured evidence instead of raw text scanning.

## Responsibilities

- Classify input as `code`, `config`, `text`, or `mixed`.
- Infer likely language or format conservatively.
- Split input into analyzable regions such as executable statements, string literals, HTML sinks, file-path operations, query builders, config keys, and credential-like values.

## Inputs

- raw code snippets
- config files such as `.env`, YAML, JSON, XML, `appsettings.json`
- text with embedded code or configuration fragments

## Output Contract

Return normalized evidence blocks such as:

```json
{
  "input_type": "code | config | text | mixed",
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

## Workflow

1. Detect whether the input is relevant to static security analysis.
2. Infer language or format only from explicit syntax, filenames, or well-known config structure.
3. Extract only evidence-backed regions.
4. Preserve exact snippets so downstream detectors can cite real input.

## Guardrails

- Do not invent AST structure that is not visible in the input.
- Prefer `mixed` over an overconfident language guess.
- If the input is not code/config-like, say so explicitly.
- Do not include business logic or BaseFAQ-specific assumptions.
