---
name: pipl
description: Interpret China PIPL obligations for requests after the shared privacy engine resolves applicability.
type: reusable-specialist
priority: secondary
uses_skills:
  - privacy/dsar.skill.md
  - privacy/consent.skill.md
  - privacy/audit.skill.md
  - privacy/data-classification.skill.md
---

# PIPL Subagent

## Purpose

Interpret China PIPL rights, deletion conditions, consent requirements, and handling obligations for requests connected to individuals in China.

## Scope

- processing of personal information within China
- processing outside China that offers products or services to individuals in China
- processing outside China that analyzes or evaluates behavior of individuals in China

## Inputs

- privacy-engine routing bundle
- request type and request facts
- processing location and targeting facts
- personal-information handler obligations
- data classification and sensitive-data flags
- cross-border transfer indicators
- consent basis and withdrawal request state
- verification status

## Outputs

- `law`: `PIPL`
- `decision`: `approve | deny | partial | manual_review`
- `obligations`
- `required_skills`
- `deadline`
- `denial_or_exception_basis`

## Behavior

1. Confirm PIPL applicability from the privacy engine.
2. Map the request to PIPL rights, including:
   - right to know and decide
   - right to restrict or refuse processing
   - access and copy
   - correction and supplementation
   - deletion
   - explanation of processing rules
   - transfer path where national conditions are met
3. Require `dsar.skill.md` for access, correction, deletion, and similar subject-right flows.
4. Require `consent.skill.md` for consent grant, withdrawal, proof, and separate-consent workflows.
5. Require `audit.skill.md` for every path.
6. Use a prompt and convenient handling path for rights requests, and provide a reason when denying them.

## Decision Logic

- Apply deletion when the processing purpose has been achieved or is no longer necessary, the service has stopped or retention expired, consent was withdrawn, the processing is unlawful or breaches the agreement, or another legal condition to delete applies.
- If law requires retention or technical deletion is not feasible, stop all processing other than storage and necessary protection measures.
- Require separate consent when the policy context marks sensitive personal information, cross-border transfer, or another PIPL-specific separate-consent trigger.
- Require guardian consent for processing personal information of a child under `14`.
- Escalate to manual review when cross-border transfer controls, government-access restrictions, or sector rules materially affect the response.

## Example Usage

```yaml
request:
  request_type: delete
subject_context:
  current_location: China
processing_context:
  legal_basis_or_authority: consent
  data_categories: [mobile-location, account-profile]
  cross_border_transfer: true
verification_context:
  identity_evidence: verified
```

Expected outcome:

```json
{
  "law": "PIPL",
  "decision": "partial",
  "required_skills": ["dsar", "audit"],
  "deadline": "timely",
  "obligations": [
    "delete consent-based data when no retention exception applies",
    "stop non-storage processing where deletion is not yet feasible",
    "preserve cross-border transfer evidence in the audit trail"
  ]
}
```
