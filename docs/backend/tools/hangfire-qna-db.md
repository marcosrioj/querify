# Hangfire QnA Database

## Purpose

`Querify.QnA.Common.Persistence.HangfireQnaDb` owns the QnA worker's durable Hangfire storage boundary.

It is separate from tenant-scoped `QnADbContext` because Hangfire jobs are operational worker state, not QnA domain entities. The QnA worker still uses `QnADbContext` for tenant module data, but Hangfire job metadata lives in `HangfireQnaDb`.

Source upload verification is RabbitMQ-driven now. Hangfire remains available for future QnA
operational jobs and may be used later for a low-frequency reconciliation job that finds stuck
`Uploaded` sources, but the recurring source-upload verification sweep is no longer registered.

## Runtime configuration

The QnA worker reads the storage connection from:

```text
ConnectionStrings:HangfireQnaDb
```

The Hangfire provider options still live under:

```text
HangFire:*
```

Important settings:

- `HangFire:SchemaName`: PostgreSQL schema used by `Hangfire.PostgreSql`; local default is `hangfire`.
- `HangFire:PrepareSchemaIfNecessary`: `true` lets `Hangfire.PostgreSql` create/update its own storage schema at startup.
- `HangFire:StartupConnectionMaxRetries`, `HangFire:StartupConnectionBaseDelaySeconds`, `HangFire:StartupConnectionMaxDelaySeconds`, and `HangFire:AllowDegradedModeWithoutStorage`: provider startup resilience knobs.

## Schema ownership

Do not model Hangfire's internal tables as Querify domain entities. The table layout belongs to `Hangfire.PostgreSql`, and its version is controlled through `Querify.Common.Infrastructure.Hangfire`.

This means an EF migration generated from `HangfireQnaDbContext` can be empty. That is expected: `AggregatedCounter`, `Counter`, `Hash`, `Job`, `JobParameter`, `JobQueue`, `List`, `Schema`, `Server`, `Set`, and `State` are provider-owned storage tables, not EF Core entities in this project.

Recommended modes:

- Local/development: leave `HangFire:PrepareSchemaIfNecessary=true`.
- Release-managed production: create an EF migration in `HangfireQnaDb`, put the provider schema SQL for the pinned `Hangfire.PostgreSql` version in that migration, apply it through release automation, then run with `HangFire:PrepareSchemaIfNecessary=false`.

## EF commands

Add a migration:

```bash
dotnet ef migrations add InitialHangfireQnaDb \
  --project dotnet/Querify.QnA.Common.Persistence.HangfireQnaDb \
  --startup-project dotnet/Querify.QnA.Worker.Api \
  --context HangfireQnaDbContext
```

If this migration contains no `CreateTable(...)` calls, do not add EF entities just to make the tables appear. Use one of these paths:

- Keep `HangFire:PrepareSchemaIfNecessary=true` and let `Hangfire.PostgreSql` install/upgrade its provider schema when the worker starts.
- For production environments that require schema changes to be release-controlled, paste the exact SQL from the pinned `Hangfire.PostgreSql` install scripts into `migrationBuilder.Sql(...)`, then set `HangFire:PrepareSchemaIfNecessary=false`.

Apply migrations:

```bash
dotnet ef database update \
  --project dotnet/Querify.QnA.Common.Persistence.HangfireQnaDb \
  --startup-project dotnet/Querify.QnA.Worker.Api \
  --context HangfireQnaDbContext
```

If you already built the projects and hit a `dotnet ef` wrapper build issue, rerun with `--no-build` after confirming `dotnet build dotnet/Querify.QnA.Worker.Api -v minimal` passes.

## Validation

```bash
dotnet build dotnet/Querify.QnA.Common.Persistence.HangfireQnaDb -v minimal
dotnet build dotnet/Querify.QnA.Worker.Api -v minimal
```

Then start the QnA worker and open:

```text
http://localhost:5030/HangfireDashboard
```

After the worker initializes storage, the PostgreSQL `hangfire` schema should contain the provider tables such as `job`, `state`, `jobqueue`, `server`, `set`, `list`, `hash`, `counter`, and `aggregatedcounter`.
