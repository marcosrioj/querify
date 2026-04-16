#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
source "${SCRIPT_DIR}/lib/common.sh"

STAGE="dev"
CUSTOM_ENV_FILE=""
MODE=""

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
        --mode)
            MODE="${2:-}"
            shift 2
            ;;
        *)
            fail "Unknown argument: $1"
            ;;
        esac
    done
}

seed_action_number_for_mode() {
    local mode="$1"
    case "${mode}" in
    essential) printf "2\n" ;;
    full) printf "3\n" ;;
    dummy) printf "1\n" ;;
    *) fail "Unsupported bootstrap mode: ${mode}" ;;
    esac
}

run_seed_action() {
    local action_number="$1"
    printf "%s\n" "${action_number}" | \
        ConnectionStrings__TenantDb="${TENANT_DB_CONNECTION_STRING}" \
        ConnectionStrings__FaqDb="${FAQ_DB_CONNECTION_STRING}" \
        dotnet run --project "${REPO_ROOT}/dotnet/BaseFaq.Tools.Seed/BaseFaq.Tools.Seed.csproj"
}

validate_config() {
    require_vars TENANT_DB_CONNECTION_STRING FAQ_DB_CONNECTION_STRING

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

    if [[ -z "${MODE}" ]]; then
        MODE="${BOOTSTRAP_MODE:-$(default_bootstrap_mode_for_stage "${STAGE}")}"
    fi
    validate_bootstrap_mode "${MODE}"

    log_info "Running bootstrap mode '${MODE}' for stage '${STAGE}'..."
    selected_action="$(seed_action_number_for_mode "${MODE}")"
    selected_output="$(run_seed_action "${selected_action}")"
    printf "%s\n" "${selected_output}"

    log_info ""
    log_info "Bootstrap completed for stage: ${STAGE}"
}

main "$@"
