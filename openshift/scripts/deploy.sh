#!/bin/bash

env=${1-dev}
tag=${2-latest}
echo "Deploying to $env"

# Detect OS (Git Bash / MSYS / Cygwin all report Windows-like uname)
_os=$(uname -s 2>/dev/null || echo "Windows")
case "$_os" in
  MINGW*|MSYS*|CYGWIN*) _is_windows=true ;;
  *) _is_windows=false ;;
esac

# --- Pre-flight: verify required tools are available ---
_missing=false

if ! command -v oc &>/dev/null; then
  echo "ERROR: oc (OpenShift CLI) is not installed."
  echo "       https://docs.openshift.com/container-platform/latest/cli_reference/openshift_cli/getting-started-cli.html"
  _missing=true
fi

if command -v jq &>/dev/null; then
  _json_tool="jq"
elif command -v python3 &>/dev/null; then
  echo "NOTE: jq not found; will use python3 for JSON parsing."
  echo "      To install jq (also works in Git Bash on Windows):"
  echo "        Windows:       winget install stedolan.jq"
  echo "        Windows:       choco install jq  (if using Chocolatey)"
  echo "        RHEL/Fedora:   sudo dnf install jq"
  echo "        Ubuntu/Debian: sudo apt install jq"
  echo "        macOS:         brew install jq"
  _json_tool="python3"
else
  echo "ERROR: Neither jq nor python3 is available."
  echo "       Install jq (also works in Git Bash on Windows):"
  echo "         Windows:       winget install stedolan.jq"
  echo "         Windows:       choco install jq  (if using Chocolatey)"
  echo "         RHEL/Fedora:   sudo dnf install jq"
  echo "         Ubuntu/Debian: sudo apt install jq"
  echo "         macOS:         brew install jq"
  _missing=true
fi

if [[ "$_is_windows" == false ]] && command -v skopeo &>/dev/null; then
  _tag_tool="skopeo"
elif command -v curl &>/dev/null; then
  if [[ "$_is_windows" == false ]]; then
    echo "NOTE: skopeo not found; will use curl for image retagging."
    echo "      To install skopeo:"
    echo "        RHEL/Fedora:   sudo dnf install skopeo"
    echo "        Ubuntu/Debian: sudo apt install skopeo"
    echo "        macOS:         brew install skopeo"
  fi
  _tag_tool="curl"
else
  echo "ERROR: Neither skopeo nor curl is available. Install one to proceed."
  echo "       RHEL/Fedora:   sudo dnf install skopeo"
  echo "       Ubuntu/Debian: sudo apt install skopeo"
  echo "       macOS:         brew install skopeo"
  _missing=true
fi

if ! docker info &>/dev/null 2>&1; then
  echo "ERROR: Docker is not running. Start Docker Desktop (or the Docker daemon) and try again."
  _missing=true
fi

[[ "$_missing" == true ]] && exit 1
# -------------------------------------------------------

oc project 9b301c-tools

# Extract ACR registry details from an existing deployment image
_acr_image=$(oc get deployment nginx -n 9b301c-$env -o jsonpath='{.spec.template.spec.containers[0].image}' 2>/dev/null)
ACR_HOST=$(echo "$_acr_image" | cut -d'/' -f1)
if [[ -z "$ACR_HOST" ]]; then
  ACR_HOST="bcgov-c4awhwfpcremdbga.azurecr.io"
fi

# ACR registry name used by az acr commands (distinct from the login server hostname)
ACR_NAME="bcgov"

echo "Using ACR registry: $ACR_HOST"

# Extract ACR credentials from the known imagePullSecret
_secret_name="acr-secret"
_dockerconfig=$(oc get secret "$_secret_name" -n 9b301c-$env -o jsonpath='{.data.\.dockerconfigjson}' | base64 -d)

if [[ "$_json_tool" == "jq" ]]; then
  ACR_USER=$(echo "$_dockerconfig" | jq -r ".auths[\"$ACR_HOST\"].username")
  ACR_PASS=$(echo "$_dockerconfig" | jq -r ".auths[\"$ACR_HOST\"].password")
else
  ACR_USER=$(echo "$_dockerconfig" | python3 -c "import sys,json; d=json.load(sys.stdin); print(d['auths']['$ACR_HOST']['username'])")
  ACR_PASS=$(echo "$_dockerconfig" | python3 -c "import sys,json; d=json.load(sys.stdin); print(d['auths']['$ACR_HOST']['password'])")
fi

if [[ -z "$ACR_USER" || -z "$ACR_PASS" ]]; then
  echo "ERROR: Could not extract ACR credentials from secret '$_secret_name'."
  exit 1
fi

# Log into Azure and ACR using the Azure CLI, then refresh ACR_USER/ACR_PASS.
# Prefers admin credentials (push access); falls back to --expose-token (no Docker needed).
_acr_login () {
  if ! command -v az &>/dev/null; then
    echo "ERROR: Azure CLI (az) is not installed. Cannot authenticate automatically."
    echo "       Install: https://learn.microsoft.com/cli/azure/install-azure-cli"
    exit 1
  fi
  if ! az account show &>/dev/null 2>&1; then
    echo "Logging into Azure (az login)..."
    az login
  fi
  echo "Logging into ACR ($ACR_HOST)..."
  # Try admin credentials first — these have full push/pull access
  local _admin_user _admin_pass
  _admin_user=$(az acr credential show --name "$ACR_NAME" --query username -o tsv 2>/dev/null)
  _admin_pass=$(az acr credential show --name "$ACR_NAME" --query "passwords[0].value" -o tsv 2>/dev/null)
  if [[ -n "$_admin_user" && -n "$_admin_pass" ]]; then
    ACR_USER="$_admin_user"
    ACR_PASS="$_admin_pass"
  else
    # Admin user not enabled — use OAuth token exchange (requires AcrPush role, no Docker needed)
    echo "NOTE: Admin credentials unavailable; using OAuth token exchange (AcrPush role required)."
    local _token_json _refresh_token _access_token_json

    # Step 1: get the refresh token
    _token_json=$(az acr login --name "$ACR_NAME" --expose-token --output json 2>/dev/null)
    if [[ "$_json_tool" == "jq" ]]; then
      _refresh_token=$(echo "$_token_json" | jq -r '.accessToken')
    else
      _refresh_token=$(echo "$_token_json" | python3 -c "import sys,json; print(json.load(sys.stdin)['accessToken'])")
    fi

    # Step 2: exchange the refresh token for an access token with pull+push scope
    _access_token_json=$(curl -sf -X POST "https://$ACR_HOST/oauth2/token" \
      --data-urlencode "grant_type=refresh_token" \
      --data-urlencode "service=$ACR_HOST" \
      --data-urlencode "scope=repository:*:pull,push" \
      --data-urlencode "refresh_token=$_refresh_token")

    ACR_USER="00000000-0000-0000-0000-000000000000"
    if [[ "$_json_tool" == "jq" ]]; then
      ACR_PASS=$(echo "$_access_token_json" | jq -r '.access_token')
    else
      ACR_PASS=$(echo "$_access_token_json" | python3 -c "import sys,json; print(json.load(sys.stdin)['access_token'])")
    fi

    if [[ -z "$ACR_PASS" || "$ACR_PASS" == "null" ]]; then
      echo "ERROR: Failed to exchange refresh token for an access token."
      echo "       Ensure your Azure account has the AcrPush role on $ACR_HOST."
      exit 1
    fi
  fi
}

# Verify ACR credentials are valid before doing any work; auto-login if not
echo "Verifying ACR authentication..."
_auth_status=$(curl -sf -o /dev/null -w "%{http_code}" -u "$ACR_USER:$ACR_PASS" "https://$ACR_HOST/v2/")
if [[ "$_auth_status" != "200" ]]; then
  echo "NOTE: ACR credentials invalid or expired (HTTP $_auth_status). Attempting login..."
  _acr_login
  _auth_status=$(curl -sf -o /dev/null -w "%{http_code}" -u "$ACR_USER:$ACR_PASS" "https://$ACR_HOST/v2/")
  if [[ "$_auth_status" != "200" ]]; then
    echo "ERROR: ACR authentication failed after login (HTTP $_auth_status)."
    exit 1
  fi
fi
echo "ACR authentication verified."

# Get current pod counts
getPods () {
  name=${1-}
  type=${2-deployment}
  env=${3-'dev'}
  result=$(oc get $type -n 9b301c-$env $name -o "jsonpath={.status.availableReplicas}" 2>/dev/null)
  # Ensure we always return a number (default to 0 if empty)
  if [ -z "$result" ]; then
    result="0"
  fi
  echo "$result"
}

scale () {
  name=${1-}
  replicas=${2-0}
  type=${3-deployment}
  env=${4-'dev'}
  oc scale $type $name -n 9b301c-$env --replicas=$replicas
}

# Retag an image in ACR: source tag → env tag (manifest copy, no layer download)
# Uses skopeo when available; falls back to curl + Docker Registry v2 API on Windows or when skopeo is absent.
# Verify an image tag exists in ACR after tagging
acr_verify () {
  local image=$1
  local _accept="application/vnd.docker.distribution.manifest.list.v2+json,application/vnd.docker.distribution.manifest.v2+json,application/vnd.oci.image.index.v1+json,application/vnd.oci.image.manifest.v1+json"
  local _status
  _status=$(curl -sf -o /dev/null -w "%{http_code}" -u "$ACR_USER:$ACR_PASS" \
    -H "Accept: $_accept" \
    "https://$ACR_HOST/v2/$image/manifests/$env")
  if [[ "$_status" != "200" ]]; then
    echo "ERROR: Tag verification failed for $image:$env (HTTP $_status)."
    exit 1
  fi
  echo "  ✓ $ACR_HOST/$image:$env"
}

# Retag an image in ACR: source tag → env tag (manifest copy, no layer download)
# Uses skopeo when available; falls back to curl + Docker Registry v2 API on Windows or when skopeo is absent.
if [[ "$_tag_tool" == "skopeo" ]]; then
  acr_tag () {
    local image=$1
    if ! skopeo copy \
        --src-creds "$ACR_USER:$ACR_PASS" \
        --dest-creds "$ACR_USER:$ACR_PASS" \
        "docker://$ACR_HOST/$image:$tag" \
        "docker://$ACR_HOST/$image:$env"; then
      echo "ERROR: skopeo failed to tag $image ($tag → $env)."
      exit 1
    fi
    acr_verify "$image"
  }
else
  acr_tag () {
    local image=$1
    local _accept="application/vnd.docker.distribution.manifest.list.v2+json,application/vnd.docker.distribution.manifest.v2+json,application/vnd.oci.image.index.v1+json,application/vnd.oci.image.manifest.v1+json"

    # Fetch the content-type of the source manifest
    local _ctype
    _ctype=$(curl -sf -u "$ACR_USER:$ACR_PASS" \
      -I -H "Accept: $_accept" \
      "https://$ACR_HOST/v2/$image/manifests/$tag" \
      | grep -i "^content-type:" | awk '{print $2}' | tr -d '\r')

    if [[ -z "$_ctype" ]]; then
      echo "ERROR: Could not fetch manifest for $image:$tag — image may not exist in ACR."
      exit 1
    fi

    # Fetch the source manifest body
    local _manifest
    _manifest=$(curl -sf -u "$ACR_USER:$ACR_PASS" \
      -H "Accept: $_accept" \
      "https://$ACR_HOST/v2/$image/manifests/$tag")

    # Push the manifest under the new tag (no layer download)
    local _put_status
    _put_status=$(curl -sf -o /dev/null -w "%{http_code}" -u "$ACR_USER:$ACR_PASS" \
      -X PUT \
      -H "Content-Type: $_ctype" \
      -d "$_manifest" \
      "https://$ACR_HOST/v2/$image/manifests/$env")
    if [[ "$_put_status" != "201" && "$_put_status" != "200" ]]; then
      echo "ERROR: Failed to tag $image ($tag → $env) — ACR returned HTTP $_put_status."
      echo "       Ensure your account has the AcrPush role or that admin credentials are enabled."
      exit 1
    fi

    acr_verify "$image"
  }
fi

# Get current pod counts for all services (25 total - oracle not in dev)

# Stateless Services (10 services)
podsNginx=$(getPods nginx deployment $env)
podsEditor=$(getPods editor deployment $env)
podsSubscriber=$(getPods subscriber deployment $env)
podsNginxEditor=$(getPods nginx-editor deployment $env)
podsNginxSubscriber=$(getPods nginx-subscriber deployment $env)
podsCharts=$(getPods charts-api deployment $env)
podsApi=$(getPods api statefulset $env)
podsApiServices=$(getPods api-services deployment $env)
podsCorenlp=$(getPods corenlp deployment $env)
podsNLP=$(getPods nlp-service deployment $env)
podsFFmpeg=$(getPods ffmpeg-service deployment $env)
podsTranscription=$(getPods transcription-service deployment $env)
podsExtractQuotes=$(getPods extract-quotes-service deployment $env)

# Kafka Consumers - Stateless (7 services)
podsFolderCollection=$(getPods folder-collection-service deployment $env)
podsContent=$(getPods content-service deployment $env)
podsIndexing=$(getPods indexing-service deployment $env)
if [[ "$env" != "dev" ]]; then
  podsIndexingCloud=$(getPods indexing-service-cloud deployment $env)
fi
podsEventHandler=$(getPods event-handler-service deployment $env)
podsNotification=$(getPods notification-service deployment $env)
podsReporting=$(getPods reporting-service deployment $env)
podsChesRetry=$(getPods ches-retry-service deployment $env)
podsAutoClipper=$(getPods auto-clipper-service deployment $env)

# Kafka Consumers - Single-Instance (4 services)
podsScheduler=$(getPods scheduler-service deployment $env)
podsFileMonitor=$(getPods filemonitor-service deployment $env)
podsSyndication=$(getPods syndication-service deployment $env)
podsImage=$(getPods image-service deployment $env)

# Supporting Services
podsKowl=$(getPods kowl deployment $env)

# Tag all images in ACR ($tag → $env)
echo "Tagging images in ACR ($ACR_HOST): $tag → $env"

# Stateless Services
acr_tag nginx
acr_tag editor
acr_tag subscriber
acr_tag charts-api
acr_tag api
acr_tag corenlp
acr_tag nlp-service
acr_tag ffmpeg-service
acr_tag transcription-service
acr_tag extract-quotes-service

# Kafka Consumers (Stateless)
acr_tag folder-collection-service
acr_tag content-service
acr_tag indexing-service
acr_tag event-handler-service
acr_tag notification-service
acr_tag reporting-service
acr_tag ches-retry-service
acr_tag auto-clipper-service

# Kafka Producers (Single-Instance)
acr_tag scheduler-service
acr_tag filemonitor-service
acr_tag syndication-service
acr_tag image-service

# Stop everything
./stop.sh $env

# Start everything
echo "Scaling services back to original replica counts"

# Stateless Services
scale api $podsApi statefulset $env
scale api-services $podsApiServices deployment $env

# Wait until the API is running
oc rollout status statefulset/api --timeout=10m -n 9b301c-$env
oc rollout status deployment/api-services --timeout=10m -n 9b301c-$env

scale nginx $podsNginx deployment $env
scale editor $podsEditor deployment $env
scale subscriber $podsSubscriber deployment $env
scale nginx-editor $podsNginxEditor deployment $env
scale nginx-subscriber $podsNginxSubscriber deployment $env
scale charts-api $podsCharts deployment $env
scale corenlp $podsCorenlp deployment $env
scale nlp-service $podsNLP deployment $env
scale ffmpeg-service $podsFFmpeg deployment $env
scale transcription-service $podsTranscription deployment $env
scale extract-quotes-service $podsExtractQuotes deployment $env

# Kafka Consumers
scale folder-collection-service $podsFolderCollection deployment $env
scale content-service $podsContent deployment $env
scale indexing-service $podsIndexing deployment $env
if [[ "$env" != "dev" ]]; then
  scale indexing-service-cloud $podsIndexingCloud deployment $env
fi
scale event-handler-service $podsEventHandler deployment $env
scale notification-service $podsNotification deployment $env
scale reporting-service $podsReporting deployment $env
scale ches-retry-service $podsChesRetry deployment $env
scale auto-clipper-service $podsAutoClipper deployment $env

# Kafka Producers (Single-Instance)
scale scheduler-service $podsScheduler deployment $env
scale filemonitor-service $podsFileMonitor deployment $env
scale syndication-service $podsSyndication deployment $env
scale image-service $podsImage deployment $env

# Supporting Services
# scale oracle $podsOracle deployment $env  # Not deployed in dev
scale kowl $podsKowl deployment $env

echo "Deployment complete!"
