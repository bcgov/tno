#!/bin/bash
# Build verification — runs when Claude Code stops.
# Builds the projects touched by uncommitted changes. On failure it exits 2 with the
# errors on stderr, which blocks the stop and feeds the errors back to Claude to fix.
# When the stop is already a stop-hook continuation (stop_hook_active in the payload),
# it exits 1 instead — the failure is shown to the user without blocking, so a build
# Claude cannot fix does not loop forever.

PAYLOAD=$(cat)
STOP_ACTIVE=$(printf '%s' "$PAYLOAD" | python3 -c '
import json, sys
try:
    print("true" if json.load(sys.stdin).get("stop_hook_active") else "false")
except Exception:
    print("false")
' 2>/dev/null || echo false)

REPO_ROOT="$(git rev-parse --show-toplevel 2>/dev/null)" || exit 0
CHANGED=$(git -C "$REPO_ROOT" status --porcelain 2>/dev/null | awk '{print $2}')

if [ -z "$CHANGED" ]; then
  exit 0
fi

FAILED=""
LOG="$(mktemp)"
trap 'rm -f "$LOG"' EXIT

# run_build <name> <dir> <cmd...> — on failure, emit the tail of the build output to stderr.
run_build() {
  local name="$1" dir="$2"
  shift 2
  if ! (cd "$dir" && "$@" >"$LOG" 2>&1); then
    FAILED="$FAILED $name"
    {
      echo ""
      echo "=== $name build failed (last 60 lines) ==="
      tail -n 60 "$LOG"
    } >&2
  fi
}

if echo "$CHANGED" | grep -qE "^app/subscriber/"; then
  run_build "subscriber" "$REPO_ROOT/app/subscriber" yarn build
fi

if echo "$CHANGED" | grep -qE "^app/editor/"; then
  run_build "editor" "$REPO_ROOT/app/editor" yarn build
fi

if echo "$CHANGED" | grep -qE "^(libs/net|api/net|services/net)/"; then
  run_build ".NET solution" "$REPO_ROOT" dotnet build
fi

if [ -z "$FAILED" ]; then
  exit 0
fi

echo "" >&2
echo "Build verification failed:$FAILED. Fix the errors above before stopping." >&2

if [ "$STOP_ACTIVE" = "true" ]; then
  # Already continuing because of this hook — report to the user instead of blocking again.
  exit 1
fi
exit 2
