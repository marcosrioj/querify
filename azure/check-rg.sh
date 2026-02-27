#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
source "${SCRIPT_DIR}/lib/common.sh"

STAGE="dev"
CUSTOM_ENV_FILE=""

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

validate_stage "${STAGE}"
ENV_FILE="$(resolve_env_file "${STAGE}" "${CUSTOM_ENV_FILE}")"
load_env_file "${ENV_FILE}"

require_vars AZURE_SUBSCRIPTION_ID AZURE_RESOURCE_GROUP
assert_azure_login
set_azure_subscription "${AZURE_SUBSCRIPTION_ID}"

exists="$(az group exists --name "${AZURE_RESOURCE_GROUP}" -o tsv)"

if [[ "${exists}" == "true" ]]; then
    log_info "Resource Group '${AZURE_RESOURCE_GROUP}' exists for stage '${STAGE}'."
else
    log_info "Resource Group '${AZURE_RESOURCE_GROUP}' does not exist for stage '${STAGE}'."
fi
