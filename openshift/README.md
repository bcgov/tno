# Openshift

This TNO solution is primarily hosted within Openshift.
Everything relevant and required by the solution is capture as "Infrastructure as Code" so that it is easily setup, configured, built, and deployed.
You can find all templates files and instructions for Openshift in this folder.

## Elasticsearch migration image and deployment

From the repository root, build and push the migration tool to ACR, then run it in DEV as described in the
[operations runbook](./kustomize/elastic-migration/README.md). How the tool works is in the
[tool README](../tools/elastic/migration/README.md).

```bash
make -C openshift build n=elastic-migration t=latest
make -C openshift push n=elastic-migration t=latest
make -C openshift deploy n=elastic-migration e=dev t=latest
```

The image commands also support `pull n=elastic-migration t=latest` and
`tag n=elastic-migration f=latest t=dev` (add `r=local` to tag locally).
Build uses `tools/elastic/migration/Dockerfile` with the repository root as its context.
The build option `e=prod` (the default) selects the Dockerfile variant, whereas deploy's
`e=dev|test|prod` selects the target namespace.

Deployment promotes the source tag to the environment tag and creates a one-shot Job in
`9b301c-<environment>`. It follows the logs, reports failure, and retains the Job for 24 hours.
Jobs are not automatically retried. An unqualified full-environment deployment does not run
migrations; request `n=elastic-migration` explicitly.

Database connection references are copied from the API StatefulSet, just as for `db-migration`.
Use `s=<secret>` to override the database credential secret. Elasticsearch defaults match the
migration workflow:

| Environment | ConfigMap | Secret | Authentication |
| --- | --- | --- | --- |
| dev | indexing-service | elastic | USERNAME / PASSWORD |
| test, prod | indexing-service-cloud | elastic-cloud | ApiKey |

The ConfigMap supplies `ELASTICSEARCH_URI`, `CONTENT_INDEX`, and `PUBLISHED_INDEX`.
For a different Elasticsearch target, set `ELASTIC_MIGRATION_CONFIGMAP`,
`ELASTIC_MIGRATION_SECRET`, and `ELASTIC_MIGRATION_AUTH` (`basic` or `apikey`). Only the
selected authentication method is passed to the Job. References are validated before promotion.

Omit `m=` to apply pending migrations, or specify `m=1.0.11` to target that version. An earlier
version requests rollback. Unlike EF database migrations, Elasticsearch migrations do not accept
`m=0`. The tool's existing migration-history checks remain in effect; these scripts do not seed
or override the baseline automatically. For a verified existing 1.0.10 schema without migration
history (including the inspected TEST/PROD Cloud instances), explicitly pass
`ELASTIC_MIGRATION_BASELINE=1.0.10`. Do not use a baseline to skip unapplied schema changes.

Add `p=prepare`, `p=copy`, `p=verify` or `p=cutover` (with `m=`) to run one step of a migration as its
own Job, which lets a second indexing service keep the new indexes current with no downtime; see the
[operations runbook](./kustomize/elastic-migration/README.md#run-the-migration).

See the runbook for storage checks, partial-index cleanup, task recovery, and validation. The default Job execution
budget is 24 hours for Elasticsearch and 30 minutes for database migrations; override with
`MIGRATION_ACTIVE_DEADLINE_SECONDS`. Startup waits default to 15 minutes and are independently
controlled by `MIGRATION_STARTUP_TIMEOUT_SECONDS`. A Job timeout does not cancel Elasticsearch's
server-side tasks. `ELASTIC_MIGRATION_REQUESTS_PER_SECOND` throttles a new native copy (default -1).

TEST/PROD local Elasticsearch and local `indexing-service` overlays now specify zero replicas.
Their Cloud indexing deployments remain enabled. Applying these overlays is a separate operational
step; local PVCs remain allocated until explicitly retired. See the runbook before reclaiming them.

To verify script behavior without contacting Docker, ACR, or OpenShift:

```bash
python3 -m unittest discover -s openshift/scripts/tests -v
```

## Kafka topic migrations

Apply the Kafka topic migrations (`db/kafka/migrations`) to an environment's brokers. Log in with
`oc login` first. The settings (partitions, replication factor, topic configuration, per-topic
overrides) are in `db/kafka/environments/<environment>.conf`; see `db/kafka/README.md`.

```bash
make kafka-update e=dev d=1     # dry run: show what would change
make kafka-update e=dev         # apply every migration (asks first; y=1 skips)
make kafka-update e=prod n=1.0.1
make kafka-topics e=prod        # list topics with partitions and replication factor
```

## Platform Registry Services

The Exchange Lab has an app that provides a way to request a new product, or provision more resource quotas here [https://registry.developer.gov.bc.ca/dashboard](https://registry.developer.gov.bc.ca/dashboard)

## Helpful Tips

There are a number of helpful tips here - [openshift.tips](https://openshift.tips/)

## Extract Parameters

The following command will extract parameters from the template.
This will provide you a list of all the parameters, which you can extract to create a parameter file.
When creating a parameter file, use `.env` files so they are not committed to the source code repository.

```bash
oc process -f ${pathToFile:-build.yaml} --parameters=true
```

## Create Objects

The following command will process a template, apply configured parameters and create the objects in the template within Openshift.
This will also save the template within Openshift.

```bash
oc process -f ${pathToFile:-build.yaml} --param-file=${pathToFile:-build.dev.env} | oc create --save-config=true -f -
```

## Delete Objects

The following command will delete all objects in the template.

```bash
oc process -f ${pathToFile:-build.yaml} --param-file=${pathToFile:-build.dev.env} | oc delete -f -
```

## Replace Objects

The following command will replace all objects in the template, apply configured parameters and create the objects in the template within Openshift.

```bash
oc process -f ${pathToFile:-build.yaml} --param-file=${pathToFile:-build.dev.env} | oc replace --save-config=true -f -
```

## Update Objects

The following command will update all objects in the template, apply configured parameters and update the template within Openshift.

```bash
oc process -f ${pathToFile:-build.yaml} --param-file=${pathToFile:-build.dev.env} | oc apply -f -
```

For Services, the process is slightly different. Use `apply` to patch values or `replace` to replace. Note that `apply` will not remove existing values if you change names.

```bash
oc kustomize ${pathToEnvironmentFolder} | oc [apply|replace] -f -
```

## Port Forward

The following command will port forward the specified container so that you can communicate with the pod directly from your computer.

```bash
# Get the pod name so you can reference it.
oc get pods -n ${project:-9b301c-dev}
oc port-forward $podName ${localPort:-22222}:${containerPort:-5432}
```

## Network Policies

By default security is Zero Trust.
Which means nothing can communicate.
For a basic setup where TOOLS provides images the following projects need to have access DEV, TEST, and PROD.

Enable the **Service Account** to pull images from external sources.

```bash
oc policy add-role-to-user system:image-puller system:serviceaccount:9b301c-dev:default -n 9b301c-tools
oc policy add-role-to-user system:image-puller system:serviceaccount:9b301c-test:default -n 9b301c-tools
oc policy add-role-to-user system:image-puller system:serviceaccount:9b301c-prod:default -n 9b301c-tools

# Or apply to the tno account
oc policy add-role-to-user system:image-puller system:tno:9b301c-prod:default -n 9b301c-tools

# Or apply this to all serviceaccounts
oc policy add-role-to-group system:image-puller system:serviceaccounts:9b301c-prod -n 9b301c-tools
```

There are additional default configurations in the `./templates/network-policy` folder.
These should be applied to each project.

```bash
oc process -f ./network-policy/default.yaml --param-file=${pathToFile:default.dev.env} | oc create --save-config=true -f -
oc process -f ./network-policy/default.yaml --param-file=${pathToFile:default.test.env} | oc create --save-config=true -f -
oc process -f ./network-policy/default.yaml --param-file=${pathToFile:default.prod.env} | oc create --save-config=true -f -
oc process -f ./network-policy/default.yaml --param-file=${pathToFile:default.tools.env} | oc create --save-config=true -f -
```

## Find images

If you need to find images hosted in a image registry within Openshift.

```bash
# List all images in bcgov namespace
oc -n bcgov get is
# Search images
oc -n bcgov get imagestreamtag | grep $imageName
```

## Import an Image from Redhat

Some Redhat images are only accessible if they are first imported through Openshift.

```bash
oc import-image postgresql-13 --from=registry.redhat.io/rhel8/postgresql-13 --confirm -n 9b301c-tools
```

## Push/Pull Images with Docker

Login first.

```bash
# Login with Docker (insecure)
docker login -u $(oc whoami) -p $(oc whoami -t) image-registry.apps.silver.devops.gov.bc.ca
# Or login securely
oc whoami -t | docker login -u $(oc whoami) --password-stdin image-registry.apps.silver.devops.gov.bc.ca
```

## Push Image

If you have a local image you can push up to Openshift.

```bash
# List images
docker images
# Tag the image you want to push
docker tag $imageName:$tag image-registry.apps.silver.devops.gov.bc.ca/9b301c-tools/$imageName:$tag
# Push to image registry in Openshift
docker push image-registry.apps.silver.devops.gov.bc.ca/9b301c-tools/$imageName:$tag
```

## Pull Image

If you want to pull down an image from Openshift.

```bash
# List image
oc get is -n 9b301c-tools
# Pull image from Openshift
docker pull image-registry.apps.silver.devops.gov.bc.ca/9b301c-tools/$imageName:$tag
```

## Test Network in Container

Sometimes you may need to confirm you container can communicate with the internet.

```bash
timeout 5 bash -c "</dev/tcp/google.com/443"; echo $?
```

## Change Resource Requirements

If you need to update the resource requirements of pods you can do this without editing their templates.

```bash
oc set resources dc/${DeployConfig.name} --requests=cpu=50m,memory=50Mi --limits=cpu=500m,memory=500Mi
```

## Extract the current project name without the environment

Sometimes it's a pain to remember the random names of the project, it's nice to use a command that extracts it.

```bash
# Extract the random characters of the project namespace.
project=$(oc project --short); project=${project//-[a-z]*/}; echo $project

# Change the current environment
oc project $project-tools
```

## Copy Files to or from a Container

You can use the CLI to copy local files to or from a remote directory in a container.
More information [here](https://docs.openshift.com/container-platform/3.11/dev_guide/copy_files_to_container.html)

```bash
oc rsync <source> <destination> [-c <container>]

# Copy to pod
oc rsync /home/user/source devpod1234:/src

# Copy from pod
oc rsync devpod1234:/src /home/user/source
```

If you need to create a pod that mounts a PVC first.

```bash
oc run some-pod --overrides='{"spec": {"containers": [{"command": ["/bin/bash", "-c", "trap : TERM INT; sleep infinity & wait"], "image": "registry.access.redhat.com/rhel7/rhel:latest", "name": "some-pod", "volumeMounts": [{"mountPath": "/data", "name": "some-data"}]}], "volumes": [{"name": "some-data", "persistentVolumeClaim": {"claimName": "test-file"}}]}}' --image=dummy --restart=Never
```

## Helpful Information on Docker Permissions

[Documentation](https://developers.redhat.com/blog/2020/10/26/adapting-docker-and-kubernetes-containers-to-run-on-red-hat-openshift-container-platform#executable_permissions)

## Open a remote shell to containers

[Documentation](https://docs.openshift.com/container-platform/3.11/dev_guide/ssh_environment.html)

```bash
oc rsh <pod>
```

## Delete all Pods in Error State

```bash
for pod in $(oc get pods | grep Error | awk '{print $1}'); do oc delete pod --grace-period=1 ${pod}; done
```

## Get Image Hash for Pod

```bash
oc get pods api-0 -o jsonpath="{..imageID}"
```

## Sysdig

<https://app.sysdigcloud.com/#/login>

Login with OpenID.

Enter `BCDevOps` for the authentication.

Login with IDIR.

## Build and Deploy to ACR

```bash
# Login to Azure
az login
az acr login --name bcgov

# Build the image
docker build --platform linux/amd64 -t bcgov-c4awhwfpcremdbga.azurecr.io/transcription-service:latest -f services/net/transcription/Dockerfile .

# Tag if required (skip if already tagged above)
docker push bcgov-c4awhwfpcremdbga.azurecr.io/transcription-service:latest

# Push to ACR
docker push bcgov-c4awhwfpcremdbga.azurecr.io/transcription-service:latest

# Verify
az acr repository show-tags --name bcgov --repository transcription-service --output table

# Tag with a different tag
az acr import \
  --name bcgov \
  --source bcgov-c4awhwfpcremdbga.azurecr.io/transcription-service:latest \
  --image transcription-service:dev \
  --force

# Rollout deployment
oc rollout restart deployment/transcription-service -n 9b301c-dev
```


## Orphaned content administration

Deploy database migration **1.5.13** with the API change. It prevents deleting sources,
media types, licenses, and series referenced by content; deleting a contributor instead
clears the content's contributor reference. The API also refreshes affected search documents.

API deployments now use `services/EMAIL_FAILURE_TO` for `Service__EmailTo`, matching the
indexing service. `Service__SendEmailOnFailure=true` enables API index-dispatch alerts;
CHES configuration and email authorization still apply. Exhausted Elasticsearch write
retries use the indexing service's existing error-email mechanism. Alerts do not provide
a durable outbox: a process terminated before sending an alert cannot deliver one.

Administrators can open **Data Import → Orphaned Content** in the Editor. Scans compare
PostgreSQL with the configured all-content or published-content index in bounded batches.
Private database content is excluded; the published comparison includes only Publish/Published
content. Migration **1.5.14** adds durable scan progress and results. **1.5.15** adds posted-date
filters and **1.5.16** adds the date cursor and a concurrent `(posted_on, id)` index. Apply
these migrations before starting the updated API worker. Run the concurrent-index migration
without wrapping it in an additional transaction.

The scan runs inside the API as a hosted background service. PostgreSQL advisory locking and
a unique active-job index coordinate replicas. Each worker creates a fresh dependency scope
per batch, compares IDs only, and stores candidates in PostgreSQL. Headlines are fetched only
when candidates are confirmed. The Editor polls saved progress and displays 100 results per
page; closing the browser does not stop the scan. Administrators can cancel a run or restrict
it by start/end `postedOn` dates. Dates are inclusive Pacific calendar days; the API converts
them to UTC boundaries, using an exclusive midnight after the end date. Either bound can be
omitted. Content with no `postedOn` is excluded when a date bound is supplied. Each run checks one direction against one selected index.

Default API settings (environment variables use double underscores):

| Setting | Default | Allowed range |
| --- | --- | --- |
| `OrphanScan__BatchSize` | 200 | 1–500 |
| `OrphanScan__BatchDelayMilliseconds` | 1000 | 1000–30000 |
| `OrphanScan__GracePeriodSeconds` | 300 | 30–86400 |
| `OrphanScan__BatchTimeoutSeconds` | 30 | 5–60 |

The delay applies across replicas, not independently per worker. Batches have a time limit;
API memory use scales with batch size, not dataset size. PostgreSQL traversal uses a `(posted_on, id)` cursor for date-filtered scans and a primary-key
cursor otherwise, with a fixed upper ID bound; recently updated content is excluded. Elasticsearch traversal
uses `search_after`, `_shard_doc`, and one point-in-time snapshot with a two-minute renewable
expiry. Snapshots are closed after discovery, cancellation, or failure; expiry also releases them.
A long scan retains Elasticsearch segments for its duration, so use targeted ranges when practical.

Progress resumes after a short API interruption. If the Elasticsearch snapshot has expired,
the run fails explicitly and needs a new scan. Partial/timed-out Elasticsearch results never
become confirmed orphans. Candidates are rechecked against both live stores after the grace
period (confirmation starts after discovery). Results are observations, not a transactionally
consistent snapshot of both stores. Recent changes and indexing lag may require a later run.
Completed and cancelled scan records/results are retained in PostgreSQL for review; they do not
hold full content documents. Operators should include these tables in their metadata retention policy.

Repairs recheck each item. Indexing uses the normal Kafka workflow and is shown as queued
until a later scan verifies it. Restoration uses the original ID and the full search document
when available; a published-only copy may lack an unapproved transcript. Missing lookup
records block restoration (a missing optional contributor is cleared). Search documents
cannot recover deleted media files, report membership, deliveries, or analysis history.
Deleting a PostgreSQL orphan also deletes its related records through the existing cascades.
All scan and repair endpoints under `/api/admin/orphaned-content` require Administrator.
