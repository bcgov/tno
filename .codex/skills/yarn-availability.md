# Skill: Yarn Availability In CLI Sessions

## Problem Pattern

- `yarn: command not found` in automation/tool shells.
- `node` and `corepack` may also be missing in the same session.

## Root Cause In This Environment

- Node/Yarn are installed via `nvm` under `~/.nvm`.
- `~/.bashrc` has an early return for non-interactive shells:
  - `case $- in *i*) ;; *) return;; esac`
- Tool-run shell commands are non-interactive, so `.bashrc` returns before loading `nvm`.
- Result: PATH does not include `~/.nvm/versions/node/<version>/bin`.

## Repository Fix Applied

- `~/.bashrc` now initializes `nvm` and PATH **before** the non-interactive early return.
- This makes `node`, `yarn`, and `corepack` available in automation shells (including Codex/Claude bash tool calls).
- Verified with:

```bash
command -v node && node --version
command -v yarn && yarn --version
```

Expected result: binaries resolve from `~/.nvm/versions/node/.../bin`.

## Quick Verification

```bash
echo "$PATH"
command -v node || true
command -v yarn || true
command -v corepack || true
```

If these are missing, compare with interactive shell:

```bash
bash -ic 'command -v node; command -v yarn; yarn --version'
```

## Reliable Fix For Commands

If a machine still has the old shell config, use one command that sources `nvm` before running Yarn:

```bash
export NVM_DIR="$HOME/.nvm" && . "$NVM_DIR/nvm.sh" && nvm use 18 >/dev/null && yarn --version
```

For app commands:

```bash
export NVM_DIR="$HOME/.nvm" && . "$NVM_DIR/nvm.sh" && nvm use 18 >/dev/null && yarn build
```

## Notes

- Repo expects Yarn 3 and Node 18 for app workspaces (`app/editor`, `app/subscriber`, `libs/npm/core`, `api/node`).
- Prefer Node 18 for app builds/tests (install/use `18.19.0` when required by local team standards).
- If Node defaults to a different major version, pin per command:

```bash
export NVM_DIR="$HOME/.nvm" && . "$NVM_DIR/nvm.sh" && nvm use 18 >/dev/null && yarn build
```
