param(
    [string]$HostIp = "127.0.0.1",
    [string]$UpstreamHost = "host.docker.internal",
    [int]$TenantBackOfficePort = 5000,
    [int]$TenantPublicPort = 5004,
    [int]$TenantPortalPort = 5002,
    [int]$PortalAppPort = 5500,
    [int]$QnaPortalPort = 5010,
    [int]$QnaPublicPort = 5020,
    [int]$TestPort = 5999,
    [string]$ObjectStorageEndpointHost = "",
    [int]$ObjectStorageEndpointPort = 9000,
    [string]$ObjectStorageSignedHost = "localhost:5900"
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($ObjectStorageEndpointHost)) {
    $ObjectStorageEndpointHost = $UpstreamHost
}

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$composeFile = Join-Path $scriptDir "docker-compose.nginx-proxy.yml"
$composeProject = "qf_baseservices"
$composeService = "querify.local.nginx"
$runtimeDir = Join-Path $scriptDir "runtime"
$nginxDir = Join-Path $runtimeDir "nginx"
$nginxConfDir = Join-Path $nginxDir "conf.d"
$nginxConfFile = Join-Path $nginxConfDir "querify-subdomains.conf"
$certDir = Join-Path $scriptDir "certs"
$certFile = Join-Path $certDir "dev.querify.net.crt"
$certKeyFile = Join-Path $certDir "dev.querify.net.key"
$hostsBackupDir = Join-Path $runtimeDir "hosts-backups"
$hostsFile = Join-Path $env:SystemRoot "System32\drivers\etc\hosts"

$markerBegin = "# >>> QUERIFY LOCAL SUBDOMAINS >>>"
$markerEnd = "# <<< QUERIFY LOCAL SUBDOMAINS <<<"

function Test-IsAdmin {
    $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($identity)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

function Remove-MarkerBlock {
    param(
        [string[]]$Lines,
        [string]$Begin,
        [string]$End
    )

    $result = New-Object System.Collections.Generic.List[string]
    $skip = $false

    foreach ($line in $Lines) {
        if ($line -eq $Begin) {
            $skip = $true
            continue
        }
        if ($line -eq $End) {
            $skip = $false
            continue
        }
        if (-not $skip) {
            $null = $result.Add($line)
        }
    }

    return $result
}

docker compose version | Out-Null

if (-not (Test-Path -Path $certFile -PathType Leaf) -or -not (Test-Path -Path $certKeyFile -PathType Leaf)) {
    throw "TLS files are required for HTTPS listener support. Missing: $certFile or $certKeyFile"
}

New-Item -ItemType Directory -Path $nginxConfDir -Force | Out-Null
New-Item -ItemType Directory -Path $hostsBackupDir -Force | Out-Null

$nginxTemplate = @'
map $http_upgrade $connection_upgrade {
    default upgrade;
    '' close;
}

server {
    listen 80 default_server;
    server_name _;
    return 404;
}

server {
    listen 443 ssl default_server;
    server_name _;
    ssl_certificate /etc/nginx/certs/dev.querify.net.crt;
    ssl_certificate_key /etc/nginx/certs/dev.querify.net.key;
    return 404;
}

server {
    listen 80;
    listen 443 ssl;
    server_name dev.portal.querify.net;
    ssl_certificate /etc/nginx/certs/dev.querify.net.crt;
    ssl_certificate_key /etc/nginx/certs/dev.querify.net.key;

    location /s3/ {
        client_max_body_size 60m;
        proxy_pass http://__OBJECT_STORAGE_ENDPOINT_HOST__:__OBJECT_STORAGE_ENDPOINT_PORT__/;
        proxy_http_version 1.1;
        proxy_request_buffering off;
        proxy_buffering off;
        proxy_set_header Host __OBJECT_STORAGE_SIGNED_HOST__;
        proxy_set_header Cookie "";
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Host $host;
        proxy_set_header X-Forwarded-Prefix /s3;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    location /api/tenant/ {
        proxy_pass http://__UPSTREAM_HOST__:__TENANT_PORTAL_PORT__;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection $connection_upgrade;
    }

    location /api/user/ {
        proxy_pass http://__UPSTREAM_HOST__:__TENANT_PORTAL_PORT__;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection $connection_upgrade;
    }

    location /api/qna/ {
        proxy_pass http://__UPSTREAM_HOST__:__QNA_PORTAL_PORT__;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection $connection_upgrade;
    }

    location / {
        proxy_pass http://__UPSTREAM_HOST__:__PORTAL_APP_PORT__;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection $connection_upgrade;
    }
}

server {
    listen 80;
    listen 443 ssl;
    server_name dev.tenant.backoffice.querify.net;
    ssl_certificate /etc/nginx/certs/dev.querify.net.crt;
    ssl_certificate_key /etc/nginx/certs/dev.querify.net.key;

    location / {
        proxy_pass http://__UPSTREAM_HOST__:__TENANT_BACKOFFICE_PORT__;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection $connection_upgrade;
    }
}

server {
    listen 80;
    listen 443 ssl;
    server_name dev.tenant.public.querify.net;
    ssl_certificate /etc/nginx/certs/dev.querify.net.crt;
    ssl_certificate_key /etc/nginx/certs/dev.querify.net.key;

    location / {
        proxy_pass http://__UPSTREAM_HOST__:__TENANT_PUBLIC_PORT__;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection $connection_upgrade;
    }
}

server {
    listen 80;
    listen 443 ssl;
    server_name dev.tenant.portal.querify.net;
    ssl_certificate /etc/nginx/certs/dev.querify.net.crt;
    ssl_certificate_key /etc/nginx/certs/dev.querify.net.key;

    location / {
        proxy_pass http://__UPSTREAM_HOST__:__TENANT_PORTAL_PORT__;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection $connection_upgrade;
    }
}

server {
    listen 80;
    listen 443 ssl;
    server_name dev.qna.portal.querify.net;
    ssl_certificate /etc/nginx/certs/dev.querify.net.crt;
    ssl_certificate_key /etc/nginx/certs/dev.querify.net.key;

    location / {
        proxy_pass http://__UPSTREAM_HOST__:__QNA_PORTAL_PORT__;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection $connection_upgrade;
    }
}

server {
    listen 80;
    listen 443 ssl;
    server_name dev.qna.public.querify.net;
    ssl_certificate /etc/nginx/certs/dev.querify.net.crt;
    ssl_certificate_key /etc/nginx/certs/dev.querify.net.key;

    location / {
        proxy_pass http://__UPSTREAM_HOST__:__QNA_PUBLIC_PORT__;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection $connection_upgrade;
    }
}

server {
    listen 80;
    listen 443 ssl;
    server_name dev.test.querify.net *.test.querify.net;
    ssl_certificate /etc/nginx/certs/dev.querify.net.crt;
    ssl_certificate_key /etc/nginx/certs/dev.querify.net.key;

    location / {
        proxy_pass http://__UPSTREAM_HOST__:__TEST_PORT__;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection $connection_upgrade;
    }
}
'@

$nginxConfig = $nginxTemplate.
    Replace("__UPSTREAM_HOST__", $UpstreamHost).
    Replace("__TENANT_BACKOFFICE_PORT__", $TenantBackOfficePort.ToString()).
    Replace("__TENANT_PUBLIC_PORT__", $TenantPublicPort.ToString()).
    Replace("__TENANT_PORTAL_PORT__", $TenantPortalPort.ToString()).
    Replace("__PORTAL_APP_PORT__", $PortalAppPort.ToString()).
    Replace("__QNA_PORTAL_PORT__", $QnaPortalPort.ToString()).
    Replace("__QNA_PUBLIC_PORT__", $QnaPublicPort.ToString()).
    Replace("__TEST_PORT__", $TestPort.ToString()).
    Replace("__OBJECT_STORAGE_ENDPOINT_HOST__", $ObjectStorageEndpointHost).
    Replace("__OBJECT_STORAGE_ENDPOINT_PORT__", $ObjectStorageEndpointPort.ToString()).
    Replace("__OBJECT_STORAGE_SIGNED_HOST__", $ObjectStorageSignedHost)

Set-Content -Path $nginxConfFile -Encoding ascii -Value $nginxConfig

$backupFile = $null
if (-not (Test-IsAdmin)) {
    throw "Run this script in an elevated PowerShell session (Administrator) to update the hosts file."
}

$hostsLines = Get-Content -Path $hostsFile -ErrorAction Stop
$cleanHosts = Remove-MarkerBlock -Lines $hostsLines -Begin $markerBegin -End $markerEnd

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$backupFile = Join-Path $hostsBackupDir "hosts.$timestamp.bak"
Copy-Item -Path $hostsFile -Destination $backupFile -Force

if ($cleanHosts.Count -gt 0 -and $cleanHosts[$cleanHosts.Count - 1] -ne "") {
    $null = $cleanHosts.Add("")
}

$null = $cleanHosts.Add($markerBegin)
$null = $cleanHosts.Add("$HostIp dev.portal.querify.net")
$null = $cleanHosts.Add("$HostIp dev.tenant.backoffice.querify.net")
$null = $cleanHosts.Add("$HostIp dev.tenant.portal.querify.net")
$null = $cleanHosts.Add("$HostIp dev.qna.portal.querify.net")
$null = $cleanHosts.Add("$HostIp dev.qna.public.querify.net")
$null = $cleanHosts.Add("$HostIp dev.test.querify.net")
$null = $cleanHosts.Add($markerEnd)

Set-Content -Path $hostsFile -Encoding ascii -Value $cleanHosts

$env:COMPOSE_IGNORE_ORPHANS = "1"
try {
    docker compose -p $composeProject -f $composeFile up -d --force-recreate --no-deps $composeService
}
finally {
    Remove-Item Env:\COMPOSE_IGNORE_ORPHANS -ErrorAction SilentlyContinue
}

Write-Host ""
Write-Host "Querify local subdomain proxy is ready."
Write-Host "Docker compose project: $composeProject"
Write-Host "Upstream host: $UpstreamHost"
Write-Host ""
Write-Host "Domain mappings:"
Write-Host "  dev.portal.querify.net            -> $UpstreamHost`:$PortalAppPort"
Write-Host "  dev.portal.querify.net/api/tenant -> $UpstreamHost`:$TenantPortalPort"
Write-Host "  dev.portal.querify.net/api/user   -> $UpstreamHost`:$TenantPortalPort"
Write-Host "  dev.portal.querify.net/api/qna    -> $UpstreamHost`:$QnaPortalPort"
Write-Host "  dev.portal.querify.net/s3         -> $ObjectStorageEndpointHost`:$ObjectStorageEndpointPort"
Write-Host "  dev.tenant.backoffice.querify.net -> $UpstreamHost`:$TenantBackOfficePort"
Write-Host "  dev.tenant.portal.querify.net     -> $UpstreamHost`:$TenantPortalPort"
Write-Host "  dev.qna.portal.querify.net        -> $UpstreamHost`:$QnaPortalPort"
Write-Host "  dev.qna.public.querify.net        -> $UpstreamHost`:$QnaPublicPort"
Write-Host "  dev.test.querify.net              -> $UpstreamHost`:$TestPort"
Write-Host ""
Write-Host "Generated Nginx config:"
Write-Host "  $nginxConfFile"
Write-Host "Object storage signed host header: $ObjectStorageSignedHost"
Write-Host "TLS cert files:"
Write-Host "  $certFile"
Write-Host "  $certKeyFile"
Write-Host "Forward external 80 -> machine 80."
Write-Host "Forward external 443 -> machine 443."
if ($backupFile) {
    Write-Host ""
    Write-Host "Hosts backup:"
    Write-Host "  $backupFile"
}
