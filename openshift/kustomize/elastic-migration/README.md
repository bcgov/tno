# Elasticsearch Migration 1.0.11

Operations runbook for running Elasticsearch migration 1.0.11 in OpenShift. For the ordered steps and commands, use the
[1.0.11 checklist](./1.0.11.md). How the tool works internally (task reconnection, repair, alias cutover, rollback)
is described in the [tool README](../../../tools/elastic/migration/README.md).

## What the Migration Does

1.0.11 adds content analysis to the search indexes and a new evidence index used by report AI. It runs as four
steps, each a separate Job (`p=<step>`), or all of them in one Job.

| Step | What it does |
| ---- | ------------ |
| `prepare` | Creates new indexes named `<alias>_v1.0.11-<generation>` for the published content, unpublished content and evidence indexes, with the 1.0.11 mappings plus the fields the old indexes mapped dynamically. They load without replicas or refreshes, and remember deletes for 48 hours. The Job log prints their names. |
| `copy` | Copies every document from the 1.0.10 indexes with a native Elasticsearch `_reindex`. Each document's `_version` is set from its `projectionRevision`, so an older copy can never replace a newer write. A cancelled or failed copy is not counted as done. |
| `verify` | Brings current analyses in from PostgreSQL, compares every content ID and revision with the database, and repairs or removes differences. Content that changes while it runs is left to the indexing service once repaired. |
| `cutover` | Restores the replicas, refreshes and delete retention, waits until the replicas are ready, moves the `content`, `unpublished_content` (or `published_content` on Cloud) and `content_evidence` aliases to the new indexes in one atomic request, verifies again, then records `1.0.11` in the `migrations` index. |

The generation is derived from the source index UUIDs, so every step and every rerun uses the same names. Each step
checks the one before it has finished, and each can be rerun. Searches keep using the 1.0.10 indexes until cutover.

There are two ways to run it:

- **Stepped, no downtime (recommended).** The existing indexing service keeps writing to the live indexes. A
  [second indexing service](#second-indexing-service), started after `prepare`, writes the same changes into the new
  indexes while they are copied and verified. Every write is versioned by `projectionRevision`, so the two services,
  the copy and the repairs can overlap in any order. Search stays current throughout. The
  [1.0.11 checklist](./1.0.11.md) follows this way.
- **Single run, indexing off.** The indexing service that writes to the migrated cluster (`indexing-service` in DEV,
  `indexing-service-cloud` in TEST and PROD) is scaled to 0 and one Job runs every step. Content changes wait in the
  Kafka `index` topic, kept for 7 days, and are indexed into the new indexes when indexing is turned back on after
  the alias switch. Search shows nothing new or changed for as long as the run takes (hours).

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
- **Job time limit.** Each Job's default limit is 24 hours (`MIGRATION_ACTIVE_DEADLINE_SECONDS=86400`). Copy takes
  most of the time. Raise it for PROD if TEST suggests it is needed, e.g. `MIGRATION_ACTIVE_DEADLINE_SECONDS=172800`.
- **Disk.** The new indexes load without replicas, so the copy needs the source's primary size again. Cutover adds
  the source's replica count, so afterwards the new indexes take the source's full size, replicas included. On DEV
  that takes each node to about 88% and past Elasticsearch's 85% low watermark. TEST Cloud reaches about 82%. PROD
  has room. Free space or add disk before cutover in DEV or TEST; doing it before `prepare` is simplest. Do not
  disable disk watermarks.

## Accessing Elasticsearch

The commands below use these helpers. DEV Elasticsearch has security disabled and is reached through a pod. Cloud is
reached directly from your machine with the API key from the `elastic-cloud` secret, so it works while
`indexing-service-cloud` is scaled to 0.

```bash
# DEV
N=9b301c-dev
es() { oc exec -n $N elastic-0 -- curl -s "$@"; }

# TEST or PROD Cloud: paths only, e.g. escloud '_cat/indices?v'
N=9b301c-test   # or 9b301c-prod
ESKEY=$(oc get secret elastic-cloud -n $N -o jsonpath='{.data.ApiKey}' | base64 -d)
ESURL=$(oc get configmap indexing-service-cloud -n $N -o jsonpath='{.data.ELASTICSEARCH_URI}')
escloud() { curl -s -H "Authorization: ApiKey $ESKEY" "$ESURL/$1"; }
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

5. **`content_evidence` is not a concrete index, if you can avoid it.** The migration creates `content_evidence` as an
   alias. A running indexing service creates an empty concrete `content_evidence` index when it deletes evidence for
   content without analysis. Cutover backs it up to `content_evidence-backup-<time>` and replaces it with the alias,
   blocking writes to it for the few seconds that takes. While the indexing service runs it will be recreated, which
   is expected. Otherwise, if one exists and is empty, delete it by its exact name:

   ```bash
   es 'localhost:9200/_cat/indices/content_evidence?v&h=index,docs.count'
   es 'localhost:9200/_alias/content_evidence'           # expect 404 "alias missing"
   es -XDELETE 'localhost:9200/content_evidence'
   ```

6. **Disk.** Confirm the space described in [Size, Duration and Disk](#size-duration-and-disk).

## Run the Migration

From the repository root, build and push the tool image, then run it in each environment in turn.

```bash
make -C openshift build n=elastic-migration t=latest
make -C openshift push n=elastic-migration t=latest

# Stepped: one Job per step, the second indexing service running from after prepare until after cutover
make -C openshift deploy n=elastic-migration e=dev t=latest m=1.0.11 p=prepare
make -C openshift deploy n=elastic-migration e=dev t=latest m=1.0.11 p=copy
make -C openshift deploy n=elastic-migration e=dev t=latest m=1.0.11 p=verify
make -C openshift deploy n=elastic-migration e=dev t=latest m=1.0.11 p=cutover

# Single run, with indexing off: every step in one Job
make -C openshift deploy n=elastic-migration e=dev t=latest m=1.0.11
```

For TEST and PROD add `ELASTIC_MIGRATION_BASELINE=1.0.10` (see [the table above](#state-on-2026-10-09)), and for
PROD's copy a longer `MIGRATION_ACTIVE_DEADLINE_SECONDS`.

`p=` needs `m=`, and takes `prepare`, `copy`, `verify`, `cutover` or `all` (the default). A rollback (`m=1.0.10`)
always runs as a single run.

`deploy` promotes the image tag to the environment, creates a one-shot Job `elastic-migration-<timestamp>` (or
`elastic-migration-<step>-<timestamp>`) in `9b301c-<environment>`, follows its logs and reports failure. Jobs are not
retried and are kept for 24 hours.

| Variable | Default | Purpose |
| --------------------------------------- | ------- | ---------------------------------------------------------- |
| `ELASTIC_MIGRATION_BASELINE` | empty | Records an existing version when history is absent (TEST, PROD) |
| `ELASTIC_MIGRATION_REQUESTS_PER_SECOND` | -1 | Throttles a new copy; a positive integer or -1 for unlimited |
| `MIGRATION_ACTIVE_DEADLINE_SECONDS` | 86400 | Job execution limit |
| `MIGRATION_STARTUP_TIMEOUT_SECONDS` | 900 | Wait for the image to pull and the container to start |
| `ELASTIC_MIGRATION_CONFIGMAP` / `_SECRET` / `_AUTH` | per environment | Override the Elasticsearch target (`basic` or `apikey`) |

The GitHub Actions workflow **Elastic Migration CI/CD** runs the same script for DEV and TEST through
`workflow_dispatch`, with an optional baseline. It runs every pending migration in a single run, so use it only with
indexing off. A hosted runner can time out before a long Job finishes; the Job keeps running, so check it with `oc`.

### Second Indexing Service

For the stepped migration, a second copy of the environment's live indexing service writes every content change
into the new indexes from `prepare` until cutover. It is a temporary Deployment, created from the live one and
deleted afterwards; nothing in the repository describes it.

| Setting | Value | Why |
| ------- | ----- | --- |
| `Kafka__Consumer__GroupId` | its own, e.g. `IndexingMigration` | A consumer group shared with the live service would split the messages between them. |
| `Kafka__Admin__ClientId`, `Kafka__Producer__ClientId` | the same | Keeps it apart from the live service in Kafka. |
| `Kafka__Consumer__AutoOffsetReset` | `Latest` | A new group otherwise starts at the oldest message, replaying 7 days of the `index` topic. |
| `Service__IndexOnly` | `true` | Stops a second set of alerts, notifications, status updates and `folder` topic messages. |
| `Elastic__ContentIndex` | the new unpublished content index (`CONTENT_INDEX` in the `prepare` log) | Where it writes. |
| `Elastic__PublishedIndex` | the new published content index (`PUBLISHED_INDEX`) | Where it writes. |
| `Elastic__EvidenceIndex` | the new evidence index (`EVIDENCE_INDEX`) | Where it writes. Set it explicitly: the live DEV Deployment does not reference it and uses the default `content_evidence`. |

Everything else (Elasticsearch URL and credentials, API, Kafka servers) stays as the live service has it, so it writes
to the same cluster as the migration. Create it after `prepare` and before `copy`, so no change made during the copy
is missed:

```bash
SRC=indexing-service          # TEST and PROD: indexing-service-cloud
NEW=indexing-service-migration

# 1. Copy the live Deployment, renamed, with no pods yet
oc get deploy $SRC -n $N -o json | jq --arg name $NEW '
  del(.metadata.uid, .metadata.resourceVersion, .metadata.creationTimestamp, .metadata.generation,
      .metadata.managedFields, .metadata.annotations, .status)
  | .metadata.name = $name | .metadata.labels.name = $name | .metadata.labels.component = $name
  | .spec.selector.matchLabels.name = $name | .spec.selector.matchLabels.component = $name
  | .spec.template.metadata.labels.name = $name | .spec.template.metadata.labels.component = $name
  | .spec.replicas = 0' | oc apply -n $N -f -

# 2. Point it at the new indexes, from the prepare log
oc set env deploy/$NEW -n $N \
  Kafka__Consumer__GroupId=IndexingMigration Kafka__Admin__ClientId=IndexingMigration \
  Kafka__Producer__ClientId=IndexingMigration Kafka__Consumer__AutoOffsetReset=Latest Service__IndexOnly=true \
  Elastic__ContentIndex=<CONTENT_INDEX> Elastic__PublishedIndex=<PUBLISHED_INDEX> Elastic__EvidenceIndex=<EVIDENCE_INDEX>

# 3. Start it and check it subscribes
oc scale deploy/$NEW -n $N --replicas=1
oc logs -f deploy/$NEW -n $N                 # "Subscribing to topics: index", then "Content indexed ... Index: <new index>"
```

The renamed labels keep its pods out of the live service's selector and Service. Use a group name that has never been
used: `AutoOffsetReset` only applies to a group with no saved position.

Keep it running until cutover has finished, then remove it:

```bash
oc delete deploy/$NEW -n $N
```

The consumer group `IndexingMigration` stays in Kafka with its last position; it does nothing once no service uses it.

## Watch Progress

```bash
oc get jobs -n $N -l component=elastic-migration --sort-by=.metadata.creationTimestamp
oc logs -f job/<job-name> -n $N

# Copy progress: compare "created" with "total"
es 'localhost:9200/_tasks?actions=*reindex&detailed=true' | grep -o '"total":[0-9]*\|"created":[0-9]*\|"running_time_in_nanos":[0-9]*'

# Disk while the copy runs
es 'localhost:9200/_cat/allocation?v'
```

## After It Succeeds

1. The aliases point at the `_v1.0.11-<generation>` indexes, including `content_evidence`.
2. `migrations` contains `1.0.11`.
3. Stepped: delete the second indexing service. Single run: turn indexing back on and wait for its Kafka lag to
   reach 0.
4. Test editor and subscriber search, published transcript filtering and report AI evidence.
5. Keep the `_v1.0.10` indexes until those checks and the retention window pass. Delete them later by exact name;
   never with a wildcard.

## Stop a Running Migration

Before cutover nothing users see has changed, so any step can be stopped. A cancelled copy is not counted as done:
the next `copy` copies again into the same indexes.

1. **Delete the Job.** `backoffLimit` is 0, so it is not retried; its PostgreSQL lock is released when the pod ends.

   ```bash
   oc delete job <job-name> -n $N --wait
   oc get pods -n $N | grep elastic-migration     # expect nothing
   ```

2. **Cancel the copy in Elasticsearch**, if one is running, and wait until no reindex task remains. The task ID is in
   the Job log and in `_tasks`. Deleting the Job alone leaves it running.

   ```bash
   es 'localhost:9200/_tasks?actions=*reindex'
   es -XPOST 'localhost:9200/_tasks/<task-id>/_cancel'
   es 'localhost:9200/_tasks?actions=*reindex'     # repeat until "nodes":{}
   ```

3. **Check the aliases did not move.**

   ```bash
   es 'localhost:9200/_cat/aliases?v' | grep -v '^\.'
   ```

To carry on later, rerun the step that was stopped; the second indexing service can keep running meanwhile. To give
up instead, delete the second indexing service, then the `_v1.0.11-<generation>` indexes by exact name, once no alias
uses them and no task is running:

```bash
es -XDELETE 'localhost:9200/unpublished_content_v1.0.11-<generation>'
es -XDELETE 'localhost:9200/content_v1.0.11-<generation>'
es -XDELETE 'localhost:9200/content_evidence_v1.0.11-<generation>'
```

History is only recorded after the alias switch, so a stopped run needs no rollback.

## If the Job Fails or Times Out

A Job that fails or times out does not stop the copy running inside Elasticsearch.

- **Let it finish and rerun.** Watch `_tasks/<task-id>`; when it completes, run the same step again. The new Job reuses
  the same indexes and reconnects to the finished task.
- **Or stop it** with the steps in [Stop a Running Migration](#stop-a-running-migration).
- **Repairs did not take.** A document still differs from the database at the same revision after it was repaired.
  The tool refuses to finish and leaves the aliases unchanged. Check the Job log for the index and IDs before
  rerunning.
- **A step says an earlier one has not completed.** Run the step it names first.
- **Cutover waits for replicas.** It waits up to an hour for the new indexes to be as healthy as the ones they
  replace, then fails with the aliases unchanged. Check disk and `_cat/shards`, then rerun `p=cutover`.
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

## Troubleshooting

| Symptom | Cause | Action |
| ------- | ----- | ------ |
| Copy is very slow | Elasticsearch CPU or heap limits | Check `_cat/nodes?v&h=name,heap.max,cpu,load_1m` and the pod limits |
| `disk usage exceeded flood-stage watermark`, indexes read-only | Copy filled the disk | Stop the run, free disk, delete the partial indexes, rerun |
| Cluster red after a restart | Pods restarted together | Wait for `_cluster/health`; use the partition steps next time |
| `content_evidence` alias cannot be created | A concrete `content_evidence` index exists | See [Before You Start](#before-you-start), step 5 |
| `Repairs to '<index>' did not take for content ...` | Differences the tool cannot fix | Check the Job log; aliases are unchanged |
| `Index '<index>' has not completed step '<step>'` | A step was run out of order | Run the named step first |
| Second indexing service logs `index_not_found_exception` | Its index names do not match the `prepare` log | Fix them with `oc set env`; `verify` repairs anything it missed |
| `Source index '<index>' was replaced; refusing to resume` | Source index changed between runs | Delete the partial indexes and rerun |
| Job succeeded but searches miss recent edits | Single run: indexing is still off, or still working through its Kafka backlog | Turn it back on; check the consumer group's lag |
