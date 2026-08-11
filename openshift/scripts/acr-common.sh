#!/bin/bash
# Shared helpers for the ACR image scripts (build/push/pull/tag).
# Resolves a short component name (n=) to its image name, Dockerfile, and build context,
# mirroring the OpenShift BuildConfigs under openshift/kustomize/**/build.
#
# Overrides (env vars):
#   ACR_REGISTRY - registry host (default: bcgov-c4awhwfpcremdbga.azurecr.io)
#   ACR_NAME     - registry name for az commands (default: bcgov)
#   IMAGE        - image repository name (skips the mapping)
#   DOCKERFILE   - Dockerfile path (skips the mapping; relative to repo root)
#   CONTEXT      - build context (skips the mapping; relative to repo root)
#
# Only build.sh cares about the environment; push/pull/tag resolve the image name alone, which
# is the same for every variant.

ACR_REGISTRY=${ACR_REGISTRY:-bcgov-c4awhwfpcremdbga.azurecr.io}
ACR_NAME=${ACR_NAME:-bcgov}

# Repo root (scripts live in openshift/scripts).
REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"

# resolve_image <name> [environment]
# Sets IMAGE, DOCKERFILE, CONTEXT for the component. The environment selects which Dockerfile
# variant is used and defaults to 'prod' - these images are destined for the cluster, so the
# default must always match the BuildConfigs. Components map as follows:
#
#   component  image          prod Dockerfile (default)   local Dockerfile            context
#   api        api            api/net/Dockerfile.openshift api/net/Dockerfile          .
#   editor     editor         app/editor/Dockerfile.nginx  app/editor/Dockerfile       app/editor
#   subscriber subscriber     app/subscriber/Dockerfile.nginx app/subscriber/Dockerfile app/subscriber
#   charts     charts-api     api/node/Dockerfile.open     api/node/Dockerfile.local   api/node
#   db-migration db-migration libs/net/Dockerfile          (same for both)             libs/net
#   <name>     <name>-service services/net/<name>/Dockerfile (same for both)           .
#
# The 'local' variants are the ones docker-compose builds for development; they are not
# deployable to OpenShift (dev servers, no nginx runtime, wrong base images).
resolve_image() {
  local name=$1
  local environment=${2:-prod}
  if [ -z "$name" ]; then
    echo "ERROR: a component name is required (e.g. n=automation)." >&2
    return 1
  fi

  case "$environment" in
    prod|local) ;;
    *)
      echo "ERROR: environment must be 'prod' or 'local' (got '$environment')." >&2
      return 1
      ;;
  esac

  case "$name" in
    api)
      IMAGE=${IMAGE:-api}
      CONTEXT=${CONTEXT:-.}
      if [ "$environment" = "local" ]; then
        DOCKERFILE=${DOCKERFILE:-api/net/Dockerfile}
      else
        DOCKERFILE=${DOCKERFILE:-api/net/Dockerfile.openshift}
      fi
      ;;
    editor)
      IMAGE=${IMAGE:-editor}
      CONTEXT=${CONTEXT:-app/editor}
      if [ "$environment" = "local" ]; then
        DOCKERFILE=${DOCKERFILE:-app/editor/Dockerfile}
      else
        DOCKERFILE=${DOCKERFILE:-app/editor/Dockerfile.nginx}
      fi
      ;;
    subscriber)
      IMAGE=${IMAGE:-subscriber}
      CONTEXT=${CONTEXT:-app/subscriber}
      if [ "$environment" = "local" ]; then
        DOCKERFILE=${DOCKERFILE:-app/subscriber/Dockerfile}
      else
        DOCKERFILE=${DOCKERFILE:-app/subscriber/Dockerfile.nginx}
      fi
      ;;
    charts)
      IMAGE=${IMAGE:-charts-api}
      CONTEXT=${CONTEXT:-api/node}
      if [ "$environment" = "local" ]; then
        DOCKERFILE=${DOCKERFILE:-api/node/Dockerfile.local}
      else
        DOCKERFILE=${DOCKERFILE:-api/node/Dockerfile.open}
      fi
      ;;
    db-migration)
      # An EF migrations bundle, not a service; the same Dockerfile in both environments.
      IMAGE=${IMAGE:-db-migration}
      DOCKERFILE=${DOCKERFILE:-libs/net/Dockerfile}
      CONTEXT=${CONTEXT:-libs/net}
      ;;
    *)
      if [ -f "$REPO_ROOT/services/net/$name/Dockerfile" ]; then
        # The services have a single Dockerfile; compose and the BuildConfigs both use it.
        IMAGE=${IMAGE:-$name-service}
        DOCKERFILE=${DOCKERFILE:-services/net/$name/Dockerfile}
        CONTEXT=${CONTEXT:-.}
      else
        echo "ERROR: unknown component '$name' (no services/net/$name/Dockerfile)." >&2
        echo "       Override with IMAGE=, DOCKERFILE=, CONTEXT= for anything unusual." >&2
        return 1
      fi
      ;;
  esac
  return 0
}

require_docker() {
  if ! command -v docker &>/dev/null; then
    echo "ERROR: docker is not installed or not on the PATH." >&2
    return 1
  fi
}

require_az() {
  if ! command -v az &>/dev/null; then
    echo "ERROR: az (Azure CLI) is not installed. https://learn.microsoft.com/cli/azure/install-azure-cli" >&2
    return 1
  fi
}

# True when docker already holds a working credential for the registry - e.g. a normal
# 'docker login', or a token from 'az acr login --expose-token' pasted from Cloud Shell on a
# machine where 'az login' itself is blocked (Conditional Access requires a registered device).
# Probes the registry's oauth2 endpoint with the stored refresh token; checks the snap docker
# config path too, since snap confinement keeps it out of ~/.docker.
docker_acr_logged_in() {
  local cfg token code
  for cfg in "${DOCKER_CONFIG:-$HOME/.docker}/config.json" "$HOME/snap/docker/current/.docker/config.json"; do
    [ -f "$cfg" ] || continue
    token=$(python3 -c 'import json,sys; print(json.load(open(sys.argv[1])).get("auths",{}).get(sys.argv[2],{}).get("identitytoken",""))' "$cfg" "$ACR_REGISTRY" 2>/dev/null) || continue
    [ -n "$token" ] || continue
    code=$(curl -s -o /dev/null -w '%{http_code}' --max-time 10 -X POST "https://$ACR_REGISTRY/oauth2/token" \
      --data-urlencode "grant_type=refresh_token" \
      --data-urlencode "service=$ACR_REGISTRY" \
      --data-urlencode "scope=registry:catalog:*" \
      --data-urlencode "refresh_token=$token") || continue
    [ "$code" = "200" ] && return 0
  done
  return 1
}

# Login to ACR (idempotent; refreshes the docker credential). Tries, in order:
#   1. an existing valid docker credential (skips az entirely),
#   2. ACR_USERNAME/ACR_PASSWORD env vars (the service-principal creds CI uses),
#   3. az acr login, prompting a device login when the az session is missing/expired.
acr_login() {
  if docker_acr_logged_in; then
    echo "Using existing docker credential for $ACR_REGISTRY"
    return 0
  fi
  if [ -n "${ACR_USERNAME:-}" ] && [ -n "${ACR_PASSWORD:-}" ]; then
    printf '%s' "$ACR_PASSWORD" | docker login "$ACR_REGISTRY" -u "$ACR_USERNAME" --password-stdin
    return
  fi
  require_az || return 1
  if ! az acr login --name "$ACR_NAME" 2>/dev/null; then
    echo "ACR login failed; attempting 'az login' first..."
    az login || return 1
    az acr login --name "$ACR_NAME" || return 1
  fi
}
