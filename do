#!/usr/bin/env bash
# Bash 3.2+; run from any directory, including paths containing spaces.
set -eo pipefail
DO_ROOT="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
cd "$DO_ROOT"
# Prevent Git Bash from translating container paths and OpenShift arguments.
case "${OSTYPE:-}" in msys*|cygwin*) export MSYS_NO_PATHCONV=1 MSYS2_ARG_CONV_EXCL='*' ;; esac
# shellcheck source=src/tools/scripts/do/common.sh
source "$DO_ROOT/src/tools/scripts/do/common.sh"
# shellcheck source=src/tools/scripts/do/help.sh
source "$DO_ROOT/src/tools/scripts/do/help.sh"
# shellcheck source=src/tools/scripts/do/containers.sh
source "$DO_ROOT/src/tools/scripts/do/containers.sh"
# shellcheck source=src/tools/scripts/do/oc.sh
source "$DO_ROOT/src/tools/scripts/do/oc.sh"
# shellcheck source=src/tools/scripts/do/kafka.sh
source "$DO_ROOT/src/tools/scripts/do/kafka.sh"
# shellcheck source=src/tools/scripts/do/database.sh
source "$DO_ROOT/src/tools/scripts/do/database.sh"
group=${1:-help}
[[ $# == 0 ]] || shift
case "$group" in
  help|-h|--help) do_help "${1:-}" ;;
  docker|podman|oc)
    action=${1:-help}
    [[ $# == 0 ]] || shift
    case "$action" in help|-h|--help) do_help "$group"; exit ;; esac
    parse_options "$@"
    if [[ "$group" == oc ]]; then oc_action; else
      export CONTAINER_ENGINE="$group"
      container_action "$action" "$@"
    fi ;;
  db-*) action=$group; parse_options "$@"; database_action "$action" "$@" ;;
  setup|move|worktree)
    if [[ $# == 1 && ( "$1" == -h || "$1" == --help ) ]]; then do_help; exit 0; fi
    [[ $# == 0 ]] || die "$group takes no arguments"
    case "$group" in
      setup) python3 src/tools/scripts/move-env-files.py; bash src/tools/scripts/gen-env-files.sh; mkdir -p src/app/node_modules ;;
      move) python3 src/tools/scripts/move-env-files.py ;;
      worktree) bash src/tools/scripts/worktree.sh ;;
    esac ;;
  *) die "Unknown command '$group'. Run 'bash do help'." ;;
esac
