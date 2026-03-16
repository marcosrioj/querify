# BaseFAQ Local Subdomain Helper

This folder contains a self-contained helper to expose local APIs behind these subdomains:

- `dev.tenant.portal.basefaq.com`
- `dev.tenant.backoffice.basefaq.com`
- `dev.faq.public.basefaq.com`
- `dev.faq.portal.basefaq.com`
- `dev.ai.basefaq.com`
- `dev.test.basefaq.com`

The approach is intentionally decoupled from host machine Nginx:

- Nginx runs in Docker (`nginx:alpine`) with config generated inside `local/env/simulatedev/runtime/`.
- Nginx publishes `80:80` and `443:443`.
- HTTP requests are proxied to the mapped API ports.
- HTTPS requests are terminated at nginx and redirected to `http://<same-host><same-path>` on port `80`.
- Hosts-file entries are always updated by setup and removed by teardown using marker blocks.
- Cleanup scripts stop the proxy container and remove managed hosts entries.

## API mapping

- `dev.tenant.backoffice.basefaq.com` -> `http://<upstream-host>:5000`
- `dev.tenant.portal.basefaq.com` -> `http://<upstream-host>:5002`
- `dev.faq.portal.basefaq.com` -> `http://<upstream-host>:5010`
- `dev.faq.public.basefaq.com` -> `http://<upstream-host>:5020`
- `dev.ai.basefaq.com` -> `http://<upstream-host>:5030`
- `dev.test.basefaq.com` -> `http://<upstream-host>:5999`

`<upstream-host>` defaults:
- Linux: `host.docker.internal`
- Windows: `host.docker.internal`

These defaults match the ports in the current API `launchSettings.json`.

## Files

- `docker-compose.nginx-proxy.yml`: reverse proxy stack (Linux/Windows).
- `certs/dev.basefaq.com.crt`: dev TLS certificate used for HTTPS redirect listener.
- `certs/dev.basefaq.com.key`: dev TLS private key used for HTTPS redirect listener.
- `setup-subdomains.sh`: Linux setup.
- `teardown-subdomains.sh`: Linux cleanup.
- `setup-subdomains.ps1`: Windows setup.
- `teardown-subdomains.ps1`: Windows cleanup.

## Linux usage

From repo root:

```bash
chmod +x local/env/simulatedev/setup-subdomains.sh local/env/simulatedev/teardown-subdomains.sh
./local/env/simulatedev/setup-subdomains.sh
```

Cleanup:

```bash
./local/env/simulatedev/teardown-subdomains.sh
```

## Windows usage (PowerShell)

From repo root:

```powershell
.\local\env\simulatedev\setup-subdomains.ps1
```

Cleanup:

```powershell
.\local\env\simulatedev\teardown-subdomains.ps1
```

## Optional overrides

You can override defaults through environment variables (Linux) or parameters (PowerShell):

- Core settings:
  - `HOST_IP` / `-HostIp` (default `127.0.0.1`)
  - `UPSTREAM_HOST` / `-UpstreamHost` (default `host.docker.internal`)
- API ports:
  - `TENANT_BACKOFFICE_PORT` / `-TenantBackOfficePort`
  - `TENANT_PORTAL_PORT` / `-TenantPortalPort`
  - `FAQ_PORTAL_PORT` / `-FaqPortalPort`
  - `FAQ_PUBLIC_PORT` / `-FaqPublicPort`
  - `AI_PORT` / `-AiPort`
  - `TEST_PORT` / `-TestPort`

Linux example:

```bash
TENANT_PORTAL_PORT=6002 ./local/env/simulatedev/setup-subdomains.sh
```

PowerShell example:

```powershell
.\local\env\simulatedev\setup-subdomains.ps1 -TenantPortalPort 6002
```

## Generated runtime files

Generated artifacts are created under:

- `local/env/simulatedev/runtime/nginx/conf.d/basefaq-subdomains.conf`
- `local/env/simulatedev/runtime/hosts-backups/`

This keeps all helper outputs inside `local/` while preserving host-file backups.

Use elevated permissions when running scripts because hosts-file updates are mandatory.
HTTPS redirect uses a dev self-signed certificate, so browsers may show a certificate warning before redirecting.

## Internet-facing machine checklist

- Point DNS `A`/`CNAME` records for all `dev.*.basefaq.com` subdomains to your public IP.
- Router/NAT forwarding required: `TCP 80 -> <machine_lan_ip>:80` (nginx entrypoint).
- Router/NAT forwarding required: `TCP 443 -> <machine_lan_ip>:443` (HTTPS redirect entrypoint).
- Router/NAT forwarding required: `TCP 5000-5999 -> <machine_lan_ip>:5000-5999` (direct backend ports, optional for diagnostics).
- Do not use `:5000`, `:5002`, `:5010`, `:5020`, `:5030`, `:5999` in public URLs; those are backend API ports.
- Use `http://...` in public URLs if you want to avoid certificate warnings.
- Ensure APIs are running on the mapped local ports.
- Docker compose project used by scripts: `bf_baseservices`.
