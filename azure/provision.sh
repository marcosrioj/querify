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

wait_for_provider_registration() {
    local namespace="$1"

    az provider register --namespace "${namespace}" >/dev/null
    for _ in {1..40}; do
        local state
        state="$(az provider show --namespace "${namespace}" --query registrationState -o tsv 2>/dev/null || true)"
        if [[ "${state}" == "Registered" ]]; then
            return
        fi
        sleep 2
    done

    fail "Timed out waiting provider registration for ${namespace}"
}

detect_public_ip() {
    if command -v curl >/dev/null 2>&1; then
        curl -fsSL "https://api.ipify.org" 2>/dev/null || true
        return
    fi

    if command -v wget >/dev/null 2>&1; then
        wget -qO- "https://api.ipify.org" 2>/dev/null || true
        return
    fi

    printf "\n"
}

ensure_resource_group() {
    log_info "Ensuring resource group..."
    az group create \
        --name "${AZURE_RESOURCE_GROUP}" \
        --location "${AZURE_LOCATION}" >/dev/null
}

ensure_container_apps_environment() {
    log_info "Ensuring Container Apps environment..."
    az extension add --name containerapp --upgrade >/dev/null

    if ! az containerapp env show \
        --resource-group "${AZURE_RESOURCE_GROUP}" \
        --name "${AZURE_CONTAINERAPPS_ENVIRONMENT}" >/dev/null 2>&1; then
        az containerapp env create \
            --resource-group "${AZURE_RESOURCE_GROUP}" \
            --name "${AZURE_CONTAINERAPPS_ENVIRONMENT}" \
            --location "${AZURE_LOCATION}" >/dev/null
    fi
}

ensure_acr() {
    log_info "Ensuring ACR..."
    if ! az acr show --name "${AZURE_ACR_NAME}" >/dev/null 2>&1; then
        az acr create \
            --resource-group "${AZURE_RESOURCE_GROUP}" \
            --name "${AZURE_ACR_NAME}" \
            --sku "${AZURE_ACR_SKU:-Basic}" \
            --admin-enabled true >/dev/null
    else
        az acr update --name "${AZURE_ACR_NAME}" --admin-enabled true >/dev/null
    fi
}

ensure_postgres_server() {
    log_info "Ensuring PostgreSQL Flexible Server..."
    if ! az postgres flexible-server show \
        --resource-group "${AZURE_RESOURCE_GROUP}" \
        --name "${AZURE_POSTGRES_SERVER_NAME}" >/dev/null 2>&1; then
        az postgres flexible-server create \
            --resource-group "${AZURE_RESOURCE_GROUP}" \
            --name "${AZURE_POSTGRES_SERVER_NAME}" \
            --location "${AZURE_LOCATION}" \
            --admin-user "${AZURE_POSTGRES_ADMIN_USER}" \
            --admin-password "${AZURE_POSTGRES_ADMIN_PASSWORD}" \
            --sku-name "${AZURE_POSTGRES_SKU_NAME:-Standard_B1ms}" \
            --version "${AZURE_POSTGRES_VERSION:-16}" >/dev/null
    fi
}

ensure_postgres_firewall_rules() {
    log_info "Ensuring PostgreSQL firewall rules..."
    if ! az postgres flexible-server firewall-rule show \
        --resource-group "${AZURE_RESOURCE_GROUP}" \
        --name "${AZURE_POSTGRES_SERVER_NAME}" \
        --rule-name "allow-azure-services" >/dev/null 2>&1; then
        az postgres flexible-server firewall-rule create \
            --resource-group "${AZURE_RESOURCE_GROUP}" \
            --name "${AZURE_POSTGRES_SERVER_NAME}" \
            --rule-name "allow-azure-services" \
            --start-ip-address "0.0.0.0" \
            --end-ip-address "0.0.0.0" >/dev/null
    fi

    if [[ -n "${POSTGRES_CLIENT_IP:-auto}" ]]; then
        local ip_value="${POSTGRES_CLIENT_IP:-auto}"
        if [[ "${ip_value}" == "auto" ]]; then
            ip_value="$(detect_public_ip)"
        fi

        if [[ -n "${ip_value}" ]]; then
            if ! az postgres flexible-server firewall-rule show \
                --resource-group "${AZURE_RESOURCE_GROUP}" \
                --name "${AZURE_POSTGRES_SERVER_NAME}" \
                --rule-name "allow-local-bootstrap" >/dev/null 2>&1; then
                az postgres flexible-server firewall-rule create \
                    --resource-group "${AZURE_RESOURCE_GROUP}" \
                    --name "${AZURE_POSTGRES_SERVER_NAME}" \
                    --rule-name "allow-local-bootstrap" \
                    --start-ip-address "${ip_value}" \
                    --end-ip-address "${ip_value}" >/dev/null
            fi
        fi
    fi
}

ensure_postgres_databases() {
    log_info "Ensuring PostgreSQL databases..."

    local db_names=("${TENANT_DB_NAME}" "${QNA_DB_NAME}")
    if [[ -n "${QNA_DB_NAME_2:-}" ]]; then
        db_names+=("${QNA_DB_NAME_2}")
    fi

    local db_name
    for db_name in "${db_names[@]}"; do
        if ! az postgres flexible-server db show \
            --resource-group "${AZURE_RESOURCE_GROUP}" \
            --server-name "${AZURE_POSTGRES_SERVER_NAME}" \
            --database-name "${db_name}" >/dev/null 2>&1; then
            az postgres flexible-server db create \
                --resource-group "${AZURE_RESOURCE_GROUP}" \
                --server-name "${AZURE_POSTGRES_SERVER_NAME}" \
                --database-name "${db_name}" >/dev/null
        fi
    done
}

ensure_redis() {
    log_info "Ensuring Azure Cache for Redis..."
    if ! az redis show \
        --resource-group "${AZURE_RESOURCE_GROUP}" \
        --name "${AZURE_REDIS_NAME}" >/dev/null 2>&1; then
        az redis create \
            --resource-group "${AZURE_RESOURCE_GROUP}" \
            --name "${AZURE_REDIS_NAME}" \
            --location "${AZURE_LOCATION}" \
            --sku "${AZURE_REDIS_SKU:-Basic}" \
            --vm-size "${AZURE_REDIS_VM_SIZE:-C0}" >/dev/null
    fi
}

wait_for_container_deletion() {
    for _ in {1..40}; do
        if ! az container show \
            --resource-group "${AZURE_RESOURCE_GROUP}" \
            --name "${AZURE_RABBITMQ_CONTAINER_NAME}" >/dev/null 2>&1; then
            return
        fi
        sleep 3
    done

    fail "Timed out waiting for RabbitMQ container deletion."
}

wait_for_container_running() {
    for _ in {1..40}; do
        local state
        state="$(az container show \
            --resource-group "${AZURE_RESOURCE_GROUP}" \
            --name "${AZURE_RABBITMQ_CONTAINER_NAME}" \
            --query "instanceView.state" -o tsv 2>/dev/null || true)"

        if [[ "${state}" == "Running" ]]; then
            return
        fi
        sleep 3
    done

    fail "RabbitMQ container did not reach Running state."
}

ensure_rabbitmq() {
    log_info "Ensuring RabbitMQ container (ACI)..."
    if az container show \
        --resource-group "${AZURE_RESOURCE_GROUP}" \
        --name "${AZURE_RABBITMQ_CONTAINER_NAME}" >/dev/null 2>&1; then
        az container delete \
            --resource-group "${AZURE_RESOURCE_GROUP}" \
            --name "${AZURE_RABBITMQ_CONTAINER_NAME}" \
            --yes >/dev/null
        wait_for_container_deletion
    fi

    az container create \
        --resource-group "${AZURE_RESOURCE_GROUP}" \
        --name "${AZURE_RABBITMQ_CONTAINER_NAME}" \
        --image "rabbitmq:3-management-alpine" \
        --dns-name-label "${AZURE_RABBITMQ_DNS_LABEL}" \
        --ports 5672 15672 \
        --cpu 1 \
        --memory 2 \
        --environment-variables \
        "RABBITMQ_DEFAULT_USER=${RABBITMQ_USERNAME}" \
        "RABBITMQ_DEFAULT_PASS=${RABBITMQ_PASSWORD}" >/dev/null

    wait_for_container_running
}

sync_generated_values_to_env() {
    log_info "Syncing generated connection values into env file..."

    local postgres_host
    postgres_host="$(az postgres flexible-server show \
        --resource-group "${AZURE_RESOURCE_GROUP}" \
        --name "${AZURE_POSTGRES_SERVER_NAME}" \
        --query fullyQualifiedDomainName -o tsv)"

    local redis_host
    redis_host="$(az redis show \
        --resource-group "${AZURE_RESOURCE_GROUP}" \
        --name "${AZURE_REDIS_NAME}" \
        --query hostName -o tsv)"

    local redis_password
    redis_password="$(az redis list-keys \
        --resource-group "${AZURE_RESOURCE_GROUP}" \
        --name "${AZURE_REDIS_NAME}" \
        --query primaryKey -o tsv)"

    local rabbitmq_host
    rabbitmq_host="$(az container show \
        --resource-group "${AZURE_RESOURCE_GROUP}" \
        --name "${AZURE_RABBITMQ_CONTAINER_NAME}" \
        --query ipAddress.fqdn -o tsv)"

    local tenant_db_connection_string
    tenant_db_connection_string="Host=${postgres_host};Port=5432;Database=${TENANT_DB_NAME};Username=${AZURE_POSTGRES_ADMIN_USER};Password=${AZURE_POSTGRES_ADMIN_PASSWORD};SslMode=Require;TrustServerCertificate=true;"

    local qna_db_connection_string
    qna_db_connection_string="Host=${postgres_host};Port=5432;Database=${QNA_DB_NAME};Username=${AZURE_POSTGRES_ADMIN_USER};Password=${AZURE_POSTGRES_ADMIN_PASSWORD};SslMode=Require;TrustServerCertificate=true;"

    upsert_env "${ENV_FILE}" "TENANT_DB_CONNECTION_STRING" "${tenant_db_connection_string}"
    upsert_env "${ENV_FILE}" "QNA_DB_CONNECTION_STRING" "${qna_db_connection_string}"
    upsert_env "${ENV_FILE}" "REDIS_HOST" "${redis_host}"
    upsert_env "${ENV_FILE}" "REDIS_PORT" "6380"
    upsert_env "${ENV_FILE}" "REDIS_PASSWORD" "${redis_password}"
    upsert_env "${ENV_FILE}" "REDIS_USE_SSL" "true"
    upsert_env "${ENV_FILE}" "RABBITMQ_HOST" "${rabbitmq_host}"
    upsert_env "${ENV_FILE}" "RABBITMQ_PORT" "5672"
}

validate_config() {
    require_vars \
        AZURE_SUBSCRIPTION_ID \
        AZURE_LOCATION \
        AZURE_RESOURCE_GROUP \
        AZURE_CONTAINERAPPS_ENVIRONMENT \
        AZURE_ACR_NAME \
        AZURE_POSTGRES_SERVER_NAME \
        AZURE_POSTGRES_ADMIN_USER \
        AZURE_POSTGRES_ADMIN_PASSWORD \
        TENANT_DB_NAME \
        QNA_DB_NAME \
        AZURE_REDIS_NAME \
        AZURE_RABBITMQ_CONTAINER_NAME \
        AZURE_RABBITMQ_DNS_LABEL \
        RABBITMQ_USERNAME \
        RABBITMQ_PASSWORD

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
    assert_azure_login
    set_azure_subscription "${AZURE_SUBSCRIPTION_ID}"

    log_info "Registering required Azure providers..."
    wait_for_provider_registration "Microsoft.App"
    wait_for_provider_registration "Microsoft.ContainerRegistry"
    wait_for_provider_registration "Microsoft.DBforPostgreSQL"
    wait_for_provider_registration "Microsoft.Cache"
    wait_for_provider_registration "Microsoft.ContainerInstance"

    ensure_resource_group
    ensure_container_apps_environment
    ensure_acr
    ensure_postgres_server
    ensure_postgres_firewall_rules
    ensure_postgres_databases
    ensure_redis
    ensure_rabbitmq
    sync_generated_values_to_env

    log_info ""
    log_info "Provisioning completed for stage: ${STAGE}"
    log_info "Env file updated: ${ENV_FILE}"
}

main "$@"
