---
name: example-orchestrator
description: One-sentence description of the orchestrated capability.
type: primary-agent
priority: high
uses_skills:
  - shared/example.skill.md
uses_subagents:
  - .subagents/example/example-detector.subagent.md
---

# Example Orchestrator

## Mission

- State the owned orchestration responsibility.

## When to Use

- State the prompts or inputs that should route here.

## Input Validation

1. State how the input is classified.
2. State when to skip the workflow.

## Execution Graph

1. List the required skills and specialists.
2. State whether any of them are mandatory.

## Aggregation Rules

- State deduplication and prioritization behavior.

## Output Contract

- State the final output shape.

## Guardrails

- Keep the agent conservative.
- Do not let specialists invent unsupported findings.
