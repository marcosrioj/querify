# Seed Tool

## Purpose

`BaseFaq.Tools.Seed` prepares the local or target databases with either essential platform data or realistic sample FAQ data.

It is designed to work across both the tenant database and a FAQ database, which is why it is the standard entrypoint instead of ad hoc SQL or hand-written scripts.

## What it seeds

### Essential data

Essential seed creates or ensures:

- AI provider records
- the AI Agent user used by the AI runtime
- the seed tenant users in `TenantDb`
- the default FAQ tenant metadata in `TenantDb`
- tenant-user membership, tenant AI-provider assignments, and the FAQ tenant connection

This is the minimum required tenant-side data for AI, tenant configuration, and FAQ database routing to work correctly.

### Sample data

Sample seed creates realistic FAQ-style data in the FAQ database for the essential seed tenant.

It also seeds billing sample scenarios in `TenantDb` covering five tenants with varied states:

| Tenant | Plan | Status |
|---|---|---|
| NorthPeak Analytics | pro-monthly | Active — healthy, two paid invoices |
| Pacific Trail Studio | starter-monthly | Trialing — no payment yet, trial ends May 15 2026 |
| MapleForge Media | pro-monthly | PastDue — latest payment failed, in grace period |
| Aurora Clinic Systems | pro-yearly | Canceled — historical invoices, entitlement inactive |
| BlueHarbor Legal | business-monthly | Active — webhook and email outbox troubleshooting demo |

Billing seed also includes five `BillingWebhookInbox` rows (Completed, Completed, Failed-with-error, Pending, Completed) and three `EmailOutbox` rows (Pending, Completed, Failed-retryable).

## Configuration source

The tool reads:

- `ConnectionStrings:TenantDb`
- `ConnectionStrings:FaqDb`

from `dotnet/BaseFaq.Tools.Seed/appsettings.json`.

## Run the tool

```bash
dotnet run --project dotnet/BaseFaq.Tools.Seed
```

## Menu options

At startup the tool offers these actions:

1. seed realistic sample FAQ data
2. seed essential data
3. clean databases and seed essential plus sample data
4. clean `TenantDb` only
5. clean `FaqDb` only
0. exit

## Recommended choices

### First-time setup

Choose `3` if you want a clean environment with sample content.

### AI-only setup

Choose `2` if you want all required `TenantDb` seed data without the FAQ sample content.

### Sample-only setup

Choose `1` only when essential data already exists. On a clean environment, option `1` will stop and ask you to run the essential seed first.

### Resetting local state

Choose `4` when you want to clear only `TenantDb`.

Choose `5` when you want to clear only `FaqDb`.

## Important output: AI Agent user id

When essential seed runs, it prints the AI Agent user id. That value must be copied into:

- `dotnet/BaseFaq.AI.Api/appsettings.json`
- key: `Ai:UserId`

If the user id changes and the AI host is still pointing to the old value, AI-related flows will be misconfigured.

## Safety behavior

- the tool logs the tenant and FAQ connection info it is using
- it applies EF Core migrations before seeding
- it asks for confirmation before appending data into non-empty databases

## Recommended order of operations

1. start infrastructure
2. on a clean environment, run the seed tool or manually migrate `TenantDbContext`
3. use the migration tool later when you need to apply FAQ schema updates across tenant FAQ databases
4. run the APIs and frontend

## Related documents

- [`migration-tool.md`](migration-tool.md)
- [`../devops/local-development.md`](../devops/local-development.md)
- [`../operations/secret-manager-key-rotation.md`](../operations/secret-manager-key-rotation.md)
