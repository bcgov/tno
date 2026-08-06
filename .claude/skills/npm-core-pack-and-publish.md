# Skill: NPM Core Package (tno-core) Pack & Publish

## TL;DR — testing a local `tno-core` change in a running app

This is **the** supported workflow. Any time you edit `libs/npm/core/src/**` and want to see it
in the editor (or subscriber) app, run exactly this:

```bash
cd libs/npm/core
make pack n=editor        # build + pack a local .tgz and install it into app/editor
cd ../../..
make refresh n=editor     # stop, rebuild, and start the tno-editor container
```

Repeat both steps after every `tno-core` change. `make refresh n=editor` takes the editor
container down for a few minutes while it rebuilds; the API is unaffected.

**Do NOT shortcut this** by building `dist/` and copying it into the running container
(`docker cp dist/. tno-editor:/usr/app/node_modules/tno-core/dist/`). That appears to work but:

- it is silently lost the next time the container is rebuilt or recreated;
- it bypasses Yarn resolution, `package.json` metadata, and the packaged file set, so what you
  test is not what `make pack` ships;
- it leaves the app's `package.json`/lockfile untested against the packed artifact.

## What This Covers

- `libs/npm/core` builds the shared **`tno-core`** npm package.
- It is consumed by the frontend apps `app/editor` and `app/subscriber`.
- Two Make targets drive its lifecycle (run them from `libs/npm/core`):
  - `make pack n=<app>` — build + pack a local `.tgz` and install it into an app for local testing.
  - `make publish` — build + bump the version + publish to the public npm registry.
- Discover both with `make help` inside `libs/npm/core`.

## Prerequisites

- Node 18 + Yarn 3 must be available (see `yarn-availability`). If `yarn`/`node` are missing in a
  non-interactive shell, prefix the command:
  ```bash
  export NVM_DIR="$HOME/.nvm" && . "$NVM_DIR/nvm.sh" && nvm use 18 >/dev/null && make pack n=editor
  ```
- `make publish` additionally requires npm auth with publish rights to `tno-core`.

## Local Testing: `make pack`

Use this to try `tno-core` changes in `editor`/`subscriber` **without publishing**.

```bash
cd libs/npm/core
make pack n=editor       # or: make pack n=subscriber
```

The `n=` argument is **required** (`editor` or `subscriber`). Omitting it prints a usage reminder
and does nothing. `scripts/package.sh` then:

1. Runs `yarn run build` (compiles `src` → `dist`).
2. Packs `dist` into `tno-core-<version>.tgz`.
3. Moves the `.tgz` into the target app (`app/<n>`), handling both devcontainer
   (`/workspaces/tno/...`) and local checkout paths.
4. Clears the Yarn cache and reinstalls the published `tno-core` first, then the local `.tgz`
   (this reset is what makes repeated packs of the **same version** actually take effect).
5. Runs `yarn install` in the app so it now resolves `tno-core` from the local `.tgz`.

### Full local test loop

```bash
# 1. Edit libs/npm/core/src/...
cd libs/npm/core
make pack n=editor

# 2. Rebuild/restart the app container (from the repo root)
cd ../../..
make refresh n=editor

# 3. Verify the change in the running editor app
```

Repeat `make pack n=editor` + `make refresh n=editor` after each `tno-core` change. There is no
supported alternative for testing local `tno-core` changes — see the TL;DR warning about copying
`dist/` into a running container.

## Publishing: `make publish`

Use this to release a new version to npm (consumed by CI/other checkouts).

```bash
cd libs/npm/core
make publish
```

`scripts/publish.sh` then:

1. Ensures you are logged in (`yarn npm whoami`; runs `yarn npm login` if not).
2. Prints the current version and the registry's latest version.
3. Prompts for the new version — enter `m` (major), `i` (minor), `p` (patch), or an explicit
   version string; it runs `npm version <choice>` to bump `package.json`.
4. Sets the registry to `https://registry.npmjs.org` and runs `yarn npm publish --access public`.
5. Prints the published version.

After publishing, apps pick up the new version via their normal `yarn add tno-core@<version>` /
`yarn install`.

## Gotchas

- **`make pack` mutates the target app**: it rewrites `app/<n>/package.json` + lockfile to point at
  the local `.tgz` and drops a `tno-core-<version>.tgz` in the app folder. Do **not** commit those
  local-testing changes — revert them before opening a PR.
- `make pack` reinstalls the published `tno-core` before the local one, so it needs network access
  and the package must already exist on npm.
- `make pack` does **not** bump the version; only `make publish` does. Packing the same version
  repeatedly is fine because the script clears the cache.
- `make publish` is interactive (login + version prompt) and irreversible once a version is on npm —
  bump to a new version rather than republishing an existing one.
