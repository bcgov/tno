#!/bin/bash
# Push a locally built image to the Azure Container Registry.
# Usage: push.sh <component> [tag]
#   component - short name (automation, notification, api, editor, ...); see acr-common.sh
#   tag       - image tag (default: latest)

set -euo pipefail
source "$(dirname "${BASH_SOURCE[0]}")/acr-common.sh"

name=${1-}
tag=${2-latest}

require_docker
resolve_image "$name"

image="$ACR_REGISTRY/$IMAGE:$tag"

if ! docker image inspect "$image" &>/dev/null; then
  echo "ERROR: $image does not exist locally. Build it first: make build n=$name t=$tag" >&2
  exit 1
fi

acr_login
echo "Pushing $image"
docker push "$image"

echo "Pushed $image"
echo "Verify: az acr repository show-tags --name $ACR_NAME --repository $IMAGE --output table"
