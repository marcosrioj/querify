#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
source "${SCRIPT_DIR}/lib/common.sh"

STAGE="dev"
CUSTOM_ENV_FILE=""

parse_args() {
    while [[ $# -gt 0 ]]; do
        case "$1" in
        --stage)
            STAGE="${2:-}"
            shift 2
            ;;
        --env-file)
            CUSTOM_ENV_FILE="${2:-}"
            shift 2
            ;;
        *)
            fail "Unknown argument: $1"
            ;;
        esac
    done
}

ensure_dotnet_ef() {
    export PATH="${PATH}:${HOME}/.dotnet/tools"

    if dotnet ef --version >/dev/null 2>&1; then
        return
    fi

    dotnet tool install --global dotnet-ef --version 10.0.0 >/dev/null
}

run_tenant_migration() {
    log_info "Applying Tenant DB migration..."
    ConnectionStrings__TenantDb="${TENANT_DB_CONNECTION_STRING}" \
        dotnet ef database update \
        --project "${REPO_ROOT}/dotnet/BaseFaq.Common.EntityFramework.Tenant/BaseFaq.Common.EntityFramework.Tenant.csproj" \
        --startup-project "${REPO_ROOT}/dotnet/BaseFaq.Tenant.BackOffice.Api/BaseFaq.Tenant.BackOffice.Api.csproj"
}

run_faq_migrations_for_tenants() {
    log_info "Applying FAQ DB migrations for all tenant connections..."
    ConnectionStrings__TenantDb="${TENANT_DB_CONNECTION_STRING}" \
        dotnet run \
        --project "${REPO_ROOT}/dotnet/BaseFaq.Tools.Migration/BaseFaq.Tools.Migration.csproj" \
        -- \
        --app faq \
        --command database-update
}

validate_config() {
    require_vars TENANT_DB_CONNECTION_STRING

    if [[ "${BASEFAQ_STAGE:-${STAGE}}" != "${STAGE}" ]]; then
        fail "BASEFAQ_STAGE in env does not match --stage ${STAGE}"
    fi
}

main() {
    parse_args "$@"
    validate_stage "${STAGE}"

    ENV_FILE="$(resolve_env_file "${STAGE}" "${CUSTOM_ENV_FILE}")"
    load_env_file "${ENV_FILE}"

    validate_config
    assert_command dotnet
    ensure_dotnet_ef

    run_tenant_migration
    run_faq_migrations_for_tenants

    log_info "Migrations completed for stage: ${STAGE}"
}

main "$@"
