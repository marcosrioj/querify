---
name: enforce-cqrs-architecture-rules
description: Encode BaseFAQ's write-side architecture rules as automated compliance checks.
category: data
priority: high
triggers:
  - architecture test
  - cqrs rules
  - forbid dto return
  - command contract check
owned_paths:
  - PROJECT_RULES.md
  - docs/standards/solution-cqrs-write-rules.md
  - dotnet/BaseFaq.Common.Architecture.Test.IntegrationTest
collaborates_with:
  - build-cqrs-feature-module
  - write-real-database-integration-tests
---

# Enforce CQRS Architecture Rules

## When to Use

- The repository introduces or changes a cross-cutting write-side rule.
- A new feature risks breaking BaseFAQ command or controller response conventions.

## Responsibilities

- Convert architecture rules into automated checks.
- Keep enforcement targeted to the right project prefixes.
- Produce actionable failure messages.

## Workflow

1. Read the authoritative rule from `PROJECT_RULES.md` and the CQRS write rules document.
2. Translate the rule into a checkable invariant.
3. Scan the relevant source files while excluding generated output.
4. Fail on prohibited return types, wrapper patterns, or other explicit anti-patterns.
5. Keep the failure output precise enough to fix quickly.

## BaseFAQ Domain Alignment

- Commands return simple values only.
- Controllers and services stay thin on the write side.
- Architecture memory belongs in automated checks, not only in code review.

## Collaborates With

- [`build-cqrs-feature-module`](../../backend/build-cqrs-feature-module/SKILL.md)
- [`write-real-database-integration-tests`](../write-real-database-integration-tests/SKILL.md)

## Done When

- The rule is enforced automatically for the intended project set.
- Failing output points to concrete offending files or patterns.
- Contributors cannot regress the write-side contract silently.
