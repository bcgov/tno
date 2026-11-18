# MMI web application

One React + Vite project that contains both Media Monitoring Insights designs:

| Design | Users | Source | HTML document |
| --- | --- | --- | --- |
| `editor` | editors and administrators | `src/editor` | `editor.html` |
| `subscriber` | subscribers | `src/subscriber` | `subscriber.html` |

Code used by both designs lives in `src/shared` (it replaced the `tno-core` npm package).
`src/shared` never imports a design, and the designs never import each other; the build fails
if either design's bundle contains the other's source.

A deployment presents exactly one design, chosen at runtime by the `APP_MODE` environment
variable (`editor` or `subscriber`). The same image runs both.

## Requirements

- Node.js 26 (`nvm use` reads `.nvmrc`)
- npm (Yarn is no longer used)

## Commands

```bash
npm ci                          # install
APP_MODE=editor npm run dev     # dev server on http://localhost:3000 (APP_MODE is required)
npm run build                   # build both designs into dist/
APP_MODE=subscriber npm run preview
npm run typecheck               # TypeScript 7 (tsgo), app and tooling configs
npm run lint                    # Oxlint, zero warnings
npm run check                   # Prettier (npm run format writes)
npm test                        # Vitest (projects: shared, editor, subscriber)
```

Imports use one alias, `@/` → `src/`, e.g. `@/shared`, `@/editor/store`.

## Docker

- `Dockerfile` — dev server used by docker-compose. The `editor` and `subscriber` services both
  build it and differ only in `APP_MODE`; `src/`, `public/` and the HTML documents are
  bind-mounted, so changes hot-reload.
- `Dockerfile.nginx` — production image: static files served by nginx on port 8080 as any UID.
  `nginx/start.sh` reads `APP_MODE` and starts `nginx/nginx.<APP_MODE>.conf`; a missing or
  unknown value exits 64, so a misconfigured pod never becomes ready. Build arg `APP_VERSION`
  is written to `/version.json`.

## Static files

`public/shared` is served at the site root for both designs. `public/editor` and
`public/subscriber` hold the files that differ (favicon, manifest, some assets) and are served
over it, so each design keeps exactly the URLs it had before the merge. Built, hashed files go
to `/static/`.
