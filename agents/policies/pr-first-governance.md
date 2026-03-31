# PR-First Governance

## Core Rule

Every implementation change must be prepared for a GitHub pull request. Agents may inspect, plan, edit within approved repository scopes, and assemble evidence, but the final acceptance point is always the GitHub PR review flow.

## Where PRs Are Approved

- Code and documentation approvals: GitHub Pull Requests.
- Deployment approvals after merge: protected GitHub Environments and the Azure promotion flow.
- Emergency or production overrides: human-only, outside the agent runtime.

## Risk Levels

### Low risk

- Documentation-only updates
- Isolated UI/UX artifacts
- Localized frontend refactors with no contract changes

Approval:

- Standard reviewer for the owning domain

### Medium risk

- API implementation updates without breaking contracts
- New frontend modules consuming existing APIs
- Test, observability, or release automation improvements

Approval:

- Owning domain reviewer
- One adjacent reviewer when cross-domain behavior changes

### High risk

- Breaking OpenAPI or event contract changes
- Multitenant data model changes
- Azure, CI/CD, container, or secret-management changes
- Security-sensitive flows, authentication, authorization, or supply-chain changes

Approval:

- Owning domain reviewer
- Required domain lead or platform/security approver
- Human sign-off before merge

## Mandatory Gates

- No direct production edits.
- No secret material in prompts, files, or traces.
- No cross-tenant data leakage.
- No PR merge without explicit human review.
- No deploy to Azure production without environment approval.

## Evidence Required In Every PR Packet

- Summary of scope
- Changed paths
- Risk level
- Required approvers
- Tests executed or missing
- Rollback note
- Follow-up items or known gaps
