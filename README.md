# BaseFAQ Documentation

BaseFAQ is a multi-tenant FAQ platform with APIs for tenant administration, authenticated portal workflows, and public FAQ access. It uses shared infrastructure components and local Docker services to run the full stack in development.

## Documentation purpose
This document is the operational playbook for local development, integration validation, and troubleshooting.

## Business and technical context
- Business objective: support tenant administration, authenticated FAQ management, and public FAQ consumption in one platform.
- Technical objective: run all APIs and dependencies with production-like infrastructure locally using Docker and .NET tooling.

## Step-by-step execution path
1. Restore and build the solution.
2. Start base services (databases, queues, cache, telemetry, monitoring).
3. Apply EF Core migrations.
4. Seed required and sample data.
5. Run APIs locally or via Docker.
6. Validate telemetry, auth integration, and tests.
7. Use troubleshooting and shutdown runbooks as needed.

## Solution inventory (validated from `BaseFaq.sln`)
### API hosts
- `BaseFaq.Faq.Portal.Api`
- `BaseFaq.Faq.Public.Api`
- `BaseFaq.Tenant.BackOffice.Api`
- `BaseFaq.Tenant.Portal.Api`
- `BaseFaq.AI.Api`

### Business modules
- FAQ Portal: `Faq`, `FaqItem`, `Tag`, `Vote`, `ContentRef`
- FAQ Public: `Faq`, `FaqItem`, `Vote`
- Tenant Back Office: `Tenant`, `User`
- Tenant Portal: `Tenant`, `User`
- AI: Generation and Matching orchestration + worker projects

### Core shared components
- Persistence contexts: `TenantDb`, `FaqDb`, `AiDb`
- Shared infrastructure libraries: Swagger/OpenAPI, Sentry, MediatR logging, API error handling, MVC, MassTransit, telemetry
- Model libraries: `BaseFaq.Models.Ai`, `BaseFaq.Models.Common`, `BaseFaq.Models.Faq`, `BaseFaq.Models.Tenant`, `BaseFaq.Models.User`
- Tooling apps: `BaseFaq.Tools.Seed`, `BaseFaq.Tools.Migration`

## Documentation catalog
- `README.md`: end-to-end local setup, run, and troubleshooting playbook.
- `docs/testing/integration-testing-strategy.md`: integration testing scope, risk matrix, and execution model.
- `docs/architecture/basefaq-ai-generation-matching-architecture.md`: AI architecture, delivery plan, and operational controls.
- `docs/operations/secret-manager-key-rotation.md`: AI provider key-management and rotation runbook.

## Platform scope
- FAQ Portal API
- FAQ AI API
- Tenant Back Office API
- Tenant Portal API
- Shared infrastructure libraries (Swagger/OpenAPI, Sentry, MediatR logging, API error handling)
- AI persistence library (`AiDbContext` / `bf_ai_db`)
- Tooling apps (`BaseFaq.Tools.Seed`, `BaseFaq.Tools.Migration`)
- Base services via Docker (PostgreSQL, RabbitMQ, Redis, SMTP4Dev, Jaeger, Prometheus, Alertmanager, Grafana)

## Prerequisites
- Docker Engine + Docker Compose v2
- .NET SDK `10.0.100` (see `global.json`)
- Optional: `dotnet-ef` tool if you want to apply migrations manually
- Helper scripts default `REDIS_PASSWORD` to `RedisTempPassword`
- If you run Docker Compose manually, export/set `REDIS_PASSWORD` first
  (must match `Redis:Password` in `appsettings.json`)

## Quick start (clean machine)

```bash
dotnet restore BaseFaq.sln
./docker-base.sh
dotnet run --project dotnet/BaseFaq.Tools.Seed
dotnet run --project dotnet/BaseFaq.Faq.Portal.Api
```

For Windows PowerShell, use `.\docker-base.ps1`.

## Step 0) Restore and build

```bash
dotnet restore BaseFaq.sln
dotnet build BaseFaq.sln
```

## Step 1) Start base services (PostgreSQL, RabbitMQ, Redis, SMTP, Jaeger/OTEL, monitoring stack)
From the repo root:

macOS / Linux:

```bash
./docker-base.sh
```

Windows (PowerShell):

```powershell
.\docker-base.ps1
```

Notes:
- The script only stops/removes containers in the `bf_baseservices` compose project.
- It starts the base services using `docker/docker-compose.baseservices.yml` and creates:
  `bf_tenant_db`, `bf_faq_db_01`, `bf_faq_db_02`, and `bf_ai_db`.
- It also starts Jaeger with OTLP enabled (`http://localhost:16686`, OTLP gRPC `localhost:4317`).
- It starts Prometheus (`http://localhost:9090`), Alertmanager (`http://localhost:9093`), and Grafana (`http://localhost:3000`).
- Grafana default development credentials: `admin` / `admin`.
- Grafana auto-provisions the `BaseFaq RabbitMQ Overview` dashboard and Prometheus datasource.
- PostgreSQL password is `Pass123$` (the compose file uses `$$` to escape `$`).
- If PowerShell blocks script execution, run:
  `Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser`

If you prefer to run Docker Compose manually:

```bash
export REDIS_PASSWORD=RedisTempPassword
docker compose -p bf_baseservices -f docker/docker-compose.baseservices.yml up -d --wait
```

## Step 2) Apply EF Core migrations
If the tables are not created yet, install EF tooling:

```bash
dotnet tool install --global dotnet-ef
```

Tenant DB (stores tenant records and each tenant's connection string):

```bash
dotnet ef database update \
  --project dotnet/BaseFaq.Common.EntityFramework.Tenant \
  --startup-project dotnet/BaseFaq.Tenant.BackOffice.Api
```

Connection strings live in:
- `dotnet/BaseFaq.Tenant.BackOffice.Api/appsettings.json`

App DBs (run per tenant):

Use the migrations console app (`BaseFaq.Tools.Migration`) and follow the prompts:

```bash
dotnet run --project dotnet/BaseFaq.Tools.Migration
```

Notes:
- The console app asks for the target `AppEnum` and whether to run `Migrations add` or `Database update`.
- `Database update` applies migrations for **all** tenant connection strings in `Tenant.ConnectionString` filtered by the chosen app.
- It reads the tenant DB connection string from `dotnet/BaseFaq.Tenant.BackOffice.Api/appsettings.json`
  (`ConnectionStrings:TenantDb`).
- When creating a new migration, make sure the current tenant connection is properly added.
- Migrations run against all existing tenants for the selected app.

AI DB (independent, non-tenant):

```bash
dotnet ef database update \
  --project dotnet/BaseFaq.AI.Common.Persistence.AiDb \
  --startup-project dotnet/BaseFaq.AI.Api
```

Notes:
- `AiDbContext` is separated from `FaqDbContext` and uses `ConnectionStrings:AiDb`.
- The connection string is configured in `dotnet/BaseFaq.AI.Api/appsettings.json`.
- Configure `Ai:UserId` in the AI API appsettings file.
- Use the id generated by the seed action `Seed essential data (AI providers + IA Agent user)`.

## Step 2.1) Seed data
The seed app (`BaseFaq.Tools.Seed`) supports two seed types:
- Dummy seed: inserts realistic sample data into tenant + FAQ databases.
- Essential seed: ensures AI providers and the IA Agent user exist for AI API usage.

It reads connection strings from:
- `dotnet/BaseFaq.Tools.Seed/appsettings.json`

Run the seed:

```bash
dotnet run --project dotnet/BaseFaq.Tools.Seed
```

Notes:
- The seed logs which `TenantDb` and `FaqDb` connections it uses from `appsettings.json`.
- On startup it offers actions for dummy seed, essential seed, clean+dummy seed, or clean-only.
- If you run dummy seed only, essential data must already exist.
- Essential seed creates/updates this user:
  `GivenName: IA Agent`, `Role: Member`, `Email: iaagent@basefaq.com`, `ExternalId: iaagent@basefaq.com`.
- Essential seed also creates the required `AiProviders` entries used by generation and matching.
- Essential seed prints the IA Agent `UserId`; copy this value to `Ai:UserId` in:
  `dotnet/BaseFaq.AI.Api/appsettings.json`.
- If the target database already has data, it will ask whether to append.
- Dummy seed creates dozens of records per entity, including child entities (items, tags, votes, etc.).

## Step 2.2) Hostname strategy that works on host + Docker
If you want a single hostname that works both in Rider (host machine) and inside Docker,
use `host.docker.internal`.

Linux needs two small steps (Windows/macOS already provide this name):

1) Add the host alias for Docker containers (already included in `docker/docker-compose.yml`):

```yaml
extra_hosts:
  - "host.docker.internal:host-gateway"
```

2) Map the name on your host machine (Linux only):

```bash
echo "127.0.0.1 host.docker.internal" | sudo tee -a /etc/hosts
```

Then you can use this in tenant connection strings:

```
Host=host.docker.internal;Port=5432;Database=bf_faq_db_01;Username=postgres;Password=Pass123$;
```

## Step 3) Run APIs locally
FAQ Portal API:

```bash
dotnet run --project dotnet/BaseFaq.Faq.Portal.Api
```

Endpoints:
- HTTP: `http://localhost:5010`
- HTTPS: `https://localhost:5011`

Swagger / OpenAPI (FAQ app, Development only):
- Swagger UI: `/swagger`
- Swagger JSON: `/swagger/v1/swagger.json`
- OpenAPI JSON (minimal API): `/openapi/v1.json`

Tenant Back Office API:

```bash
dotnet run --project dotnet/BaseFaq.Tenant.BackOffice.Api
```

Endpoints:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`

Tenant Portal API:

```bash
dotnet run --project dotnet/BaseFaq.Tenant.Portal.Api
```

Endpoints:
- HTTP: `http://localhost:5002`
- HTTPS: `https://localhost:5003`

FAQ Public API:

```bash
dotnet run --project dotnet/BaseFaq.Faq.Public.Api
```

Endpoints:
- HTTP: `http://localhost:5020`
- HTTPS: `https://localhost:5021`

FAQ AI API:

```bash
dotnet run --project dotnet/BaseFaq.AI.Api
```

Endpoints:
- HTTP: `http://localhost:5030`
- HTTPS: `https://localhost:5031`

## Step 4) (Optional) Run APIs in Docker
APIs (Docker):

```bash
docker compose -p bf_services -f docker/docker-compose.yml up -d --build
```

This compose file:
- Runs these services:
  - `basefaq.faq.portal.api`
  - `basefaq.tenant.backoffice.api`
  - `basefaq.tenant.portal.api`
  - `basefaq.faq.public.api`
  - `basefaq.ai.api`
- Wires the service to the `bf-network` network created by the base services.
- Uses the repo root as the build context, so run the command from the repo root.

If you run APIs in Docker, this repo defaults to `host.docker.internal` in `appsettings.json` so the same values work for host + Docker.

## Step 5) Telemetry (OpenTelemetry)
- Shared baseline lives in `dotnet/BaseFaq.Common.Infrasctructure.Telemetry`.
- Use `AddTelemetry(...)` from `BaseFaq.Common.Infrasctructure.Telemetry.Extensions` in API startup.
- Keep this common project generic; app-specific `ActivitySource` names must be passed via the
  `additionalActivitySources` parameter.
- OTLP endpoint is read from app config:
  - `OpenTelemetry:Otlp:Endpoint`
  - Current default (AI API): `http://host.docker.internal:4317`
- Local trace UI is available in Jaeger:
  - `http://localhost:16686`

You can also use the helper script:

macOS / Linux:

```bash
./docker.sh
```

Windows (PowerShell):

```powershell
.\docker.ps1
```

Note: the script removes the BaseFaq Docker images and prunes dangling Docker images after it brings the stack up.

## Service ports and endpoints
- PostgreSQL: `localhost:5432` (databases `bf_tenant_db`, `bf_faq_db_01`, `bf_faq_db_02`, `bf_ai_db`)
- SMTP4Dev UI: `http://localhost:4590` (SMTP on `1025`)
- RabbitMQ UI: `http://localhost:15672` (AMQP on `5672`, auth disabled)
- Jaeger UI (traces): `http://localhost:16686` (OTLP gRPC `4317`, OTLP HTTP `4318`)
- Prometheus UI: `http://localhost:9090`
- Alertmanager UI: `http://localhost:9093`
- Grafana UI: `http://localhost:3000` (default `admin` / `admin`)
- RabbitMQ Exporter metrics endpoint: `http://localhost:9419/metrics`
- Redis: `localhost:6379`
- FAQ Portal API (Docker): `http://localhost:5010`
- Tenant Back Office API (Docker): `http://localhost:5000`
- Tenant Portal API (Docker): `http://localhost:5002`
- FAQ Public API (Docker): `http://localhost:5020`
- FAQ AI API (Docker): `http://localhost:5030`

## Redis cache operations
Clear all Redis databases:

```bash
redis-cli FLUSHALL
```

Clear only the current database:

```bash
redis-cli FLUSHDB
```

If you need host/port/auth:

```bash
redis-cli -h <host> -p <port> -a <password> FLUSHALL
```

## Test execution
Integration tests:

```bash
dotnet test dotnet/BaseFaq.Faq.Portal.Test.IntegrationTests/BaseFaq.Faq.Portal.Test.IntegrationTests.csproj
dotnet test dotnet/BaseFaq.Faq.Public.Test.IntegrationTests/BaseFaq.Faq.Public.Test.IntegrationTests.csproj
dotnet test dotnet/BaseFaq.Tenant.BackOffice.Test.IntegrationTests/BaseFaq.Tenant.BackOffice.Test.IntegrationTests.csproj
dotnet test dotnet/BaseFaq.Tenant.Portal.Test.IntegrationTests/BaseFaq.Tenant.Portal.Test.IntegrationTests.csproj
```

## Auth0 setup (step-by-step)
You must use an external identity provider. This project expects Auth0 to issue JWTs.

### 1) Create an Auth0 API
Create a new API:
- Name: `BaseFaq API`
- Identifier (Audience): `https://<API_IDENTIFIER>`

### 2) Create an Auth0 application (SPA for Swagger UI)
Create a Single Page Application:
- Allowed Callback URLs: `http://localhost:5010/swagger/oauth2-redirect.html`, `http://localhost:5000/swagger/oauth2-redirect.html`, `http://localhost:5002/swagger/oauth2-redirect.html`
- Allowed Web Origins: `http://localhost:5010`, `http://localhost:5000`, `http://localhost:5002`
- Ensure the app is public (no client secret required)
- In the app's **APIs** tab, authorize access to your API identifier (Audience)

### 3) Configure BaseFaq apps
Edit `dotnet/BaseFaq.Faq.Portal.Api/appsettings.json`, `dotnet/BaseFaq.Tenant.BackOffice.Api/appsettings.json`, and `dotnet/BaseFaq.Tenant.Portal.Api/appsettings.json`:
- `JwtAuthentication:Authority` = `https://<AUTH0_DOMAIN>/`
- `JwtAuthentication:Audience` = `https://<API_IDENTIFIER>`
- `Session:UserIdClaimType` = `sub`
- `SwaggerOptions:swaggerAuth:AuthorizeEndpoint` = `https://<AUTH0_DOMAIN>/authorize`
- `SwaggerOptions:swaggerAuth:TokenEndpoint` = `https://<AUTH0_DOMAIN>/oauth/token`
- `SwaggerOptions:swaggerAuth:Audience` = `https://<API_IDENTIFIER>`
- `SwaggerOptions:swaggerAuth:ClientId` = `<AUTH0_CLIENT_ID>`

Use `<AUTH0_DOMAIN>` from your Auth0 tenant (for example, `your-tenant.us.auth0.com`).

### 4) Include name/email in access tokens (optional)
Auth0 does not add `name`/`email` to access tokens by default. If you need them in API calls,
add an Action that injects namespaced claims:

```js
// Auth0 Action (Post Login)
exports.onExecutePostLogin = async (event, api) => {
  const ns = 'https://basefaq.com/';
  if (event.user.name) {
    api.accessToken.setCustomClaim(`${ns}name`, event.user.name);
  }
  if (event.user.email) {
    api.accessToken.setCustomClaim(`${ns}email`, event.user.email);
  }
};
```

### 5) Call the API
- Protected APIs require:
  - `Authorization: Bearer <access_token>`
  - APIs: FAQ Portal, Tenant Back Office, Tenant Portal
- FAQ Portal requires tenant context header:
  - `X-Tenant-Id: <tenant-guid>`
- FAQ Public requires client context header:
  - `X-Client-Key: <client-key>`

## Troubleshooting runbook
- `network bf-network declared as external, but could not be found`:
  run base services first (`./docker-base.sh` or `.\docker-base.ps1`) before `docker/docker-compose.yml`.
- `set REDIS_PASSWORD` error during base services startup:
  use the helper script, or set `REDIS_PASSWORD` manually before `docker compose`.
- HTTPS local cert warning/failure:
  run `dotnet dev-certs https --trust` once on your machine.

## Shutdown runbook

```bash
docker compose -p bf_baseservices -f docker/docker-compose.baseservices.yml down
```

```bash
docker compose -f docker/docker-compose.yml down
```
