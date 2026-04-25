# Migration Tool

## Purpose

`BaseFaq.Tools.Migration` is the tenant-aware migration runner for BaseFAQ module databases. It reads tenant metadata first, then applies supported module migrations to the correct tenant databases.

## What it manages

The tool manages supported module databases. Current supported module target: `QnA`.

It uses the tenant database to discover which module database connection strings exist and then applies migrations across those databases.

## How it works

1. Load the solution root.
2. Read the tenant database connection string.
3. Choose a command.
4. Either:
   - add a new EF Core migration, or
   - run database update across all tenant databases for the selected module.

## Interactive usage

```bash
dotnet run --project dotnet/BaseFaq.Tools.Migration
```

The tool prompts for:

- migration command
- migration name when you choose `migrations-add`

## CLI usage

### Apply QnA database updates

```bash
dotnet run --project dotnet/BaseFaq.Tools.Migration -- --module QnA --command database-update
```

### Add a new QnA migration

```bash
dotnet run --project dotnet/BaseFaq.Tools.Migration -- --module QnA --command migrations-add --migration-name AddExampleChange
```

## Configuration source

The tool reads the tenant database connection through the repository configuration used by the solution, ultimately relying on the tenant-side configuration rather than hardcoding a second migration-only environment model.

Operationally, that means:

- the tenant database must already be reachable
- tenant records must contain the relevant module database connection strings

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
- a tenant record does not have the expected module database connection string
- the solution root cannot be located
- `migrations-add` is used without `--migration-name` in CLI mode

## Related documents

- [`seed-tool.md`](seed-tool.md)
- [`local-development.md`](local-development.md)
- [`../architecture/dotnet-backend-overview.md`](../architecture/dotnet-backend-overview.md)
