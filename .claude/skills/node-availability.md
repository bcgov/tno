# Skill: Node Availability In CLI Sessions

## Problem Pattern

- `node: command not found` or `npm: command not found` in automation/tool shells.
- `node` resolves, but to the wrong major version (the web app needs Node 26).

## Root Cause In This Environment

- Node is installed via `nvm` under `~/.nvm`.
- `~/.bashrc` has an early return for non-interactive shells:
  - `case $- in *i*) ;; *) return;; esac`
- Tool-run shell commands are non-interactive, so `.bashrc` returns before loading `nvm`.
- Result: PATH does not include `~/.nvm/versions/node/<version>/bin`, or it holds the default
  version rather than the one a project needs.

## Quick Verification

```bash
command -v node && node --version
command -v npm && npm --version
```

If these are missing, compare with an interactive shell:

```bash
bash -ic 'command -v node; node --version'
```

## Reliable Fix For Commands

Source `nvm` and pick the version in the same command:

```bash
# Web app (src/app/): Node 26 and npm, version read from src/app/.nvmrc
export NVM_DIR="$HOME/.nvm" && . "$NVM_DIR/nvm.sh" && cd src/app && nvm use >/dev/null && npm run build
```

## Notes

- `src/app/` (editor and subscriber designs) is an npm project on Node 26 (`src/app/.nvmrc`,
  `engines` in `src/app/package.json`). Install with `npm ci`; never use Yarn there.
- `src/charts-api` (charts API) is separate and still uses Node 18 with Yarn 3:
  `nvm use 18 >/dev/null && yarn build` from `src/charts-api`.
- If `nvm use` reports the version is not installed, run `nvm install` in `src/app/`.
