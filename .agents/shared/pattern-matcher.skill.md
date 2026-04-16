---
name: pattern-matcher
description: Match conservative, evidence-backed risk patterns over normalized input without adding business interpretation.
type: shared-skill
scope: cross-domain
priority: high
consumers:
  - security-orchestrator.agent.md
  - .agents/subagents/security/injection-detector.subagent.md
  - .agents/subagents/security/xss-detector.subagent.md
  - .agents/subagents/security/deserialization-detector.subagent.md
  - .agents/subagents/security/secrets-detector.subagent.md
---

# Pattern Matcher Skill

## Purpose

Match explicit risky patterns over normalized evidence and return only conservative candidates that a specialist can verify.

## When to Use

- A detector needs conservative matching over executable sinks, serializers, secrets, or output rendering paths.

## Responsibilities

- Match explicit, evidence-backed patterns.
- Return candidates with rationale and confidence hints.
- Stay generic and reusable across languages and frameworks.

## Inputs

- normalized parser output from `code-parser.skill.md`
- raw snippets when parser output is unavailable
- optional detector-specific pattern families

## Outputs

Return candidates such as:

```json
{
  "matches": [
    {
      "family": "command-exec | sql-string-build | html-output | unsafe-deserializer | hardcoded-secret",
      "evidence": "exact snippet",
      "confidence": "low | medium | high",
      "reason": "why this may indicate a real risk"
    }
  ]
}
```

## Behavior

1. Match exact APIs, concatenation patterns, unsafe sinks, or secret-like literals.
2. Raise confidence only when the evidence ties untrusted input to a sensitive sink.
3. Return fewer matches rather than speculative ones.

## Guardrails

- Do not claim a vulnerability solely from a risky API name without visible unsafe usage.
- Do not infer sanitization or validation that is not visible.
- Prefer false negatives over unsupported false positives.
- Do not include BaseFAQ-specific business decisions.

## Example Usage

```yaml
input:
  snippet: "exec(\"sh -c '\" + cmd + \"'\")"
  family: command-exec
```

Expected result:

```json
{
  "matches": [
    {
      "family": "command-exec",
      "evidence": "exec(\"sh -c '\" + cmd + \"'\")",
      "confidence": "high",
      "reason": "string concatenation reaches a shell execution sink"
    }
  ]
}
```
