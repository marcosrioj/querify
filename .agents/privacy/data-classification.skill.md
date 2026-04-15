---
name: data-classification
description: Classify personal data, sensitivity, transfer risk, and rights impact so privacy agents can make consistent decisions.
type: shared-skill
scope: privacy-domain
priority: high
consumers:
  - privacy/privacy-orchestrator.agent.md
  - privacy/privacy-engine.subagent.md
  - privacy/gdpr.subagent.md
  - privacy/lgpd.subagent.md
  - privacy/ccpa.subagent.md
  - privacy/pipl.subagent.md
---

# Data Classification Skill

## Purpose

Classify data into a reusable privacy taxonomy. This skill identifies whether the data is personal, whether it is sensitive, and which downstream privacy controls are likely required.

This skill is intentionally law-agnostic. It returns normalized labels for the law-specific subagents to interpret.

## Inputs

- `fields`
- `sample_values` when available
- `source_system`
- `processing_purpose`
- `retention_context`
- `transfer_context`

## Outputs

- `personal_data_labels`
- `sensitivity_labels`
- `special_handling_flags`
- `recommended_controls`

Example contract:

```json
{
  "personal_data_labels": ["email", "device-id", "location"],
  "sensitivity_labels": ["precise-location"],
  "special_handling_flags": ["cross-border-transfer", "consent-review"],
  "recommended_controls": ["minimize", "review sensitive-data basis"]
}
```

## Behavior

1. Determine whether each field is personal data, pseudonymous data, anonymized data, or non-personal data.
2. Detect high-risk categories such as:
   - child data
   - biometric data
   - health data
   - financial-account data
   - precise location
   - government identifiers
3. Return normalized labels without deciding the law.
4. Flag likely downstream needs:
   - special consent review
   - deletion carve-out review
   - cross-border-transfer review
   - sensitive-personal-information limitation review

## Example Usage

```yaml
fields:
  - full_name
  - email
  - gps_coordinates
  - card_last4
source_system: mobile-app
processing_purpose: behavioral-analytics
transfer_context:
  cross_border: true
```

Expected result:

```json
{
  "personal_data_labels": [
    "name",
    "email",
    "precise-location",
    "payment-fragment"
  ],
  "sensitivity_labels": ["precise-location"],
  "special_handling_flags": ["cross-border-transfer", "consent-review"],
  "recommended_controls": [
    "minimize collection",
    "review sensitive-data rules before processing"
  ]
}
```
