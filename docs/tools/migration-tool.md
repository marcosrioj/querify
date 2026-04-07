# Migration Tool

## Purpose

`BaseFaq.Tools.Migration` is the tenant-aware migration runner for BaseFAQ. It exists because FAQ data is not stored in a single global database; the tool must read tenant metadata first and then apply FAQ migrations to the correct tenant databases.

## What it manages

The tool is focused on FAQ application databases. Its current CLI parsing supports `AppEnum.Faq`.

It uses the tenant database to discover which FAQ database connection strings exist for the selected app and then applies migrations across those databases.

## How it works

1. Load the solution root.
2. Read the tenant database connection string.
3. Choose an app and a command.
4. Either:
   - add a new EF Core migration, or
   - run `Database update` across all tenant FAQ databases for that app.

## Interactive usage

```bash
dotnet run --project dotnet/BaseFaq.Tools.Migration
```

The tool prompts for:

- target app
- migration command
- migration name when you choose `migrations-add`

## CLI usage

### Apply FAQ database updates

```bash
dotnet run --project dotnet/BaseFaq.Tools.Migration -- --app Faq --command database-update
```

### Add a new FAQ migration

```bash
dotnet run --project dotnet/BaseFaq.Tools.Migration -- --app Faq --command migrations-add --migration-name AddExampleChange
```

## Configuration source

The tool reads the tenant database connection through the repository configuration used by the solution, ultimately relying on the tenant-side configuration rather than hardcoding a second migration-only environment model.

Operationally, that means:

- the tenant database must already be reachable
- tenant records must contain the relevant FAQ database connection strings

## Recommended workflow

### On a fresh local environment

1. Start the base services.
2. Run the seed tool first, or manually migrate `TenantDbContext`:

```bash
dotnet ef database update \
  --project dotnet/BaseFaq.Common.EntityFramework.Tenant \
  --startup-project dotnet/BaseFaq.Tenant.BackOffice.Api
```

3. Run the migration tool with `database-update` when tenant metadata already exists.

### When introducing a schema change

1. make the EF model change in the correct persistence project
2. add the migration with `migrations-add`
3. apply the update locally
4. run the relevant integration tests

## Common failure cases

- tenant database is not reachable
- a tenant record does not have the expected FAQ database connection string
- the solution root cannot be located
- `migrations-add` is used without `--migration-name` in CLI mode

## Related documents

- [`seed-tool.md`](seed-tool.md)
- [`../devops/local-development.md`](../devops/local-development.md)
- [`../backend/dotnet-backend-overview.md`](../backend/dotnet-backend-overview.md)
