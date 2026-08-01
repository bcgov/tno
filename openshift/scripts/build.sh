#!/bin/bash
# Build a linux/amd64 image tagged for the Azure Container Registry, ready to push.
# Usage: build.sh <component> [tag]
#   component - short name (automation, notification, api, editor, ...); see acr-common.sh
#   tag       - image tag (default: latest)

set -euo pipefail
source "$(dirname "${BASH_SOURCE[0]}")/acr-common.sh"

name=${1-}
tag=${2-latest}

require_docker
resolve_image "$name"

image="$ACR_REGISTRY/$IMAGE:$tag"
echo "Building $image"
echo "  dockerfile: $DOCKERFILE"
echo "  context:    $CONTEXT"

# OpenShift nodes are amd64; always build for that platform so images from ARM machines
# (e.g. Apple Silicon) run in the cluster.
cd "$REPO_ROOT"
docker build --platform linux/amd64 -t "$image" -f "$DOCKERFILE" "$CONTEXT"

echo "Built $image"
echo "Next: make push n=$name t=$tag"
