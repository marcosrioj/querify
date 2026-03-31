# Data And Tooling Policy

## Allowed Prompt Material

- Repository code that is already present in this workspace
- Architecture notes, runbooks, ADRs, and public documentation
- Sanitized logs and traces with secrets removed
- Task statements, acceptance criteria, and issue context

## Prohibited Prompt Material

- Secrets, API keys, certificates, private keys, or tokens
- Raw tenant data or personally identifiable information
- Production-only incident data unless it is sanitized and approved
- Any content that would violate contractual isolation between tenants

## Tooling Rules

- Read operations are allowed inside the repository.
- Write operations must stay within the assigned specialist scope.
- Production, cloud, and destructive operations stay human-gated.
- Delivery must remain PR-first even when the runtime can edit files locally.

## Trace Handling

- Runtime traces are local to `agents/.state/`.
- Trace files are ignored by git.
- Trace files are for debugging and audit only; do not treat them as release artifacts.
