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

main() {
    parse_args "$@"
    validate_stage "${STAGE}"

    ENV_FILE="$(resolve_env_file "${STAGE}" "${CUSTOM_ENV_FILE}")"
    ensure_env_file_exists "${ENV_FILE}"

    mode_args=()
    if [[ -n "${MODE}" ]]; then
        validate_bootstrap_mode "${MODE}"
        mode_args=(--mode "${MODE}")
    fi

    "${SCRIPT_DIR}/provision.sh" --stage "${STAGE}" --env-file "${ENV_FILE}"
    "${SCRIPT_DIR}/bootstrap-data.sh" --stage "${STAGE}" --env-file "${ENV_FILE}" "${mode_args[@]}"
    "${SCRIPT_DIR}/deploy.sh" --stage "${STAGE}" --env-file "${ENV_FILE}"
}

main "$@"
