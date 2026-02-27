#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
source "${SCRIPT_DIR}/lib/common.sh"

STAGE="dev"

while [[ $# -gt 0 ]]; do
    case "$1" in
    --stage)
        STAGE="${2:-}"
        shift 2
        ;;
    *)
        fail "Unknown argument: $1"
        ;;
    esac
done

validate_stage "${STAGE}"

target_env="${AZURE_DIR}/env/${STAGE}.env"
source_example="${AZURE_DIR}/env/${STAGE}.env.example"

if [[ ! -f "${source_example}" ]]; then
    fail "Template not found: ${source_example}"
fi

if [[ -f "${target_env}" ]]; then
    log_info "Env file already exists: ${target_env}"
    exit 0
fi

cp "${source_example}" "${target_env}"
log_info "Created: ${target_env}"
