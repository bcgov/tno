# Elasticsearch Migration 1.0.11

Operations runbook for running Elasticsearch migration 1.0.11 in OpenShift. For the ordered steps and commands, use the
[1.0.11 checklist](./1.0.11.md). How the tool works internally (task reconnection, repair, alias cutover, rollback)
is described in the [tool README](../../../tools/elastic/migration/README.md).

## What the Migration Does

1.0.11 adds content analysis to the search indexes and a new evidence index used by report AI.

1. Creates new indexes named `<alias>_v1.0.11-<generation>` for the published content, unpublished content and
   evidence indexes. The generation is derived from the source index UUIDs, so a rerun reuses the same names.
2. Copies every document from the 1.0.10 indexes with a native Elasticsearch `_reindex`. Each document's
   `_version` is set from its `projectionRevision`, so an older copy can never replace a newer write.
3. Brings current analyses in from PostgreSQL, compares every content ID and revision with the database, and
   repairs or removes differences.
4. Moves the `content`, `unpublished_content` (or `published_content` on Cloud) and `content_evidence` aliases to
   the new indexes in one atomic request, then records `1.0.11` in the `migrations` index.

Writers do not need to be paused. Searches keep using the 1.0.10 indexes until the alias switch.

> Until the [stepped migration](#planned-zero-downtime-stepped-migration) is built, content that changes after the
> tool has verified an index only reaches the new index the next time that content is indexed.

## Environments

| Environment | Elasticsearch migrated | Index ConfigMap | Credential Secret | Authentication |
| ----------- | ----------------------- | ------------------------ | ----------------- | ------------------- |
| DEV | OpenShift `elastic` StatefulSet | `indexing-service` | `elastic` | USERNAME / PASSWORD |
| TEST | Elastic Cloud (`test-mmi`) | `indexing-service-cloud` | `elastic-cloud` | ApiKey |
| PROD | Elastic Cloud (`prod-mmi`) | `indexing-service-cloud` | `elastic-cloud` | ApiKey |

Only the primary cluster is migrated. TEST and PROD still run a local OpenShift `elastic` StatefulSet, which is
being retired and is not migrated.

### State on 2026-10-09

| Environment | Migration history | Indexes behind the aliases | Baseline needed |
| ----------- | ----------------- | -------------------------- | --------------- |
| DEV | `1.0.10` | `content_v1.0.10`, `unpublished_content_v1.0.10` | No |
| TEST Cloud | `migrations` index exists but is empty | `content_v1.0.10`, `published_content_v1.0.10` | `ELASTIC_MIGRATION_BASELINE=1.0.10` |
| PROD Cloud | no `migrations` index | `content_v1.0.10`, `published_content_v1.0.10` | `ELASTIC_MIGRATION_BASELINE=1.0.10` |
| TEST local (not migrated) | `1.0.10` | `content_v1.0.10`, `unpublished_content_v1.0.10` | — |

A baseline records history only; it does not apply changes. Verify the Cloud mappings really are 1.0.10 before
using it.

## Size, Duration and Disk

| Environment | Documents to copy (unpublished + published) | Primary data | Free disk |
| ----------- | ------------------------------------------- | ----------------- | ---------------------------------------- |
| DEV | 4.69M + 0.62M = 5.31M | 19.1 GB + 1.8 GB | ~27 GB per node, 3 nodes, 45% used |
| TEST Cloud | 4.59M + 0.90M = 5.49M | 21.1 GB + 3.0 GB | 34.8 GB free of 60 GB, 1 data node |
| PROD Cloud | 9.22M + 3.30M = 12.52M | 40.9 GB + 12.6 GB | ~250 GB across 3 nodes |

Counts are top-level documents; `unpublished_content` holds about 4.8 nested documents for each one, which is the
main cost of the copy.

- **Duration.** The cancelled DEV run copied about 120–180 documents/second while each Elasticsearch pod was limited
  to 0.15 CPU and a 512 MB heap, which projects to 8–12 hours for DEV. DEV now has 2 CPU and a 1.5 GB heap, and the
  Cloud clusters are not CPU-limited, so expect faster runs; time TEST and estimate PROD at about 2.3 × TEST.
- **Job time limit.** The Job's default limit is 24 hours (`MIGRATION_ACTIVE_DEADLINE_SECONDS=86400`). Raise it for
  PROD if TEST suggests it is needed, e.g. `MIGRATION_ACTIVE_DEADLINE_SECONDS=172800`.
- **Disk.** The new indexes are created with the source's replica count, so each copy needs the source's full size
  again, including replicas. On DEV that takes each node to about 88% and past Elasticsearch's 85% low watermark.
  TEST Cloud reaches about 82%. PROD has room. Free space or add disk before a DEV or TEST run. Do not disable disk
  watermarks.

## Accessing Elasticsearch

The commands below use these helpers. DEV Elasticsearch has security disabled and is reached through a pod; Cloud is
reached through the `indexing-service-cloud` pod, which holds the API key.

```bash
# DEV
N=9b301c-dev
es() { oc exec -n $N elastic-0 -- curl -s "$@"; }

# TEST or PROD Cloud: paths only, e.g. escloud '_cat/indices?v'
N=9b301c-test   # or 9b301c-prod
escloud() { oc exec -n $N deploy/indexing-service-cloud -- sh -c "curl -s -H \"Authorization: ApiKey \$Elastic__ApiKey\" \"\$Elastic__Url/$1\""; }
```

For DEV, pass the URL as `localhost:9200/<path>`, e.g. `es 'localhost:9200/_cat/health?v'`.

## Before You Start

1. **Backups.** Take or confirm an Elasticsearch snapshot and a database backup.
2. **Cluster health.** Green, or yellow only for replicas that cannot be placed.

   ```bash
   es 'localhost:9200/_cat/health?v'
   es 'localhost:9200/_cat/allocation?v'
   ```

3. **Aliases and history.** The aliases point at the `_v1.0.10` indexes and history matches the
   [table above](#state-on-2026-10-09).

   ```bash
   es 'localhost:9200/_cat/aliases?v' | grep -v '^\.'
   es 'localhost:9200/migrations/_search?size=50&filter_path=hits.hits._id'
   ```

4. **Nothing left over.** No `_v1.0.11` indexes, no running reindex task and no `elastic-migration` Job.

   ```bash
   es 'localhost:9200/_cat/indices?v&s=index' | grep -v '^\.'
   es 'localhost:9200/_tasks?actions=*reindex'          # expect "nodes":{}
   oc get jobs -n $N | grep elastic-migration
   ```

5. **`content_evidence` is not a concrete index.** The migration creates `content_evidence` as an alias. An
   indexing service running the content-analysis code creates a concrete `content_evidence` index the first time it
   writes evidence. If one exists and is empty, delete it by its exact name:

   ```bash
   es 'localhost:9200/_cat/indices/content_evidence?v&h=index,docs.count'
   es 'localhost:9200/_alias/content_evidence'           # expect 404 "alias missing"
   es -XDELETE 'localhost:9200/content_evidence'
   ```

   In DEV, keep `indexing-service` scaled to 0 until the alias exists, or it will recreate the index.

6. **Disk.** Confirm the space described in [Size, Duration and Disk](#size-duration-and-disk).

## Run the Migration

From the repository root, build and push the tool image, then run it in each environment in turn.

```bash
make -C openshift build n=elastic-migration t=latest
make -C openshift push n=elastic-migration t=latest

# DEV
make -C openshift deploy n=elastic-migration e=dev t=latest m=1.0.11

# TEST, then PROD once TEST is validated
ELASTIC_MIGRATION_BASELINE=1.0.10 make -C openshift deploy n=elastic-migration e=test t=latest m=1.0.11
ELASTIC_MIGRATION_BASELINE=1.0.10 MIGRATION_ACTIVE_DEADLINE_SECONDS=172800 \
  make -C openshift deploy n=elastic-migration e=prod t=latest m=1.0.11
```

`deploy` promotes the image tag to the environment, creates a one-shot Job `elastic-migration-<timestamp>` in
`9b301c-<environment>`, follows its logs and reports failure. Jobs are not retried and are kept for 24 hours.

| Variable | Default | Purpose |
| --------------------------------------- | ------- | ---------------------------------------------------------- |
| `ELASTIC_MIGRATION_BASELINE` | empty | Records an existing version when history is absent (TEST, PROD) |
| `ELASTIC_MIGRATION_REQUESTS_PER_SECOND` | -1 | Throttles a new copy; a positive integer or -1 for unlimited |
| `MIGRATION_ACTIVE_DEADLINE_SECONDS` | 86400 | Job execution limit |
| `MIGRATION_STARTUP_TIMEOUT_SECONDS` | 900 | Wait for the image to pull and the container to start |
| `ELASTIC_MIGRATION_CONFIGMAP` / `_SECRET` / `_AUTH` | per environment | Override the Elasticsearch target (`basic` or `apikey`) |

The GitHub Actions workflow **Elastic Migration CI/CD** runs the same script for DEV and TEST through
`workflow_dispatch`, with an optional baseline. A hosted runner can time out before a long Job finishes; the Job keeps
running, so check it with `oc`.

## Watch Progress

```bash
oc get jobs -n $N | grep elastic-migration
oc logs -f job/<job-name> -n $N

# Copy progress: compare "created" with "total"
es 'localhost:9200/_tasks?actions=*reindex&detailed=true' | grep -o '"total":[0-9]*\|"created":[0-9]*\|"running_time_in_nanos":[0-9]*'

# Disk while the copy runs
es 'localhost:9200/_cat/allocation?v'
```

## After It Succeeds

1. The aliases point at the `_v1.0.11-<generation>` indexes, including `content_evidence`.
2. `migrations` contains `1.0.11`.
3. Test editor and subscriber search, published transcript filtering and report AI evidence.
4. Keep the `_v1.0.10` indexes until those checks and the retention window pass. Delete them later by exact name;
   never with a wildcard.

## Stop a Running Migration

Order matters. The tool treats a cancelled copy as a finished one, so cancelling the task while the Job is running,
or before the partial indexes are deleted, makes the tool continue as if the copy were complete.

1. **Delete the Job first.** `backoffLimit` is 0, so it is not retried; its PostgreSQL lock is released when the pod
   ends.

   ```bash
   oc delete job <job-name> -n $N --wait
   oc get pods -n $N | grep elastic-migration     # expect nothing
   ```

2. **Cancel the copy in Elasticsearch** and wait until no reindex task remains. The task ID is in the Job log and in
   `_tasks`.

   ```bash
   es 'localhost:9200/_tasks?actions=*reindex'
   es -XPOST 'localhost:9200/_tasks/<task-id>/_cancel'
   es 'localhost:9200/_tasks?actions=*reindex'     # repeat until "nodes":{}
   ```

3. **Check the aliases did not move.**

   ```bash
   es 'localhost:9200/_cat/aliases?v' | grep -v '^\.'
   ```

4. **Delete the partial `_v1.0.11-<generation>` indexes** by exact name, once no alias uses them and no task is
   running. Otherwise the next run reconnects to the cancelled task and skips the copy.

   ```bash
   es -XDELETE 'localhost:9200/unpublished_content_v1.0.11-<generation>'
   es -XDELETE 'localhost:9200/content_v1.0.11-<generation>'
   es -XDELETE 'localhost:9200/content_evidence_v1.0.11-<generation>'
   ```

History is only recorded after the alias switch, so a stopped run needs no rollback.

## If the Job Fails or Times Out

A Job that fails or times out does not stop the copy running inside Elasticsearch.

- **Let it finish and rerun.** Watch `_tasks/<task-id>`; when it completes, run `deploy` again. The new Job reuses the
  same indexes and reconnects to the finished task.
- **Or stop it** with the steps in [Stop a Running Migration](#stop-a-running-migration).
- **Repairs made no progress.** The tool refuses to finish and leaves the aliases unchanged. Check the Job log for the
  index and IDs before rerunning.
- **Task missing after a node restart.** The tool clears the task ID and fails; rerun to replay the copy into the
  retained indexes.

Do not run `m=1.0.10` to recover from a failed upgrade. That is a full rollback that copies everything again.

## DEV Elasticsearch Cluster

The DEV cluster is the `elastic` StatefulSet in `9b301c-dev`: 3 nodes running Elasticsearch 7.17.9, each master and
data, with security disabled. Configuration is in [kustomize/elastic](../elastic).

| Setting | Value | Where |
| ------------------------------- | --------------------------- | ------------------------------------- |
| CPU request / limit | 300m / 2 | `overlays/dev/kustomization.yaml` |
| Memory request / limit | 2Gi / 3Gi | `overlays/dev/kustomization.yaml` |
| Heap (`ES_JAVA_OPTS`) | `-Xms1536m -Xmx1536m` | `base/statefulset.yaml` |
| Readiness probe | `GET /_cluster/health?local=true` | `base/statefulset.yaml` |
| `terminationGracePeriodSeconds` | 120 | `base/statefulset.yaml` |
| `publishNotReadyAddresses` | `true` on `elastic-headless` | `base/services.yaml` |

TEST and PROD overlays use 2Gi / 3Gi memory to fit the same heap.

- The heap must be no more than half the container's memory limit.
- The nodes find each other through `elastic-headless`. `publishNotReadyAddresses` lets them do that before they are
  ready; without it the cluster cannot form after all pods restart together.
- The namespace quota caps total CPU requests at 4 cores; DEV uses about 3.5.
- Indexes need at most 2 replicas on 3 nodes. With 3 replicas one copy can never be placed and the cluster stays
  yellow.

### Restart the Pods One at a Time

Any change to the pod template (resources, heap, probes) restarts the pods. The readiness probe only shows that a
node answers, not that its shards have recovered, so a normal rolling update can take down two nodes at once and lose
the master quorum. Hold the rollout with a partition and release one pod at a time.

```bash
N=9b301c-dev

# 1. Hold the rollout, then make the change
oc patch sts elastic -n $N -p '{"spec":{"updateStrategy":{"type":"RollingUpdate","rollingUpdate":{"partition":3}}}}'
oc set env sts/elastic -n $N ES_JAVA_OPTS="-Xms1536m -Xmx1536m"     # example change
oc get sts elastic -n $N -o jsonpath='{.status.currentRevision} {.status.updateRevision}{"\n"}'   # two revisions
```

Then for each pod, in the order elastic-2 (partition 2), elastic-1 (partition 1), elastic-0 (partition 0). Run the
commands through a pod that is not restarting: elastic-0 for the first two, elastic-1 for elastic-0.

```bash
P=2; X=elastic-0
es() { oc exec -n $N $X -- curl -s "$@"; }

# a. Keep replicas in place while the node is down, and flush
es -XPUT localhost:9200/_cluster/settings -H 'Content-Type: application/json' -d '{"persistent":{"cluster.routing.allocation.enable":"primaries"}}'
es -XPOST localhost:9200/_flush

# b. Restart one pod and wait for it to be ready
oc patch sts elastic -n $N -p "{\"spec\":{\"updateStrategy\":{\"rollingUpdate\":{\"partition\":$P}}}}"
oc rollout status sts/elastic -n $N --timeout=10m

# c. Wait for the node to rejoin
es 'localhost:9200/_cluster/health?wait_for_nodes=3&timeout=5m&filter_path=status,number_of_nodes,timed_out'

# d. Turn allocation back on and wait for green
es -XPUT localhost:9200/_cluster/settings -H 'Content-Type: application/json' -d '{"persistent":{"cluster.routing.allocation.enable":"all"}}'
es 'localhost:9200/_cluster/health?wait_for_status=green&wait_for_no_initializing_shards=true&timeout=8m&filter_path=status,timed_out,unassigned_shards'
```

Each pod keeps its own disk, so a restarted node recovers in seconds. After elastic-0 the partition is 0 and later
changes roll out normally. Never leave `cluster.routing.allocation.enable` at `primaries`.

## Planned: Zero-Downtime Stepped Migration

> Not built yet. This section describes the agreed direction; the commands do not work today.

The tool will run as separate steps so a second indexing service can write live changes into the new indexes while
the copy runs. Each step is resumable; `all` runs every step in order.

| Step | What it does |
| -------- | ------------------------------------------------------------------------------------------------------------- |
| prepare | Creates the three new indexes with the 1.0.11 mappings, 0 replicas, refresh off and a longer `gc_deletes`, then prints their names. |
| *(you)* | Start a second indexing service pointed at the new indexes. |
| copy | Native `_reindex` of existing documents with `slices`, versioned and resumable. A cancelled task counts as failed. |
| verify | Brings analyses in from PostgreSQL and repairs differences; content changed after the step starts is left to the second indexing service. |
| cutover | Restores replicas and refresh, waits for green, switches the aliases in one request and records the history. Then scale the second indexing service down. |

### Second Indexing Service

Configure it the same way as the existing `indexing-service-cloud` overlay
([kustomize/services/indexing/overlays/cloud-test](../services/indexing/overlays/cloud-test)): a renamed
Deployment, Service and ConfigMap with these values.

| ConfigMap key | Value | Why |
| ------------------------- | --------------------------------------- | ------------------------------------------------------------------ |
| `KAFKA_CLIENT_ID` | its own, e.g. `IndexingMigration` | Used as the Kafka consumer group. A shared group splits messages between the two services. |
| `INDEX_ONLY` | `"true"` | Stops duplicate alerts and notifications. |
| `CONTENT_INDEX` | the new unpublished content index | Where it writes. |
| `PUBLISHED_INDEX` | the new published content index | Where it writes. |
| `EVIDENCE_INDEX` | the new evidence index | Where it writes. |
| `ELASTICSEARCH_URI` | the environment's primary cluster | Same cluster as the migration. |

Start it after `prepare` and before `copy`, so no change made during the copy is missed. Every write is versioned by
`projectionRevision`, so it and the copy can run at the same time in any order.

## Troubleshooting

| Symptom | Cause | Action |
| ------- | ----- | ------ |
| Copy is very slow | Elasticsearch CPU or heap limits | Check `_cat/nodes?v&h=name,heap.max,cpu,load_1m` and the pod limits |
| `disk usage exceeded flood-stage watermark`, indexes read-only | Copy filled the disk | Stop the run, free disk, delete the partial indexes, rerun |
| Cluster red after a restart | Pods restarted together | Wait for `_cluster/health`; use the partition steps next time |
| `content_evidence` alias cannot be created | A concrete `content_evidence` index exists | See [Before You Start](#before-you-start), step 5 |
| `Repairs to '<index>' made no progress` | Differences the tool cannot fix | Check the Job log; aliases are unchanged |
| `Source index '<index>' was replaced; refusing to resume` | Source index changed between runs | Delete the partial indexes and rerun |
| Job succeeded but searches miss recent edits | Content changed after verification | Reindex the affected content |
