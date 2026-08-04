#!/bin/bash
# Build a linux/amd64 image tagged for the Azure Container Registry, ready to push.
# Usage: build.sh <component> [tag] [environment]
#   component   - short name (automation, notification, api, editor, ...); see acr-common.sh
#   tag         - image tag (default: latest)
#   environment - which Dockerfile variant to build (default: prod)
#                 'prod'  - the Dockerfile the OpenShift BuildConfig uses; what you push to ACR.
#                 'local' - the Dockerfile docker-compose builds for development. Not deployable.

set -euo pipefail
source "$(dirname "${BASH_SOURCE[0]}")/acr-common.sh"

name=${1-}
tag=${2-latest}
environment=${3-prod}

# warn_dotenv_leak
# The apps bake their configuration into the bundle at build time. The cluster builds from git,
# where the app `.env` does not exist (.gitignore excludes *.env), but a local build copies the
# developer's `.env` into the context - so a production image built here can carry development
# configuration that the same image built in OpenShift would not have.
warn_dotenv_leak() {
  # Only the app images bake their environment in at build time, and they all do it through the
  # `environment.env` rename. Everything else reads its configuration at runtime, so a `.env`
  # sitting in the context (the repo root has one for compose) is harmless there.
  grep -q "environment.env" "$REPO_ROOT/$DOCKERFILE" || return 0

  local dotenv="$REPO_ROOT/$CONTEXT/.env"
  [ -f "$dotenv" ] || return 0

  # Nothing to warn about when the context already excludes it.
  local dockerignore="$REPO_ROOT/$CONTEXT/.dockerignore"
  if [ -f "$dockerignore" ] && grep -qE '^[[:space:]]*\*?\.env[[:space:]]*$' "$dockerignore"; then
    return 0
  fi

  echo
  echo "WARNING: $CONTEXT/.env exists and will be copied into the build context."
  echo "         The OpenShift build has no such file, so this image may not match the cluster's."
  local active
  active=$(grep -vE '^[[:space:]]*(#|$)' "$dotenv" || true)
  if [ -n "$active" ]; then
    echo "         Values that would be baked into this image:"
    echo "$active" | sed 's/^/           /'
  fi
  echo "         Resolve by any of:"
  echo "           - create $CONTEXT/environment.env holding the production values"
  echo "             (the Dockerfile renames it over .env during the build)"
  echo "           - add '.env' to $CONTEXT/.dockerignore"
  echo "           - comment out the development-only values before building"
  echo
}

require_docker
resolve_image "$name" "$environment"

image="$ACR_REGISTRY/$IMAGE:$tag"
echo "Building $image"
echo "  environment: $environment"
echo "  dockerfile:  $DOCKERFILE"
echo "  context:     $CONTEXT"

if [ "$environment" = "prod" ]; then
  warn_dotenv_leak
else
  echo
  echo "WARNING: '$environment' builds the development Dockerfile. Do not push this image to ACR."
  echo
fi

# OpenShift nodes are amd64; always build for that platform so images from ARM machines
# (e.g. Apple Silicon) run in the cluster.
cd "$REPO_ROOT"
docker build --platform linux/amd64 -t "$image" -f "$DOCKERFILE" "$CONTEXT"

echo "Built $image"
if [ "$environment" = "prod" ]; then
  echo "Next: make push n=$name t=$tag"
fi
