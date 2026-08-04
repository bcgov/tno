#!/bin/bash
# Retag an image in the Azure Container Registry, or locally.
# Usage: tag.sh <component> <from-tag> <to-tag> [remote|local]
#   component - short name (automation, notification, api, editor, ...); see acr-common.sh
#   from-tag  - existing tag (e.g. latest)
#   to-tag    - new tag (e.g. dev, test, prod)
#   mode      - 'remote' (default): az acr import copies the manifest inside ACR - no image
#                         download, so both tags end up pointing at the same image. This is the
#                         same mechanism deploy.sh uses to promote tags between environments.
#               'local':  docker tag on the local image, for a build that has not been pushed.

set -euo pipefail
source "$(dirname "${BASH_SOURCE[0]}")/acr-common.sh"

name=${1-}
from=${2-}
to=${3-}
mode=${4-remote}

resolve_image "$name"

if [ -z "$from" ] || [ -z "$to" ]; then
  echo "ERROR: both a from-tag and a to-tag are required (e.g. make tag n=$name f=latest t=dev)." >&2
  exit 1
fi

case "$mode" in
  local)
    require_docker
    src="$ACR_REGISTRY/$IMAGE:$from"
    dst="$ACR_REGISTRY/$IMAGE:$to"
    if ! docker image inspect "$src" &>/dev/null; then
      echo "ERROR: $src does not exist locally. Build or pull it first." >&2
      exit 1
    fi
    docker tag "$src" "$dst"
    echo "Tagged $src -> $dst (local)"
    ;;
  remote)
    require_az
    acr_login
    echo "Retagging in ACR: $IMAGE:$from -> $IMAGE:$to"
    az acr import \
      --name "$ACR_NAME" \
      --source "$ACR_REGISTRY/$IMAGE:$from" \
      --image "$IMAGE:$to" \
      --force
    echo "Tagged $IMAGE:$to in ACR ($IMAGE:$from and $IMAGE:$to now point at the same image)."
    echo "Deploy: make deploy e=$to n=$IMAGE"
    ;;
  *)
    echo "ERROR: mode must be 'local' or 'remote' (got '$mode')." >&2
    exit 1
    ;;
esac
