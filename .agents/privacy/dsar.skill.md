---
name: dsar
description: Handle data subject rights requests through intake validation, identity checks, fulfillment planning, and response packaging.
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

# DSAR Skill

## Purpose

Execute the operational steps of a data subject access request or similar privacy-right request. This skill validates the request, verifies authority, builds the fulfillment plan, and returns the response package.

This skill does not decide which law applies. It executes the obligations passed in by the calling subagent.

## Inputs

- `request_type`
- `identity_evidence`
- `authorized_agent`
- `jurisdiction_obligations`
- `systems`
- `data_categories`
- `exception_flags`
- `deadline`

## Outputs

- `request_status`
- `verification_result`
- `fulfillment_plan`
- `response_package`
- `exception_notes`

Example contract:

```json
{
  "request_status": "validated",
  "verification_result": "verified",
  "fulfillment_plan": [
    "collect records from CRM and billing",
    "apply legal-hold exception to invoices"
  ],
  "response_package": {
    "request_type": "delete",
    "status": "partial"
  },
  "exception_notes": ["invoice retention"]
}
```

## Behavior

1. Validate the request shape and the requester authority.
2. Verify identity using the evidence standard passed in by the caller.
3. Build a system-by-system fulfillment plan.
4. Apply the exceptions or carve-outs supplied by the law-specific subagent.
5. Package the result for user response and downstream execution.
6. Return a status that can be audited and surfaced by the orchestrator.

## Example Usage

```yaml
request_type: delete
identity_evidence: verified
jurisdiction_obligations:
  law: GDPR
  recipient_notice_required: true
systems: [crm, billing, support]
exception_flags: [legal_claims_hold]
deadline: 1 month
```

Expected result:

```json
{
  "request_status": "validated",
  "verification_result": "verified",
  "fulfillment_plan": [
    "delete CRM profile",
    "retain billing invoices under legal hold",
    "notify recipients where erasure applies"
  ],
  "response_package": {
    "status": "partial",
    "deadline": "1 month"
  }
}
```
