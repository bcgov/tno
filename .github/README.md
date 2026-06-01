# GitHub Actions

These workflows build MMI images, push them to ACR, and roll workloads out to
OpenShift.

## Deploy Entry Points

`auto-deploy.yml` runs on selected pushes. It calls `deploy-all.yml` with
`deploy_scope: changed`, so a push only builds and deploys the parts touched by
that change.

`deploy-all.yml` is the orchestrated deploy workflow. It supports two scopes:

- `changed`: deploy the affected components only.
- `all`: build and deploy every supported API, service, and frontend image.

This flow currently supports `dev` and `test`. Production deploys are not wired
into it yet.

The rollout order is fixed:

```text
Detect Deploy Targets
-> Build Images
-> DB Migration
-> Deploy API
-> Deploy Services
-> Deploy Frontend
-> Deployment Summary
```

Builds, service deploys, and frontend deploys use matrix jobs. Items inside the
same matrix can run in parallel, but the phases still wait on each other through
`needs`.

## Change Detection

`detect_deploy_changes.py` decides what `changed` deploys should touch.

- `api/**`: build and deploy the API.
- `app/editor/**`: build and deploy editor.
- `app/subscriber/**`: build and deploy subscriber.
- `services/net/<service>/**`: build and deploy that service.
- `libs/net/**`: build and deploy the API plus all .NET services.
- `openshift/**`: restart the matching OpenShift workloads.

`db/postgres/**` is not part of auto-deploy detection.

## DB Migration

In `changed` mode, DB migration only runs when `libs/net/dal/**` changes.

In `all` mode, DB migration runs unless `skip_db_migration` is `true`.

The migration job builds the EF migration bundle from `libs/net` and runs it as
a temporary OpenShift pod.

## Elasticsearch Migration

`deploy-all.yml` does not run Elasticsearch migrations yet. Dev uses the
OpenShift-hosted Elasticsearch instance; test and prod use cloud Elasticsearch.
The migration code and environment wiring need another pass before this belongs
in the orchestrated deploy flow.

If that setup is standardized later, add Elastic migration by:

- Adding an Elastic migration output in `detect_deploy_changes.py`.
- Adding a `deploy-all.yml` job that calls `_reusable-elastic-migration-cd.yml`.
- Passing the right `elastic_secret_name`, `indexing_configmap`, and
  `elastic_apikey_enabled` values per environment.
- Placing the job before API/service/frontend rollout if app pods depend on it.

## API StatefulSet

The API deploy step updates both `deployment/api-services` and
`statefulset/api`. This keeps the StatefulSet API pods on the same API image as
the existing deployment.

## Manual Deploy

Once `deploy-all.yml` exists on the default branch, it can be run from the
GitHub Actions UI.

Inputs:

- `environment`: `dev` or `test`
- `deploy_scope`: `changed` or `all`
- `skip_db_migration`: `true` or `false`

`prod` is not available in this workflow yet.

Use `deploy_scope: all` carefully. It builds and deploys every supported
component.
