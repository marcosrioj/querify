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

validate_config() {
    require_vars \
        AZURE_SUBSCRIPTION_ID \
        AZURE_LOCATION \
        AZURE_RESOURCE_GROUP \
        AZURE_CONTAINERAPPS_ENVIRONMENT \
        AZURE_ACR_NAME \
        BASEFAQ_ENVIRONMENT \
        CONTAINERAPP_PREFIX \
        CORS_ALLOW_ANY_ORIGINS \
        TENANT_DB_CONNECTION_STRING \
        REDIS_HOST \
        REDIS_PORT \
        REDIS_PASSWORD \
        REDIS_USE_SSL \
        RABBITMQ_HOST \
        RABBITMQ_PORT \
        RABBITMQ_USERNAME \
        RABBITMQ_PASSWORD \
        AUTHORITY_URL \
        AUTH_AUDIENCE \
        SWAGGER_AUTH_CLIENT_ID \
        SWAGGER_AUTH_AUDIENCE \
        SWAGGER_AUTH_AUTHORIZE_ENDPOINT \
        SWAGGER_AUTH_TOKEN_ENDPOINT \
        FAQ_PORTAL_DOMAIN \
        FAQ_PUBLIC_DOMAIN \
        TENANT_BACKOFFICE_DOMAIN \
        TENANT_PORTAL_DOMAIN

    if [[ "${BASEFAQ_STAGE:-${STAGE}}" != "${STAGE}" ]]; then
        fail "BASEFAQ_STAGE in env does not match --stage ${STAGE}"
    fi

    if [[ "${CORS_ALLOW_ANY_ORIGINS}" != "true" && "${CORS_ALLOW_ANY_ORIGINS}" != "false" ]]; then
        fail "CORS_ALLOW_ANY_ORIGINS must be 'true' or 'false'."
    fi

    if [[ "${CORS_ALLOW_ANY_ORIGINS}" == "false" && -z "${CORS_ALLOWED_ORIGINS:-}" ]]; then
        CORS_ALLOWED_ORIGINS="$(default_cors_origins \
            "${FAQ_PORTAL_DOMAIN}" \
            "${FAQ_PUBLIC_DOMAIN}" \
            "${TENANT_BACKOFFICE_DOMAIN}" \
            "${TENANT_PORTAL_DOMAIN}")"
        upsert_env "${ENV_FILE}" "CORS_ALLOWED_ORIGINS" "${CORS_ALLOWED_ORIGINS}"
        log_info "CORS_ALLOWED_ORIGINS was empty and has been auto-generated."
    fi
}

ensure_foundation_resources() {
    log_info "Ensuring foundation resources..."
    az extension add --name containerapp --upgrade >/dev/null
    az provider register --namespace Microsoft.App >/dev/null
    az provider register --namespace Microsoft.ContainerRegistry >/dev/null

    az group create \
        --name "${AZURE_RESOURCE_GROUP}" \
        --location "${AZURE_LOCATION}" >/dev/null

    if ! az containerapp env show \
        --resource-group "${AZURE_RESOURCE_GROUP}" \
        --name "${AZURE_CONTAINERAPPS_ENVIRONMENT}" >/dev/null 2>&1; then
        az containerapp env create \
            --resource-group "${AZURE_RESOURCE_GROUP}" \
            --name "${AZURE_CONTAINERAPPS_ENVIRONMENT}" \
            --location "${AZURE_LOCATION}" >/dev/null
    fi

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

resolve_registry_credentials() {
    ACR_LOGIN_SERVER="$(az acr show --name "${AZURE_ACR_NAME}" --query loginServer -o tsv)"
    ACR_USERNAME="$(az acr credential show --name "${AZURE_ACR_NAME}" --query username -o tsv)"
    ACR_PASSWORD="$(az acr credential show --name "${AZURE_ACR_NAME}" --query 'passwords[0].value' -o tsv)"
}

resolve_image_tag() {
    if [[ -z "${IMAGE_TAG:-}" ]]; then
        local git_sha
        git_sha="$(git -C "${REPO_ROOT}" rev-parse --short HEAD 2>/dev/null || echo "nogit")"
        IMAGE_TAG="$(date +%Y%m%d%H%M%S)-${STAGE}-${git_sha}"
    fi
}

build_image() {
    local repository="$1"
    local dockerfile="$2"

    log_info "Building image ${repository}:${IMAGE_TAG}"
    az acr build \
        --registry "${AZURE_ACR_NAME}" \
        --image "${repository}:${IMAGE_TAG}" \
        --file "${dockerfile}" \
        "${REPO_ROOT}" >/dev/null

    printf "%s/%s:%s\n" "${ACR_LOGIN_SERVER}" "${repository}" "${IMAGE_TAG}"
}

set_registry_for_app() {
    local app_name="$1"
    az containerapp registry set \
        --resource-group "${AZURE_RESOURCE_GROUP}" \
        --name "${app_name}" \
        --server "${ACR_LOGIN_SERVER}" \
        --username "${ACR_USERNAME}" \
        --password "${ACR_PASSWORD}" >/dev/null
}

deploy_service() {
    local app_name="$1"
    local image="$2"
    local target_port="$3"
    local ingress="$4"
    local cpu="$5"
    local memory="$6"
    shift 6
    local env_vars=("$@")

    local secrets=(
        "tenant-db-conn=${TENANT_DB_CONNECTION_STRING}"
        "redis-password=${REDIS_PASSWORD}"
        "rabbit-password=${RABBITMQ_PASSWORD}"
    )

    if az containerapp show --resource-group "${AZURE_RESOURCE_GROUP}" --name "${app_name}" >/dev/null 2>&1; then
        log_info "Updating ${app_name}"
        az containerapp update \
            --resource-group "${AZURE_RESOURCE_GROUP}" \
            --name "${app_name}" \
            --image "${image}" \
            --secrets "${secrets[@]}" \
            --set-env-vars "${env_vars[@]}" >/dev/null
    else
        log_info "Creating ${app_name}"
        az containerapp create \
            --resource-group "${AZURE_RESOURCE_GROUP}" \
            --environment "${AZURE_CONTAINERAPPS_ENVIRONMENT}" \
            --name "${app_name}" \
            --image "${image}" \
            --target-port "${target_port}" \
            --ingress "${ingress}" \
            --cpu "${cpu}" \
            --memory "${memory}" \
            --min-replicas 1 \
            --max-replicas 3 \
            --registry-server "${ACR_LOGIN_SERVER}" \
            --registry-username "${ACR_USERNAME}" \
            --registry-password "${ACR_PASSWORD}" \
            --secrets "${secrets[@]}" \
            --env-vars "${env_vars[@]}" >/dev/null
    fi

    set_registry_for_app "${app_name}"
}

print_service_result() {
    local app_name="$1"
    local desired_domain="$2"

    local container_fqdn
    container_fqdn="$(az containerapp show \
        --resource-group "${AZURE_RESOURCE_GROUP}" \
        --name "${app_name}" \
        --query properties.configuration.ingress.fqdn \
        --output tsv 2>/dev/null || true)"

    if [[ -n "${container_fqdn}" ]]; then
        log_info "${app_name}: https://${container_fqdn}  (target domain: https://${desired_domain})"
    else
        log_info "${app_name}: ingress not exposed (target domain: https://${desired_domain})"
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

    ensure_foundation_resources
    resolve_registry_credentials
    resolve_image_tag

    faq_portal_image="$(build_image "basefaq-faq-portal-api" "dotnet/BaseFaq.Faq.Portal.Api/Dockerfile")"
    tenant_backoffice_image="$(build_image "basefaq-tenant-backoffice-api" "dotnet/BaseFaq.Tenant.BackOffice.Api/Dockerfile")"
    tenant_portal_image="$(build_image "basefaq-tenant-portal-api" "dotnet/BaseFaq.Tenant.Portal.Api/Dockerfile")"
    faq_public_image="$(build_image "basefaq-faq-public-api" "dotnet/BaseFaq.Faq.Public.Api/Dockerfile")"

    faq_portal_app="${CONTAINERAPP_PREFIX}-faq-portal-api"
    tenant_backoffice_app="${CONTAINERAPP_PREFIX}-tenant-backoffice-api"
    tenant_portal_app="${CONTAINERAPP_PREFIX}-tenant-portal-api"
    faq_public_app="${CONTAINERAPP_PREFIX}-faq-public-api"

    shared_domains_env=(
        "BaseFaq__Domains__FaqPortal=https://${FAQ_PORTAL_DOMAIN}"
        "BaseFaq__Domains__FaqPublic=https://${FAQ_PUBLIC_DOMAIN}"
        "BaseFaq__Domains__TenantBackOffice=https://${TENANT_BACKOFFICE_DOMAIN}"
        "BaseFaq__Domains__TenantPortal=https://${TENANT_PORTAL_DOMAIN}"
    )

    common_session_env=(
        "ConnectionStrings__TenantDb=secretref:tenant-db-conn"
        "Redis__Host=${REDIS_HOST}"
        "Redis__Port=${REDIS_PORT}"
        "Redis__Password=secretref:redis-password"
        "Redis__UseSsl=${REDIS_USE_SSL}"
        "CORS__AllowAnyOrigins=${CORS_ALLOW_ANY_ORIGINS}"
        "CORS__AllowedOrigins=${CORS_ALLOWED_ORIGINS:-}"
        "${shared_domains_env[@]}"
    )

    common_auth_env=(
        "JwtAuthentication__Authority=${AUTHORITY_URL}"
        "JwtAuthentication__Audience=${AUTH_AUDIENCE}"
        "SwaggerOptions__swaggerAuth__ClientId=${SWAGGER_AUTH_CLIENT_ID}"
        "SwaggerOptions__swaggerAuth__Audience=${SWAGGER_AUTH_AUDIENCE}"
        "SwaggerOptions__swaggerAuth__AuthorizeEndpoint=${SWAGGER_AUTH_AUTHORIZE_ENDPOINT}"
        "SwaggerOptions__swaggerAuth__TokenEndpoint=${SWAGGER_AUTH_TOKEN_ENDPOINT}"
    )

    faq_portal_env=(
        "ASPNETCORE_ENVIRONMENT=${BASEFAQ_ENVIRONMENT}"
        "ASPNETCORE_URLS=http://+:5010"
        "${common_session_env[@]}"
        "${common_auth_env[@]}"
        "RabbitMQ__Hostname=${RABBITMQ_HOST}"
        "RabbitMQ__Port=${RABBITMQ_PORT}"
        "RabbitMQ__Username=${RABBITMQ_USERNAME}"
        "RabbitMQ__Password=secretref:rabbit-password"
    )

    tenant_backoffice_env=(
        "ASPNETCORE_ENVIRONMENT=${BASEFAQ_ENVIRONMENT}"
        "ASPNETCORE_URLS=http://+:5000"
        "${common_session_env[@]}"
        "${common_auth_env[@]}"
    )

    tenant_portal_env=(
        "ASPNETCORE_ENVIRONMENT=${BASEFAQ_ENVIRONMENT}"
        "ASPNETCORE_URLS=http://+:5002"
        "${common_session_env[@]}"
        "${common_auth_env[@]}"
    )

    faq_public_env=(
        "ASPNETCORE_ENVIRONMENT=${BASEFAQ_ENVIRONMENT}"
        "ASPNETCORE_URLS=http://+:5020"
        "${common_session_env[@]}"
        "RabbitMQ__Hostname=${RABBITMQ_HOST}"
        "RabbitMQ__Port=${RABBITMQ_PORT}"
        "RabbitMQ__Username=${RABBITMQ_USERNAME}"
        "RabbitMQ__Password=secretref:rabbit-password"
    )

    deploy_service "${faq_portal_app}" "${faq_portal_image}" "5010" "external" "0.5" "1Gi" "${faq_portal_env[@]}"
    deploy_service "${tenant_backoffice_app}" "${tenant_backoffice_image}" "5000" "external" "0.5" "1Gi" "${tenant_backoffice_env[@]}"
    deploy_service "${tenant_portal_app}" "${tenant_portal_image}" "5002" "external" "0.5" "1Gi" "${tenant_portal_env[@]}"
    deploy_service "${faq_public_app}" "${faq_public_image}" "5020" "external" "0.5" "1Gi" "${faq_public_env[@]}"

    log_info ""
    log_info "Deployment completed for stage: ${STAGE}"
    log_info "Image tag: ${IMAGE_TAG}"
    print_service_result "${faq_portal_app}" "${FAQ_PORTAL_DOMAIN}"
    print_service_result "${tenant_backoffice_app}" "${TENANT_BACKOFFICE_DOMAIN}"
    print_service_result "${tenant_portal_app}" "${TENANT_PORTAL_DOMAIN}"
    print_service_result "${faq_public_app}" "${FAQ_PUBLIC_DOMAIN}"
}

main "$@"
