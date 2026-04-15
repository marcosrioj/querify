---
name: consent
description: Manage consent capture, refresh, withdrawal, proof, and separate-consent evidence as a reusable privacy skill.
type: shared-skill
scope: privacy-domain
priority: high
consumers:
  - privacy/privacy-orchestrator.agent.md
  - privacy/gdpr.subagent.md
  - privacy/lgpd.subagent.md
  - privacy/ccpa.subagent.md
  - privacy/pipl.subagent.md
---

# Consent Skill

## Purpose

Execute the consent lifecycle as an atomic capability. This skill captures, refreshes, withdraws, and proves consent based on policy instructions provided by the orchestrator or a law-specific subagent.

This skill is stateless by design. It returns a consent action package and evidence payload for another system to persist.

## Inputs

- `subject_id`
- `purposes`
- `requested_action`: `collect | refresh | withdraw | prove`
- `jurisdiction_requirements`
- `current_consent_record`
- `channel`
- `age_or_guardian_context`
- `separate_consent_required`

## Outputs

- `consent_status`
- `required_notice_elements`
- `evidence_payload`
- `next_steps`

Example contract:

```json
{
  "consent_status": "withdrawn",
  "required_notice_elements": ["purpose", "withdrawal method"],
  "evidence_payload": {
    "subject_id": "usr-1",
    "timestamp": "2026-04-15T20:00:00Z",
    "channel": "web",
    "purposes": ["marketing"]
  },
  "next_steps": ["propagate withdrawal to downstream systems"]
}
```

## Behavior

1. Validate whether consent is required for the passed policy.
2. Build the correct consent action:
   - `collect`: capture informed affirmative choice
   - `refresh`: re-seek permission when scope changed or proof is stale
   - `withdraw`: make withdrawal as easy as grant
   - `prove`: return evidence of who consented, to what, when, and how
3. Enforce higher-friction rules only when the caller passes them in:
   - separate consent
   - guardian consent
   - written consent
4. Return a machine-readable evidence package and required downstream tasks.

## Example Usage

```yaml
subject_id: usr-77
requested_action: withdraw
purposes: [marketing]
jurisdiction_requirements:
  law: GDPR
  same_ease_of_withdrawal: true
channel: account-settings
```

Expected result:

```json
{
  "consent_status": "withdrawn",
  "next_steps": [
    "disable marketing outreach",
    "log withdrawal proof"
  ]
}
```
