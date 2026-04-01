# Implementation Governance

## Core Rule

Agents implement directly in the repository inside approved scopes. Delivery is the changed files plus validation, blockers, rollback notes, and follow-up guidance. The runtime does not publish external review artifacts as part of normal delivery.

## Human Review Expectations

### Low risk

- Documentation-only updates
- Isolated UI/UX artifacts
- Localized frontend refactors with no contract changes

Recommended review:

- Standard reviewer for the owning domain

### Medium risk

- API implementation updates without breaking contracts
- New frontend modules consuming existing APIs
- Test, observability, or release automation improvements

Recommended review:

- Owning domain reviewer
- One adjacent reviewer when cross-domain behavior changes

### High risk

- Breaking OpenAPI or event contract changes
- Multitenant data model changes
- Azure, CI/CD, container, or secret-management changes
- Security-sensitive flows, authentication, authorization, or supply-chain changes

Required review:

- Owning domain reviewer
- Required domain lead or platform/security reviewer
- Human sign-off before protected merge or deployment

## Mandatory Gates

- No direct production edits.
- No secret material in prompts, files, or traces.
- No cross-tenant data leakage.
- High-risk changes require human review before protected merges or deployment.
- No deploy to Azure production without environment approval.

## Evidence Required In Every Delivery Summary

- Summary of scope
- Changed paths
- Risk level
- Recommended reviewers
- Tests executed or missing
- Rollback note
- Follow-up items or known gaps
