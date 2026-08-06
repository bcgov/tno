#!/usr/bin/env bash

set -euo pipefail

repo_root="$(git rev-parse --show-toplevel)"
cd "$repo_root"

env_file="$repo_root/.env"
global_key="KEPLER_GLOBAL_WORKTREE_FOLDER"

if [[ ! -f "$env_file" ]]; then
  touch "$env_file"
fi

get_env_value() {
  local key="$1"
  local file="$2"
  local line

  line="$(grep -E "^${key}=" "$file" 2>/dev/null || true)"
  if [[ -z "$line" ]]; then
    return 1
  fi

  printf '%s' "${line#*=}"
}

set_env_value() {
  local key="$1"
  local value="$2"
  local file="$3"

  if grep -q -E "^${key}=" "$file"; then
    sed -i "s|^${key}=.*|${key}=${value}|" "$file"
  else
    if [[ -s "$file" ]]; then
      printf '\n%s=%s\n' "$key" "$value" >> "$file"
    else
      printf '%s=%s\n' "$key" "$value" >> "$file"
    fi
  fi
}

global_folder="$(get_env_value "$global_key" "$env_file" || true)"

if [[ -z "$global_folder" || ! -d "$global_folder" ]]; then
  echo "The global Kepler worktree folder is not configured in $env_file"
  read -r -p "Enter the full path to the global Kepler worktree folder: " global_folder

  if [[ -z "$global_folder" || ! -d "$global_folder" ]]; then
    echo "Invalid folder '$global_folder'."
    exit 1
  fi

  set_env_value "$global_key" "$global_folder" "$env_file"
  echo "Saved ${global_key} in $env_file"
fi

declare -a worktree_paths=()
declare -A seen=()

add_worktrees_from_folder() {
  local folder="$1"

  if [[ ! -d "$folder" ]]; then
    return
  fi

  local dir
  for dir in "$folder"/*; do
    if [[ -d "$dir" && -e "$dir/.git" && -z "${seen[$dir]:-}" ]] && git -C "$dir" rev-parse --is-inside-work-tree >/dev/null 2>&1; then
      worktree_paths+=("$dir")
      seen["$dir"]=1
    fi
  done
}

add_worktrees_from_folder "$repo_root/.worktrees"
add_worktrees_from_folder "$global_folder"

if [[ ${#worktree_paths[@]} -eq 0 ]]; then
  echo "No worktrees were found in '$repo_root/.worktrees' or '$global_folder'."
  exit 1
fi

echo ""
echo "Available worktrees:"
for i in "${!worktree_paths[@]}"; do
  branch_name="$(git -C "${worktree_paths[$i]}" branch --show-current 2>/dev/null || true)"
  if [[ -n "$branch_name" ]]; then
    printf '%2d) %s (%s)\n' "$((i + 1))" "${worktree_paths[$i]}" "$branch_name"
  else
    printf '%2d) %s\n' "$((i + 1))" "${worktree_paths[$i]}"
  fi
done

read -r -p "Select target worktree number: " selected_index

if [[ ! "$selected_index" =~ ^[0-9]+$ ]] || (( selected_index < 1 || selected_index > ${#worktree_paths[@]} )); then
  echo "Invalid selection '$selected_index'."
  exit 1
fi

target_path="${worktree_paths[$((selected_index - 1))]}"

source_branch="master"
read -r -p "Enter source branch to copy .env files from [master]: " source_branch_input
if [[ -n "$source_branch_input" ]]; then
  source_branch="$source_branch_input"
fi

source_path=""

current_branch="$(git -C "$repo_root" branch --show-current 2>/dev/null || true)"
if [[ "$current_branch" == "$source_branch" ]]; then
  source_path="$repo_root"
fi

if [[ -z "$source_path" ]]; then
  current_worktree=""
  while IFS= read -r line; do
    if [[ "$line" == worktree\ * ]]; then
      current_worktree="${line#worktree }"
      continue
    fi

    if [[ "$line" == "branch refs/heads/$source_branch" ]]; then
      source_path="$current_worktree"
      break
    fi
  done < <(git worktree list --porcelain)
fi

if [[ -z "$source_path" || ! -d "$source_path" ]]; then
  echo "Could not locate a local '$source_branch' worktree automatically."
  read -r -p "Enter the full path to your '$source_branch' worktree (or press Enter to cancel): " source_path

  if [[ -z "$source_path" ]]; then
    echo "Cancelled."
    exit 1
  fi

  if [[ ! -e "$source_path/.git" ]] || ! git -C "$source_path" rev-parse --is-inside-work-tree >/dev/null 2>&1; then
    echo "Invalid worktree path '$source_path'."
    echo "Tip: create one with: git worktree add \"$global_folder/$source_branch\" $source_branch"
    exit 1
  fi
fi

echo ""
echo "Copying .env files from branch '$source_branch': $source_path"
echo "Copying .env files to:   $target_path"

copied_count=0
while IFS= read -r -d '' source_file; do
  relative_file="${source_file#"$source_path"/}"
  target_file="$target_path/$relative_file"

  mkdir -p "$(dirname "$target_file")"
  cp "$source_file" "$target_file"
  copied_count=$((copied_count + 1))
done < <(find "$source_path" -type f \( -name '.env' -o -name '*.env' \) -print0)

# Ensure the global worktree folder is persisted in the target root .env,
# even if root .env was copied from another branch and overwrote this key.
target_env_file="$target_path/.env"
if [[ ! -f "$target_env_file" ]]; then
  touch "$target_env_file"
fi
set_env_value "$global_key" "$global_folder" "$target_env_file"

echo "Copied $copied_count .env file(s)."
echo ""
echo "Run this command to switch to the selected worktree:"
printf 'cd "%s"\n' "$target_path"
