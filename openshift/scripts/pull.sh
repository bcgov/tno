#!/bin/bash
# Pull an image from the Azure Container Registry to the local docker registry.
# Usage: pull.sh <component> [tag]
#   component - short name (automation, notification, api, editor, ...); see acr-common.sh
#   tag       - image tag (default: latest)

set -euo pipefail
source "$(dirname "${BASH_SOURCE[0]}")/acr-common.sh"

name=${1-}
tag=${2-latest}

require_docker
resolve_image "$name"

image="$ACR_REGISTRY/$IMAGE:$tag"

acr_login
echo "Pulling $image"
docker pull "$image"

echo "Pulled $image"
