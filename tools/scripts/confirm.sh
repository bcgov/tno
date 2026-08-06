#!/bin/bash

# Prompt for confirmation before a destructive operation.
#
# Usage: confirm.sh "<warning message>" [skip]
#   skip - any non-empty value bypasses the prompt (non-interactive use).
#
# Exits 0 when confirmed, 1 when declined or when there is no terminal to ask.

message="$1"
skip="$2"

if [[ -n "$skip" ]]; then
  exit 0
fi

echo ""
echo "WARNING: $message"

if [[ ! -t 0 ]]; then
  echo "Aborted: no terminal to confirm on, pass y=1 to proceed without prompting."
  exit 1
fi

read -p "Enter 'y' to continue: " varContinue
if [[ "$varContinue" != "y" && "$varContinue" != "Y" ]]; then
  echo "Aborted."
  exit 1
fi
