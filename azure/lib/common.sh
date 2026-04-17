#!/usr/bin/env bash

# shellcheck shell=bash

readonly AZURE_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)"
readonly REPO_ROOT="$(cd -- "${AZURE_DIR}/.." && pwd)"

log_info() {
    printf "%s\n" "$*"
}

fail() {
    printf "Error: %s\n" "$*" >&2
    exit 1
}

validate_stage() {
    local stage="$1"
    case "${stage}" in
    dev | qa | prod) ;;
    *)
        fail "Invalid stage '${stage}'. Use: dev, qa, prod."
        ;;
    esac
}

resolve_env_file() {
    local stage="$1"
    local custom_env_file="${2:-}"

    if [[ -n "${custom_env_file}" ]]; then
        printf "%s\n" "${custom_env_file}"
        return
    fi

    printf "%s/env/%s.env\n" "${AZURE_DIR}" "${stage}"
}

ensure_env_file_exists() {
    local env_file="$1"
    if [[ -f "${env_file}" ]]; then
        return
    fi

    local example_file="${env_file}.example"
    if [[ -f "${example_file}" ]]; then
        fail "Environment file not found: ${env_file}. Create it from ${example_file}."
    fi

    fail "Environment file not found: ${env_file}."
}

load_env_file() {
    local env_file="$1"
    ensure_env_file_exists "${env_file}"

    set -a
    source "${env_file}"
    set +a
}

assert_command() {
    local command_name="$1"
    command -v "${command_name}" >/dev/null 2>&1 || fail "Command not found: ${command_name}"
}

assert_azure_login() {
    assert_command az
    az account show >/dev/null 2>&1 || fail "Not logged in Azure CLI. Run: az login"
}

set_azure_subscription() {
    local subscription_id="$1"
    az account set --subscription "${subscription_id}"
}

require_vars() {
    local missing=()
    local name

    for name in "$@"; do
        if [[ -z "${!name:-}" ]]; then
            missing+=("${name}")
        fi
    done

    if ((${#missing[@]} > 0)); then
        fail "Missing required variables: ${missing[*]}"
    fi
}

upsert_env() {
    local env_file="$1"
    local key="$2"
    local value="$3"
    local escaped
    escaped="$(printf '%q' "${value}")"

    if grep -qE "^${key}=" "${env_file}"; then
        sed -i "s|^${key}=.*$|${key}=${escaped}|" "${env_file}"
    else
        printf "%s=%s\n" "${key}" "${escaped}" >>"${env_file}"
    fi
}

default_bootstrap_mode_for_stage() {
    local stage="$1"
    case "${stage}" in
    dev) printf "full\n" ;;
    qa | prod) printf "essential\n" ;;
    *) fail "Unknown stage for bootstrap mode: ${stage}" ;;
    esac
}

validate_bootstrap_mode() {
    local mode="$1"
    case "${mode}" in
    essential | full | dummy) ;;
    *)
        fail "Invalid bootstrap mode '${mode}'. Use: essential, full, dummy."
        ;;
    esac
}

default_cors_origins() {
    local qna_portal_domain="$1"
    local qna_public_domain="$2"
    local tenant_backoffice_domain="$3"
    local tenant_portal_domain="$4"

    printf "https://%s;https://%s;https://%s;https://%s" \
        "${qna_portal_domain}" \
        "${qna_public_domain}" \
        "${tenant_backoffice_domain}" \
        "${tenant_portal_domain}"
}
