#!/bin/bash
# Build every image, push it to ACR, and deploy the whole environment - what CI's deploy-all does,
# from a workstation.
#
# Usage: release.sh <environment> [tag] [components...]
#   environment - dev, test or prod.
#   tag         - image tag to build, push and promote (default: the short git commit, so every
#                 deployed image traces back to a commit).
#   components  - build only these (build.sh names: api, editor, charts, content, ...).
#                 Default: every image deploy.sh promotes that can be built here.
#
# Options (environment variables):
#   MIGRATE=1     - also build db-migration and run it before the deploy (the order CI uses).
#   DRY_RUN=1     - print every step without building, pushing or deploying.
#   YES=1         - skip the confirmation (required for test and prod otherwise).
#   ALLOW_DIRTY=1 - allow uncommitted changes (the images would not match any commit).
#
# Images that are not built (an unbuildable one such as nginx, or anything outside a given
# component list) are copied onto the tag inside ACR from what the environment runs now
# ('<environment>' tag), falling back to 'latest'. deploy.sh then promotes the tag for every
# image, so images that were not built keep running the same build.
#
# The image list comes from deploy.sh's own 'acr_tag <image>' lines, so the two stay in step.

set -euo pipefail
SCRIPTS="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPTS/acr-common.sh"

env=${1-}
case "$env" in
  dev|test|prod) ;;
  *) echo "Usage: release.sh <dev|test|prod> [tag] [components...]" >&2; exit 1 ;;
esac
shift
tag=${1-}
[ $# -gt 0 ] && shift
requested=("$@")

DRY_RUN=${DRY_RUN:-}
run() {
  if [ -n "$DRY_RUN" ]; then echo "  [dry run] $*"; else "$@"; fi
}

# The commit being released.
cd "$REPO_ROOT"
commit=$(git rev-parse --short=9 HEAD)
branch=$(git rev-parse --abbrev-ref HEAD)
if [ -n "$(git status --porcelain)" ] && [ -z "${ALLOW_DIRTY:-}" ]; then
  echo "ERROR: the working tree has uncommitted changes, so the images would not match commit $commit." >&2
  echo "       Commit or stash them, or set ALLOW_DIRTY=1." >&2
  exit 1
fi
tag=${tag:-$commit}

# contains <value> <list...> - exact membership, without a pipeline that pipefail could trip on.
contains() {
  local value=$1; shift
  local item
  for item in "$@"; do [ "$item" = "$value" ] && return 0; done
  return 1
}

# image -> build.sh component name (empty when it cannot be built from this repository here).
component_for() {
  case "$1" in
    nginx) echo "" ;;
    charts-api) echo "charts" ;;
    api|editor|subscriber|nginx-editor|nginx-subscriber|db-migration|elastic-migration) echo "$1" ;;
    *-service) echo "${1%-service}" ;;
    *) echo "" ;;
  esac
}

# Every image the full-environment deploy promotes.
mapfile -t images < <(grep -E '^acr_tag [a-z0-9-]+$' "$SCRIPTS/deploy.sh" | awk '{print $2}')
if [ ${#images[@]} -eq 0 ]; then
  echo "ERROR: found no 'acr_tag <image>' lines in deploy.sh." >&2
  exit 1
fi

# Decide what to build.
build=()
copy=()
for image in "${images[@]}"; do
  component=$(component_for "$image")
  if [ -n "$component" ] && { [ ${#requested[@]} -eq 0 ] || contains "$component" "${requested[@]}"; }; then
    build+=("$component")
  else
    copy+=("$image")
  fi
done
buildable=()
for image in "${images[@]}"; do
  component=$(component_for "$image")
  [ -n "$component" ] && buildable+=("$component")
done
for component in "${requested[@]}"; do
  if ! contains "$component" "${buildable[@]}"; then
    echo "ERROR: '$component' is not a buildable image that deploy.sh promotes (components: ${buildable[*]})." >&2
    exit 1
  fi
done
[ -n "${MIGRATE:-}" ] && build=(db-migration "${build[@]}")

echo "Release $branch @ $commit to $env with tag '$tag'"
echo "  build and push: ${build[*]}"
echo "  copy onto tag:  ${copy[*]:-(none)}"
echo "  migrations:     $([ -n "${MIGRATE:-}" ] && echo "db-migration runs before the deploy" || echo "not run (MIGRATE=1 to run db-migration)")"
echo "  then:           deploy.sh $env $tag (retag every image, stop and restart $env)"
[ -n "$DRY_RUN" ] && echo "  DRY RUN: nothing will be built, pushed or deployed."

if [ -z "$DRY_RUN" ] && [ -z "${YES:-}" ]; then
  if [ "$env" = "dev" ]; then
    read -r -p "Continue? [y/N] " answer
    [[ "$answer" =~ ^[Yy]$ ]] || { echo "Cancelled."; exit 1; }
  else
    read -r -p "This deploys to $env. Type '$env' to continue: " answer
    [ "$answer" = "$env" ] || { echo "Cancelled."; exit 1; }
  fi
fi

require_docker
if [ -z "$DRY_RUN" ]; then acr_login; fi

# Build and push.
built=()
for component in "${build[@]}"; do
  echo
  echo "=== $component ==="
  run "$SCRIPTS/build.sh" "$component" "$tag" prod
  run "$SCRIPTS/push.sh" "$component" "$tag"
  built+=("$component")
done

# Copy the images that were not built onto the tag: from what the environment runs, else latest.
for image in "${copy[@]}"; do
  echo
  echo "=== $image (not built) ==="
  source_tag=""
  for candidate in "$env" latest; do
    if docker manifest inspect "$ACR_REGISTRY/$image:$candidate" >/dev/null 2>&1; then source_tag=$candidate; break; fi
  done
  if [ -z "$source_tag" ]; then
    if [ -n "$DRY_RUN" ]; then
      echo "  [dry run] could not read $image:$env or $image:latest (not logged in to ACR?); a real run needs one of them."
      continue
    fi
    echo "ERROR: $ACR_REGISTRY/$image has neither a '$env' nor a 'latest' tag to copy onto '$tag'." >&2
    exit 1
  fi
  echo "Copying $image:$source_tag -> $image:$tag in ACR"
  run docker buildx imagetools create --tag "$ACR_REGISTRY/$image:$tag" "$ACR_REGISTRY/$image:$source_tag"
done

# Migrate, then deploy.
if [ -n "${MIGRATE:-}" ]; then
  echo
  echo "=== db-migration ($env) ==="
  run "$SCRIPTS/deploy.sh" "$env" "$tag" db-migration
fi
echo
echo "=== deploy $env ==="
run "$SCRIPTS/deploy.sh" "$env" "$tag"

echo
echo "Released $branch @ $commit to $env as '$tag'."
echo "  built: ${built[*]}"
echo "  copied: ${copy[*]:-(none)}"
