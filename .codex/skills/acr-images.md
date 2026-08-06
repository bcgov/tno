# Skill: ACR Images (build / push / pull / tag)

How to build service images locally and move them to/from the Azure Container Registry (ACR),
which is where OpenShift deployments pull from.

## TL;DR — ship a service image to ACR and deploy it

Run from `./openshift`:

```bash
make build n=automation            # build linux/amd64, tagged for ACR (:latest)
make push  n=automation            # push to ACR (logs into ACR via az)
make tag   n=automation t=dev r=true   # retag latest -> dev INSIDE ACR (no download)
oc rollout restart deployment/automation-service -n 9b301c-dev
```

## The four targets (all run from `./openshift`; scripts in `./openshift/scripts`)

| Target | What it does | Args |
|---|---|---|
| `make build` | `docker build --platform linux/amd64` tagged `bcgov-c4awhwfpcremdbga.azurecr.io/<image>:<t>`, ready to push | `n=component`, `t=tag` (default `latest`) |
| `make push` | Push the local image to ACR (runs `az acr login` first; falls back to `az login`) | `n=`, `t=` |
| `make pull` | Pull an image from ACR to the local docker registry | `n=`, `t=` |
| `make tag` | Local: `docker tag` between two tags. Remote (`r=true`): `az acr import` copies the manifest inside ACR — no image download; this is how tags are promoted between environments (`latest` → `dev`/`test`/`prod`) | `n=`, `f=from` (default `latest`), `t=to`, `r=true` for remote |

## Component name resolution (`n=`)

Handled by `openshift/scripts/acr-common.sh`, mirroring the OpenShift BuildConfigs:

- Any `<name>` with `services/net/<name>/Dockerfile` → image `<name>-service`
  (e.g. `n=automation` → `automation-service`, `n=notification` → `notification-service`).
- `api` → `api` (`api/net/Dockerfile.openshift`), `editor` → `editor` (`app/editor/Dockerfile.nginx`),
  `subscriber` → `subscriber` (`app/subscriber/Dockerfile.nginx`), `charts` → `charts-api`.
- Anything unusual: override with env vars `IMAGE=`, `DOCKERFILE=`, `CONTEXT=`
  (also `ACR_REGISTRY=`, `ACR_NAME=` for a different registry).

## Prerequisites

- `docker`, `az` (Azure CLI). Login is IDIR-backed: `az login` then `az acr login --name bcgov`
  (push/pull/remote-tag attempt this automatically).
- Deployments pull environment tags (`:dev`, `:test`, `:prod`), while builds push `:latest` —
  promote with the remote tag (`make tag n=<x> f=latest t=dev r=true`), then restart the
  deployment: `oc rollout restart deployment/<image> -n 9b301c-<env>`.

## Gotchas

- **Always built `--platform linux/amd64`** — OpenShift nodes are amd64; an image built natively
  on Apple Silicon without the flag will not run in the cluster. `make build` handles this.
- `make build`/`push`/etc. at the **repo root** are docker-compose targets for local dev — the
  ACR targets only exist in `./openshift/Makefile`. Run them from `./openshift`.
- Remote tagging (`r=true`) uses `az acr import --force` — it overwrites the destination tag.
  That is the intended promote flow (same mechanism as `openshift/scripts/deploy.sh`).
- The BuildConfigs under `openshift/kustomize/**/build` do the same builds in-cluster from the
  `bcgov/tno.git` `dev` branch; these scripts are the local/manual path for images that are not
  yet in upstream `dev` or when iterating quickly.
- See `openshift/README.md` ("Build and Deploy to ACR") for the underlying raw commands.
