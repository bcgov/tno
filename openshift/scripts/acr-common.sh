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

ACR_REGISTRY=${ACR_REGISTRY:-bcgov-c4awhwfpcremdbga.azurecr.io}
ACR_NAME=${ACR_NAME:-bcgov}

# Repo root (scripts live in openshift/scripts).
REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"

# resolve_image <name>
# Sets IMAGE, DOCKERFILE, CONTEXT for the component. Components map as follows:
#   <name> with services/net/<name>/Dockerfile -> <name>-service (context: repo root)
#   api        -> api (api/net/Dockerfile.openshift, context: repo root)
#   editor     -> editor (app/editor/Dockerfile.nginx, context: app/editor)
#   subscriber -> subscriber (app/subscriber/Dockerfile.nginx, context: app/subscriber)
#   charts     -> charts-api (api/node/Dockerfile.open, context: api/node)
resolve_image() {
  local name=$1
  if [ -z "$name" ]; then
    echo "ERROR: a component name is required (e.g. n=automation)." >&2
    return 1
  fi

  case "$name" in
    api)
      IMAGE=${IMAGE:-api}
      DOCKERFILE=${DOCKERFILE:-api/net/Dockerfile.openshift}
      CONTEXT=${CONTEXT:-.}
      ;;
    editor)
      IMAGE=${IMAGE:-editor}
      DOCKERFILE=${DOCKERFILE:-app/editor/Dockerfile.nginx}
      CONTEXT=${CONTEXT:-app/editor}
      ;;
    subscriber)
      IMAGE=${IMAGE:-subscriber}
      DOCKERFILE=${DOCKERFILE:-app/subscriber/Dockerfile.nginx}
      CONTEXT=${CONTEXT:-app/subscriber}
      ;;
    charts)
      IMAGE=${IMAGE:-charts-api}
      DOCKERFILE=${DOCKERFILE:-api/node/Dockerfile.open}
      CONTEXT=${CONTEXT:-api/node}
      ;;
    *)
      if [ -f "$REPO_ROOT/services/net/$name/Dockerfile" ]; then
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

# Login to ACR (idempotent; refreshes the docker credential). Requires an active az session -
# prompts a device login when the session is missing/expired.
acr_login() {
  require_az || return 1
  if ! az acr login --name "$ACR_NAME" 2>/dev/null; then
    echo "ACR login failed; attempting 'az login' first..."
    az login || return 1
    az acr login --name "$ACR_NAME" || return 1
  fi
}
