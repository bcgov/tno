#!/bin/bash
# Build verification — runs when Claude Code stops.
# Detects changed files via git status and builds affected projects.
# Non-zero exit signals Claude Code to resume and fix errors.

REPO_ROOT="$(git rev-parse --show-toplevel 2>/dev/null)" || exit 0
CHANGED=$(git -C "$REPO_ROOT" status --porcelain 2>/dev/null | awk '{print $2}')

if [ -z "$CHANGED" ]; then
  exit 0
fi

EXIT_CODE=0

if echo "$CHANGED" | grep -qE "^app/subscriber/"; then
  echo ""
  echo "=== Building subscriber app ==="
  (cd "$REPO_ROOT/app/subscriber" && yarn build 2>&1) || EXIT_CODE=1
fi

if echo "$CHANGED" | grep -qE "^app/editor/"; then
  echo ""
  echo "=== Building editor app ==="
  (cd "$REPO_ROOT/app/editor" && yarn build 2>&1) || EXIT_CODE=1
fi

if echo "$CHANGED" | grep -qE "^(libs/net|api/net|services/net)/"; then
  echo ""
  echo "=== Building .NET solution ==="
  (cd "$REPO_ROOT" && dotnet build 2>&1) || EXIT_CODE=1
fi

exit $EXIT_CODE
