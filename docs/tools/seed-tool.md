# Seed Tool

## Purpose

`BaseFaq.Tools.Seed` prepares the local or target databases with either essential platform data or realistic sample FAQ data.

It is designed to work across both the tenant database and a FAQ database, which is why it is the standard entrypoint instead of ad hoc SQL or hand-written scripts.

## What it seeds

### Essential data

Essential seed creates or ensures:

- AI provider records
- the AI Agent user used by the AI runtime

This is the minimum required data for AI-related flows to work correctly.

### Sample data

Sample seed creates realistic FAQ-style data in the FAQ database plus the tenant-side data needed to relate that FAQ content to a tenant.

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
4. clean databases only
0. exit

## Recommended choices

### First-time setup

Choose `3` if you want a clean environment with sample content.

### AI-only setup

Choose `2` if you only need AI provider metadata and the AI Agent user.

### Resetting local state

Choose `4` when you want to clear the databases without immediately recreating data.

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
2. run the migration tool
3. run the seed tool
4. run the APIs and frontend

## Related documents

- [`migration-tool.md`](migration-tool.md)
- [`../devops/local-development.md`](../devops/local-development.md)
- [`../operations/secret-manager-key-rotation.md`](../operations/secret-manager-key-rotation.md)
