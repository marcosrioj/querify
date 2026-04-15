---
name: example-skill
description: One-sentence description of the owned BaseFAQ outcome.
type: repository-skill
scope: basefaq-repository
category: backend
priority: high
triggers:
  - key phrase
owned_paths:
  - path/or/project/*
collaborates_with:
  - another-skill
---

# Example Skill

## When to Use

- State the user intents that should trigger this skill.

## Responsibilities

- State the owned decisions or implementation outcomes.
- Keep responsibilities aligned to one BaseFAQ boundary.

## Workflow

1. State the default execution sequence.
2. Keep steps explicit and repository-specific.

## BaseFAQ Domain Alignment

- Explain the domain language, runtime surface, or ownership boundary this skill must preserve.

## Collaborates With

- Link the minimum supporting skills that are commonly paired with this one.

## Done When

- State the observable outcomes that prove the skill was applied correctly.

## Agent-System Update Notes

- State which `.agents/` files must also be reviewed when this skill is created or updated.
- At minimum, consider `skills/README.md`, `patterns/intent-routing.md`, and `patterns/orchestration-playbooks.md`.
