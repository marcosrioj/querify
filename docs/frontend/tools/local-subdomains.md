# Local Subdomain Helper

## Purpose

This guide documents the self-contained helper under `devops/local/simulatedev` that exposes local APIs and the Portal behind shared-looking subdomains.

Primary hostnames:

- `dev.portal.querify.net`
- `dev.tenant.backoffice.querify.net`
- `dev.tenant.public.querify.net`
- `dev.tenant.portal.querify.net`
- `dev.qna.public.querify.net`
- `dev.qna.portal.querify.net`
- `dev.test.querify.net`

The approach is intentionally decoupled from host-machine Nginx:

- Nginx runs in Docker with config generated inside `devops/local/simulatedev/runtime/`
- Nginx publishes `80:80` and `443:443`
- HTTP requests to `dev.portal.querify.net` redirect to HTTPS; other HTTP and HTTPS requests are proxied to the mapped local app and API ports
- hosts-file entries are always updated by setup and removed by teardown using marker blocks
- cleanup scripts stop the proxy container and remove managed hosts entries

## App and API mapping

- `dev.portal.querify.net` -> `http://<upstream-host>:5500`
- `dev.portal.querify.net/api/tenant` -> `http://<upstream-host>:5002`
- `dev.portal.querify.net/api/user` -> `http://<upstream-host>:5002`
- `dev.portal.querify.net/api/qna` -> `http://<upstream-host>:5010`
- `dev.tenant.backoffice.querify.net` -> `http://<upstream-host>:5000`
- `dev.tenant.public.querify.net` -> `http://<upstream-host>:5004`
- `dev.tenant.portal.querify.net` -> `http://<upstream-host>:5002`
- `dev.qna.portal.querify.net` -> `http://<upstream-host>:5010`
- `dev.qna.public.querify.net` -> `http://<upstream-host>:5020`
- `dev.test.querify.net` -> `http://<upstream-host>:5999`

`<upstream-host>` defaults:

- Linux: `host.docker.internal`
- Windows: `host.docker.internal`

These defaults match the ports in the current API `launchSettings.json`.

## Files

- `docker-compose.nginx-proxy.yml`: reverse proxy stack for Linux and Windows
- `certs/dev.querify.net.crt`: dev TLS certificate used for HTTPS listeners
- `certs/dev.querify.net.key`: dev TLS private key used for HTTPS listeners
- `setup-subdomains.sh`: Linux setup
- `teardown-subdomains.sh`: Linux cleanup
- `setup-subdomains.ps1`: Windows setup
- `teardown-subdomains.ps1`: Windows cleanup

## Linux usage

From repo root:

```bash
chmod +x devops/local/simulatedev/setup-subdomains.sh devops/local/simulatedev/teardown-subdomains.sh
./devops/local/simulatedev/setup-subdomains.sh
```

Cleanup:

```bash
./devops/local/simulatedev/teardown-subdomains.sh
```

## Windows usage

From repo root in PowerShell:

```powershell
.\devops\local\simulatedev\setup-subdomains.ps1
```

Cleanup:

```powershell
.\devops\local\simulatedev\teardown-subdomains.ps1
```

## Optional overrides

You can override defaults through environment variables on Linux or parameters on PowerShell:

- core settings:
  - `HOST_IP` or `-HostIp`, default `127.0.0.1`
  - `UPSTREAM_HOST` or `-UpstreamHost`, default `host.docker.internal`
- API and app ports:
  - `PORTAL_APP_PORT` or `-PortalAppPort`
  - `TENANT_BACKOFFICE_PORT` or `-TenantBackOfficePort`
  - `TENANT_PUBLIC_PORT` or `-TenantPublicPort`
  - `TENANT_PORTAL_PORT` or `-TenantPortalPort`
  - `QNA_PORTAL_PORT` or `-QnaPortalPort`
  - `QNA_PUBLIC_PORT` or `-QnaPublicPort`
  - `TEST_PORT` or `-TestPort`

Linux example:

```bash
PORTAL_APP_PORT=6500 TENANT_PORTAL_PORT=6002 ./devops/local/simulatedev/setup-subdomains.sh
```

PowerShell example:

```powershell
.\devops\local\simulatedev\setup-subdomains.ps1 -PortalAppPort 6500 -TenantPortalPort 6002
```

## Generated runtime files

Generated artifacts are created under:

- `devops/local/simulatedev/runtime/nginx/conf.d/querify-subdomains.conf`
- `devops/local/simulatedev/runtime/hosts-backups/`

This keeps all helper outputs inside `devops/local/` while preserving host-file backups.

Use elevated permissions when running scripts because hosts-file updates are mandatory.
HTTPS uses a dev self-signed certificate, so browsers may show a certificate warning.

## Internet-facing machine checklist

- point DNS `A` or `CNAME` records for all `dev.*.querify.net` subdomains to your public IP
- router or NAT forwarding required: `TCP 80 -> <machine_lan_ip>:80` for the Nginx entrypoint
- router or NAT forwarding required: `TCP 443 -> <machine_lan_ip>:443` for HTTPS
- router or NAT forwarding required: `TCP 5000-5999 -> <machine_lan_ip>:5000-5999` for direct backend ports, only when needed for diagnostics
- do not use `:5000`, `:5002`, `:5010`, `:5020`, or `:5999` in public URLs; those are backend API ports
- use `https://...` in public URLs when testing TLS, or `http://...` if you want to avoid certificate warnings
- ensure APIs are running on the mapped local ports
- for the Portal Docker stack, prefer same-origin API URLs through `dev.portal.querify.net/api/*`; this avoids browser CORS/TLS failures from cross-origin local API subdomains
- Docker Compose project used by scripts: `qf_baseservices`
