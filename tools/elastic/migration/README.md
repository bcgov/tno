# Elasticsearch migration 1.0.11

The tool coordinates native Elasticsearch `_reindex` tasks. It does **not** rebuild the whole
content corpus from PostgreSQL. It copies both existing content indexes, setting Elasticsearch's
external `_version` from `projectionRevision` (legacy missing revisions start at zero). It then
hydrates current analyses and content with system topics from PostgreSQL, scans lightweight
ID/revision projections, and repairs missing/stale documents and removes extras in bounded batches.
Published documents never retain unapproved audio/video transcript bodies or their analysis.
The evidence index is populated and checked against current analyses.

This is a **maintenance-window migration**. All database and index writers must remain paused
from before the copy starts until verification, alias cutover, and migration-history recording
succeed. Routing the web apps to a maintenance page alone does not stop background writers.
The tool requires `Elastic__WritersPaused=true`; this is an operator acknowledgement, not a mechanism
that pauses services. A PostgreSQL session advisory lock prevents two migration coordinators for
the same database from running concurrently.

## Targets and prerequisites

| Environment | Primary Elasticsearch | Index ConfigMap | Credential Secret |
| --- | --- | --- | --- |
| DEV | OpenShift Elasticsearch | `indexing-service` | `elastic` (basic auth) |
| TEST / PROD | Elasticsearch Cloud | `indexing-service-cloud` | `elastic-cloud` (API key) |

Read the actual `CONTENT_INDEX` and `PUBLISHED_INDEX` values from the selected ConfigMap; names
are different between DEV and Cloud. Only the primary cluster is migrated. Additional OpenShift
storage in TEST does not increase Elasticsearch Cloud storage. Check Cloud capacity separately;
PROD does not need additional OpenShift Elasticsearch storage to migrate its Cloud cluster.

Before starting:

1. Take/verify a usable Elasticsearch snapshot and database backup. Check the destination cluster's
   health, shard allocation, disk watermarks, and free storage. `_reindex` still needs a second copy
   of primary data plus replicas and indexing overhead. The tool preserves each source index's
   shard/replica counts instead of imposing the old three-replica template. Concrete-index backup
   clones preserve replica counts too; allow additional temporary space if the storage cannot
   hard-link the source segments. Never disable production disk watermarks to force the migration.
2. Verify PostgreSQL has the content-analysis schema and `content.projection_revision` column.
   Inspect the target cluster's migration history. DEV was at 1.0.10; the inspected TEST history
   was empty and PROD's migration index was absent. Verify the existing schema before explicitly
   baselining those clusters to 1.0.10. A baseline records history; it does not apply missing changes.
3. Ensure the Elasticsearch credential can read source indexes/settings, create/write destination
   indexes and mappings, read cluster tasks/health, manage aliases, clone concrete indexes, and
   write migration history. Concrete-index replacement also requires deleting that concrete name.
   The backup clone is checked for recovered primaries before replacement.
4. Record desired replica counts and CronJob suspension states. Pause API/API-services and all
   ingestion, content-editing, indexing (including Cloud), analysis, automation, and other workers
   or jobs that can change content or its projections. Stop external writers too. Drain in-flight
   requests. Preserve Kafka consumer groups/offsets; do not delete topics or reset offsets. Keep
   PostgreSQL, Kafka, and the primary Elasticsearch cluster running.

## Failed DEV run: old partial destinations

The original database-rebuild migration left `content_v1.0.11`, `unpublished_content_v1.0.11`, and
possibly `content_evidence_v1.0.11`. The new tool **never automatically deletes** an existing index
without its own `_meta.owner = tno-native-1.0.11` marker. It refuses and names the index instead.

Before explicitly deleting any old partial destination, inspect `GET /_alias`, the index's mapping,
and `GET /_tasks?actions=*reindex&detailed=true`. Confirm no alias uses it and no process/task writes
it. Remove only those verified partial destinations. Keep the live 1.0.10 indexes. Do not use a
wildcard deletion command. An index with native migration metadata is normally resumed, not deleted.

## Build and run

From the repository root, after preparing the maintenance window:

```bash
make -C openshift build n=elastic-migration t=latest
make -C openshift push n=elastic-migration t=latest
ELASTIC_MIGRATION_WRITERS_PAUSED=true \
  make -C openshift deploy n=elastic-migration e=dev t=latest m=1.0.11
```

For a verified Cloud schema without history, explicitly supply the baseline:

```bash
ELASTIC_MIGRATION_WRITERS_PAUSED=true ELASTIC_MIGRATION_BASELINE=1.0.10 \
  make -C openshift deploy n=elastic-migration e=test t=latest m=1.0.11
# Use e=prod for the PROD Cloud primary, after validating TEST.
```

The GitHub Actions workflow uses the same deployment script. Its manual inputs require the writer
pause acknowledgement and optionally a baseline. It does not pause/resume services or routes itself.
A hosted runner may time out before a long Job finishes; inspect the Job or reconnect using the CLI.

| Script environment variable | Default | Purpose |
| --- | --- | --- |
| `ELASTIC_MIGRATION_WRITERS_PAUSED` | false | Required maintenance acknowledgement; pass per invocation |
| `ELASTIC_MIGRATION_BASELINE` | empty | Baseline only when history is absent |
| `ELASTIC_MIGRATION_REQUESTS_PER_SECOND` | -1 | New-task document throttle; positive integer or unlimited (-1) |
| `MIGRATION_ACTIVE_DEADLINE_SECONDS` | 86400 | Elasticsearch Job execution limit; DB Jobs still default to 1800 |
| `MIGRATION_STARTUP_TIMEOUT_SECONDS` | 900 | Separate wait for image/container startup |

Direct tool invocations use `Elastic__WritersPaused`, `Elastic__BaselineVersion`, and
`Elastic__ReindexRequestsPerSecond`. `Elastic__NumberOfShards` and `Elastic__NumberOfReplicas`
can explicitly override preserved topology. `Elastic__EvidenceIndex` defaults to `content_evidence`.

## Recovery and cutover

Destination mapping `_meta` persists the source index name/UUID, Elasticsearch task ID, and copy
completion. A replacement Job reconnects to that task. `X-Opaque-Id` identifies a task launched just
before the coordinator lost its response. If the server loses a task (for example after a node
restart), the tool clears the stale task ID and fails with instructions to rerun. The next run
replays into the retained destination using external versions, without overwriting newer repairs.
Task failures never count as completion. A replaced source UUID or unmanaged destination fails closed.

**Stopping or timing out the Kubernetes Job does not cancel a server-side Elasticsearch task.**
Keep writers paused. Inspect the logged task with `GET /_tasks/<task-id>`; either let it finish and
rerun, or cancel it explicitly with `POST /_tasks/<task-id>/_cancel` and wait for cancellation before
cleanup. Never delete a destination while its task is running. Existing tasks keep their original
throttle; use `POST /_reindex/<task-id>/_rethrottle?requests_per_second=...` to change it.

The migration compares database IDs/revisions with both `_source.projectionRevision` and `_version`
in the target. Counts alone are insufficient. It repairs differences and refuses to complete if
repairs make no progress. It also verifies published membership and evidence membership. With
writers paused, aliases move in one atomic request and are checked again before history is recorded.
If the alias name is currently a concrete index (common on Cloud), the tool clones it to a backup,
waits for the backup's primaries, and replaces the concrete name with the alias in the same request.
A crash after alias cutover is recoverable: rerunning reuses the targets and verifies before recording
history. Migration history uses a stable version ID so a retry cannot create duplicate records.

After success, inspect aliases, test editor/subscriber search and published transcript filtering,
then restore the recorded service/Job settings. Keep the old indexes/backups until the application
checks and the retention window pass. Old copies cost storage; deletion is an explicit later action.

Rollback to `m=1.0.10` also requires a maintenance window and storage. It performs a native copy into
rollback destinations, removes analysis fields from the copied documents, repairs from PostgreSQL,
and switches aliases. It retains the 1.0.11/evidence indexes. Repeated upgrade/rollback cycles may
encounter existing destinations from an earlier cycle; inspect and archive/clean these explicitly
rather than deleting a live target or bypassing ownership/source checks.

## Retire OpenShift Elasticsearch in TEST/PROD

The TEST/PROD overlays and `make up` indexing defaults keep local `elastic` StatefulSet and local `indexing-service` Deployment replicas
to zero. `indexing-service-cloud` remains enabled. Before applying that state, confirm API, reporting,
search, evidence lookup, and all relevant services use the Cloud endpoint and smoke-test Cloud reads
and writes. Then stop the secondary local writer and cluster:

```bash
oc scale deployment/indexing-service --replicas=0 -n 9b301c-test
oc scale statefulset/elastic --replicas=0 -n 9b301c-test
oc scale deployment/indexing-service --replicas=0 -n 9b301c-prod
oc scale statefulset/elastic --replicas=0 -n 9b301c-prod
```

Keep the Cloud writer running outside the migration maintenance window. Scaling down retains local
PVCs and therefore does not release storage quota. Reclaim local PVCs only after snapshots and Cloud
verification, checking the PV reclaim policy first. This branch does not delete cluster data or apply
these operational changes automatically.

## Verification

```bash
dotnet build tools/elastic/migration/
python3 -m unittest discover -s openshift/scripts/tests -v
# Point ONLY to an explicitly disposable, local Elasticsearch instance:
MMI_ELASTIC_TEST_URL=http://127.0.0.1:19274 \
MMI_MIGRATION_TEST_POSTGRES='Host=127.0.0.1;Port=15439;Database=postgres;Username=postgres;Password=migration-test-only' \
  dotnet test tools/elastic/tests/
```

The native integration tests create uniquely named indexes and delete only their own indexes. Without
`MMI_ELASTIC_TEST_URL`, Elasticsearch tests are skipped. They exercise real Painless transformations,
legacy version zero and subsequent writes, published transcript filtering, restart recovery, failed
and missing tasks, source replacement, and rollback transformation. The PostgreSQL-backed test
creates a uniquely named database, verifies targeted repairs/evidence membership, conversion of
concrete indexes to aliases, restart after cutover, and rollback, then drops only that test database.
It requires `MMI_MIGRATION_TEST_POSTGRES` with a local role allowed to create/drop databases.
CI runs the suite on Elasticsearch 7.17.9 (DEV) and 8.19.21 (the inspected Cloud version).

The comparison checks membership and revisions, not every original document field. Legacy revision
zero assumes the old indexed document was correct before revision tracking; equal-revision body
corruption cannot be detected without reading/reconstructing those bodies. Current analyses and
system-topic content are explicitly rehydrated.

References: [Elasticsearch 7.17 reindex](https://www.elastic.co/guide/en/elasticsearch/reference/7.17/docs-reindex.html),
[task management](https://www.elastic.co/guide/en/elasticsearch/reference/7.17/tasks.html).
