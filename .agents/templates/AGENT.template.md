---
name: example-orchestrator
description: One-sentence description of the orchestrated capability.
type: primary-agent
priority: high
uses_skills:
  - shared/example.skill.md
uses_subagents:
  - .agents/subagents/example/example-detector.subagent.md
---

# Example Orchestrator

## Purpose

- State the owned orchestration responsibility.

## Inputs

- State the accepted input shape.

## Outputs

- State the returned decision or result package.

## Behavior

1. State the default orchestration sequence.
2. State how skills and specialists are composed.

## Example Usage

- Give one short invocation or routing example.

## When to Use

- State the prompts or inputs that should route here.

## Guardrails

- Keep the agent conservative.
- Do not let specialists invent unsupported findings.
