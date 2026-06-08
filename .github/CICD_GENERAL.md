# CI/CD General

These workflows build TNO images, push them to ACR, and roll workloads out to
OpenShift.

For the full deployment flow and changed-component detection rules, see
[DEPLOYMENT.md](./DEPLOYMENT.md).

For adding a normal .NET service to deployment, see
[ADDING_SERVICE.md](./ADDING_SERVICE.md).

## Main Workflows

`auto-deploy.yml` runs on selected pushes:

- `dev` deploys to the dev environment.
- `master` deploys to the test environment.

It calls `deploy-all.yml` with `deploy_scope: changed`, so only affected
components are built and deployed.

`deploy-all.yml` is the orchestrated deployment workflow. It can also be run
manually from the GitHub Actions UI with:

- `environment`: `dev` or `test`
- `deploy_scope`: `changed` or `all`
- `skip_db_migration`: `true` or `false`

Production is not wired into this flow yet.

## Current Notes

- DB migration runs in changed mode when `libs/net/dal/**` changes.
- API deployment updates both `deployment/api-services` and `statefulset/api`.
- CronJobs are not handled by `deploy-all.yml`; they still use their own
  workflows.
- Elasticsearch migration is not part of `deploy-all.yml` yet because dev uses
  OpenShift-hosted Elasticsearch while test and prod use cloud Elasticsearch.

## Shared Actions

These local actions hold the common deployment pieces used by the newer flows:

- `actions/build-scan-push-image`: builds an image, runs Trivy, backs up the
  existing ACR tag, and pushes the new image tag. ACR login, backup, and push
  operations retry with increasing waits.
- `actions/deploy-openshift-workload`: applies kustomize, restarts the target
  workload, waits for rollout, and can update the API StatefulSet when needed.
- `actions/install-oc`: installs a pinned `oc` client directly from the
  OpenShift mirror. This avoids the Node 20 and cache warnings from
  `redhat-actions/openshift-tools-installer@v1`.
- `actions/openshift-login`: logs in to OpenShift with retry support and
  increasing waits.

DB migration also uses `actions/install-oc` and `actions/openshift-login`.
Its image build and ACR push steps are still inside
`_reusable-db-migration-cd.yml`.
