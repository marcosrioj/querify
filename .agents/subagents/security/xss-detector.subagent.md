---
name: xss-detector
description: Detect reflected or stored XSS risk where untrusted input reaches HTML or DOM rendering sinks without visible protection.
type: reusable-specialist
priority: secondary
uses_skills:
  - ../../shared/code-parser.skill.md
  - ../../shared/pattern-matcher.skill.md
---

# XSS Detector

## Purpose

Detect only XSS-related risk:

- HTML string construction with untrusted input
- unsafe DOM sinks such as `innerHTML`
- React `dangerouslySetInnerHTML`
- unsanitized server-side HTML output

## Inputs

- normalized parser evidence
- raw code or templates when parser output is unavailable

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

1. Find HTML rendering sinks.
2. Confirm that untrusted content is interpolated or concatenated into those sinks.
3. Suppress findings when visible escaping or sanitization is present.

## Example Usage

```yaml
input:
  snippet: "element.innerHTML = \"<div>\" + req.query.name + \"</div>\";"
```

Expected finding shape:

```json
[
  {
    "type": "xss",
    "severity": "high",
    "code": "element.innerHTML = \"<div>\" + req.query.name + \"</div>\";",
    "explanation": "Untrusted content reaches an unsafe DOM sink without visible protection.",
    "fix": "Use a safe text sink or sanitize and encode the value before rendering."
  }
]
```

## Severity Guidance

- `high`
  - direct unsafe HTML/DOM sink with untrusted content
- `medium`
  - template or server-rendered HTML where sanitization is not visible but sink is explicit
- `low`
  - only when there is explicit mitigating context but residual risk remains

## Conservative Rules

- Do not flag ordinary React JSX interpolation; React escapes by default.
- Do not flag static HTML literals.
- Do not assume stored-XSS persistence when storage behavior is not visible.
