param(
    [string]$HostIp = "127.0.0.1",
    [string]$UpstreamHost = "host.docker.internal",
    [int]$TenantBackOfficePort = 5000,
    [int]$TenantPublicPort = 5004,
    [int]$TenantPortalPort = 5002,
    [int]$PortalAppPort = 5500,
    [int]$FaqPortalPort = 5010,
    [int]$FaqPublicPort = 5020,
    [int]$TestPort = 5999
)

$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$composeFile = Join-Path $scriptDir "docker-compose.nginx-proxy.yml"
$composeProject = "bf_baseservices"
$composeService = "basefaq.local.nginx"
$runtimeDir = Join-Path $scriptDir "runtime"
$nginxDir = Join-Path $runtimeDir "nginx"
$nginxConfDir = Join-Path $nginxDir "conf.d"
$nginxConfFile = Join-Path $nginxConfDir "basefaq-subdomains.conf"
$certDir = Join-Path $scriptDir "certs"
$certFile = Join-Path $certDir "dev.basefaq.com.crt"
$certKeyFile = Join-Path $certDir "dev.basefaq.com.key"
$hostsBackupDir = Join-Path $runtimeDir "hosts-backups"
$hostsFile = Join-Path $env:SystemRoot "System32\drivers\etc\hosts"

$markerBegin = "# >>> BASEFAQ LOCAL SUBDOMAINS >>>"
$markerEnd = "# <<< BASEFAQ LOCAL SUBDOMAINS <<<"

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
    ssl_certificate /etc/nginx/certs/dev.basefaq.com.crt;
    ssl_certificate_key /etc/nginx/certs/dev.basefaq.com.key;
    return 404;
}

server {
    listen 80;
    listen 443 ssl;
    server_name dev.portal.basefaq.com;
    ssl_certificate /etc/nginx/certs/dev.basefaq.com.crt;
    ssl_certificate_key /etc/nginx/certs/dev.basefaq.com.key;

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
    server_name dev.tenant.backoffice.basefaq.com;
    ssl_certificate /etc/nginx/certs/dev.basefaq.com.crt;
    ssl_certificate_key /etc/nginx/certs/dev.basefaq.com.key;

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
    server_name dev.tenant.public.basefaq.com;
    ssl_certificate /etc/nginx/certs/dev.basefaq.com.crt;
    ssl_certificate_key /etc/nginx/certs/dev.basefaq.com.key;

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
    server_name dev.tenant.portal.basefaq.com;
    ssl_certificate /etc/nginx/certs/dev.basefaq.com.crt;
    ssl_certificate_key /etc/nginx/certs/dev.basefaq.com.key;

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
    server_name dev.faq.portal.basefaq.com;
    ssl_certificate /etc/nginx/certs/dev.basefaq.com.crt;
    ssl_certificate_key /etc/nginx/certs/dev.basefaq.com.key;

    location / {
        proxy_pass http://__UPSTREAM_HOST__:__FAQ_PORTAL_PORT__;
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
    server_name dev.faq.public.basefaq.com;
    ssl_certificate /etc/nginx/certs/dev.basefaq.com.crt;
    ssl_certificate_key /etc/nginx/certs/dev.basefaq.com.key;

    location / {
        proxy_pass http://__UPSTREAM_HOST__:__FAQ_PUBLIC_PORT__;
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
    server_name dev.test.basefaq.com *.test.basefaq.com;
    ssl_certificate /etc/nginx/certs/dev.basefaq.com.crt;
    ssl_certificate_key /etc/nginx/certs/dev.basefaq.com.key;

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
    Replace("__FAQ_PORTAL_PORT__", $FaqPortalPort.ToString()).
    Replace("__FAQ_PUBLIC_PORT__", $FaqPublicPort.ToString()).
    Replace("__TEST_PORT__", $TestPort.ToString())

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
$null = $cleanHosts.Add("$HostIp dev.portal.basefaq.com")
$null = $cleanHosts.Add("$HostIp dev.tenant.backoffice.basefaq.com")
$null = $cleanHosts.Add("$HostIp dev.tenant.portal.basefaq.com")
$null = $cleanHosts.Add("$HostIp dev.faq.portal.basefaq.com")
$null = $cleanHosts.Add("$HostIp dev.faq.public.basefaq.com")
$null = $cleanHosts.Add("$HostIp dev.test.basefaq.com")
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
Write-Host "BaseFAQ local subdomain proxy is ready."
Write-Host "Docker compose project: $composeProject"
Write-Host "Upstream host: $UpstreamHost"
Write-Host ""
Write-Host "Domain mappings:"
Write-Host "  dev.portal.basefaq.com            -> $UpstreamHost`:$PortalAppPort"
Write-Host "  dev.tenant.backoffice.basefaq.com -> $UpstreamHost`:$TenantBackOfficePort"
Write-Host "  dev.tenant.portal.basefaq.com     -> $UpstreamHost`:$TenantPortalPort"
Write-Host "  dev.faq.portal.basefaq.com        -> $UpstreamHost`:$FaqPortalPort"
Write-Host "  dev.faq.public.basefaq.com        -> $UpstreamHost`:$FaqPublicPort"
Write-Host "  dev.test.basefaq.com              -> $UpstreamHost`:$TestPort"
Write-Host ""
Write-Host "Generated Nginx config:"
Write-Host "  $nginxConfFile"
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
