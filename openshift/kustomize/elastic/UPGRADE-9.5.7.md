# Elasticsearch upgrade: 8.19.25 → 9.5.7 (Local and DEV)

TEST and PROD (Elastic Cloud `test-mmi`, `prod-mmi`) are upgraded separately through Elastic Cloud. This plan
covers Local (`dependencies/elasticsearch`) and the DEV StatefulSet (`9b301c-dev`).

| Environment | Cluster                                                  | Version on 2026-11-27 | Target    |
| ----------- | -------------------------------------------------------- | --------------------- | --------- |
| Local       | `tno-elastic` container                                  | 8.19.25               | **9.5.7** |
| DEV         | `elastic` StatefulSet, 3 nodes, `9b301c-dev`             | 8.19.25               | **9.5.7** |
| TEST        | Elastic Cloud (local StatefulSet scaled to 0)            | 8.19.x                | Cloud     |
| PROD        | Elastic Cloud (local StatefulSet scaled to 0)            | 8.19.x                | Cloud     |

## Why 9.5.7 and not 9.5.5

Elastic only supports upgrading to a release that was **published after** the one you run
(<https://www.elastic.co/docs/deploy-manage/upgrade/deployment-or-cluster#out-of-order-releases>). The 8.19 and
9.5 lines are patched in parallel:

| Release | Published  |
| ------- | ---------- |
| 8.19.21 | 2026-09-01 |
| 9.5.5   | 2026-10-06 |
| 8.19.25 | 2026-11-10 |
| 9.5.7   | 2026-11-23 |

Local and DEV run 8.19.25, which is newer than 9.5.5, so 9.5.5 is not a supported target. 9.5.7 is, and it also
carries the 9.5.6/9.5.7 security fixes and the `simple_pattern` tokenizer fix (shards failed to open on 9.x).
For TEST/PROD on Elastic Cloud, apply the same rule: the Cloud target must be published after their current 8.19
patch.

## 1. Blockers: resolve before any 9.x node starts

### 1.1 NEST 7.17.5 does not work against 9.x (done in this change)

`src/libs/elastic` (`TNOElasticClient`, which derives from NEST's `ElasticClient`) and the Elasticsearch migration
tool (`src/tools/elastic/migration`) use NEST / Elasticsearch.Net 7.17.5 with `EnableApiVersioningHeader()`. That
sends `compatible-with=7`; 9.x accepts only 8 or 9. Verified on 2026-11-27:

- `curl -H 'Accept: application/vnd.elasticsearch+json;compatible-with=7' …` → `400 media_type_header_exception`
  on 9.5.5; `compatible-with=8` and `9` succeed.
- `dotnet test src/tools/elastic/tests/` against 9.5.7: **10 of 18 fail** with
  `unsuccessful product check call … Status code 400 from: GET /`. NEST's product check is its first call, so every
  NEST call fails.

Affected NEST call sites: the API (`Areas/Helpers/OrphanScanService.cs`, `OrphanedContentService.cs`,
`FilterAITools.cs`), the DAL (`Services/ReportEvidenceService.cs`), and the whole migration tool. Calls made through
`TNOElasticClient`'s own `HttpClient` (`SearchAsync`, `SearchRawAsync`, `ValidateAsync`) send no compatibility
header and are not affected.

**Fix (this change):** NEST and Elasticsearch.Net are removed. `TNO.Elastic` now uses `Elastic.Clients.Elasticsearch`
**8.19.27** (already used by the indexing service, folder collection, indexer and migration tool), and
`ITNOElasticClient.RequestAsync(method, path, body)` replaces the NEST low-level calls in the API, the DAL and the
migration tool. An 8.x client sends `compatible-with=8`, which works against **both** 8.19 (TEST/PROD until Cloud
upgrades) and 9.x. Verified on 2026-11-27: the migration test suite passes on 8.19.25 and 9.5.7, and the API, DAL and
services tests pass.

Do not move to the 9.x client (latest 9.5.3) until every environment, including Cloud, runs 9.x: a 9.x client does
not support an 8.x server.

Deploy the API, the services and the `elastic-migration` image built from this change to DEV **before** phase 4 below.
Every developer's local volume upgrades one way the first time they rebuild `elastic`, so the client change and the
image change ship together.

### 1.2 Indexes created on 7.x

9.x reads indexes created on 8.x or later. A node **refuses to start** while any 7.x-created index is neither
reindexed nor marked verified read-only. Local and DEV were upgraded in place from 7.17.9 on 2026-11-01. At that
time DEV had 5 indexes created on 7.x and 7 with date-format patterns that 9.x rejects (see
[UPGRADE-8.19.21.md](UPGRADE-8.19.21.md) section 6). Migrations 1.0.12 and 1.0.13 (2026-11-06) replaced some of them,
but this has not been re-checked. Phase 0 lists what remains.

`scripts/upgrade-9.sh [--local] check` lists every blocker. For each one, choose:

| Index kind                                                | Action                                                                                    |
| --------------------------------------------------------- | ----------------------------------------------------------------------------------------- |
| Behind a live alias (`content_evidence`) or written by the app (`migrations`) | `upgrade-9.sh rebuild <index>`: recreates it under the same name with the same mappings, settings, aliases and document versions (`_create_from` + `_reindex` with external versions, through a temporary copy that serves reads meanwhile). Stop the indexing service first. |
| Unused leftover (old versions, `*-rollback-*`, `*-backup-*`) | `upgrade-9.sh delete-index <index>` (refuses an index with an alias), or mark it read-only with `PUT /<index>/_block/write` (sets `index.verified_read_only`; it stays read-only on 9.x). |
| Data stream from 7.x (`ilm-history-5`, `.logs-deprecation.elasticsearch-default`) | `upgrade-9.sh delete-data-stream <name>`. Both are Elasticsearch's own history and logs and are recreated on demand. |
| System index (`.security-7`, `.tasks`, …)                 | `upgrade-9.sh system-features` (runs `POST /_migration/system_features` and waits).       |

Restoring a 7.x-created index into 9.x needs the Enterprise-licensed archive feature, so **snapshots taken on 7.17
cannot be restored on 9.x**. Delete the 7.17 snapshots (`initial-2026-11-01`, `pre-8-19-21`) or accept that they are
only useful on an 8.19 cluster.

### 1.3 Deprecations

`GET /_migration/deprecations` on 8.19 must report **no `critical`** cluster, node or index items. Re-check after
1.1 and 1.2.

## 2. What changes in the repository

| File                                                | Change                                    |
| --------------------------------------------------- | ----------------------------------------- |
| `dependencies/elasticsearch/Dockerfile`             | `FROM …/elasticsearch:9.5.7`              |
| `openshift/kustomize/elastic/base/statefulset.yaml` | image `…/elasticsearch:9.5.7`             |
| `openshift/kustomize/elastic/overlays/dev/kustomization.yaml` | `newTag: 9.5.7`                 |

No `elasticsearch.yml` or ConfigMap change is needed. None of the settings removed in 9.0 are used, and the 9.x
image runs as `1000:0` with the same entrypoint (Red Hat UBI 9 base instead of Ubuntu; the Local Dockerfile only uses
`mkdir`, `chown` and `chmod`). Verified on 2026-11-27:

- The Local Dockerfile and `elasticsearch.yml` start on 9.5.5/9.5.7 with no warnings, and the `fs` snapshot repository
  registers.
- A Local volume created on 8.19.25 (index + alias) started on 9.5.7: data and alias intact. The only warnings were
  the transient `failed to retrieve password hash for reserved user [elastic]` messages while `.security` recovers,
  the same as on 8.19.

**Not yet verified:** a 3-node rolling upgrade with DEV's ConfigMap (rehearse it in phase 1). Data from 7.x was
prepared and upgraded on Local (section 3).

**Related, not changed here (decide separately):**

- `.github/workflows/elastic-migration-cicd.yml` now tests 8.19.25, 9.5.5 (the Cloud target) and 9.5.7 (7.17.9 dropped).
- `src/app` `@elastic/elasticsearch` 8.19.2 is only a type import (`esTypes.ts`); no runtime effect.
- `elasticsearch-mcp` (`docker.elastic.co/mcp/elasticsearch:0.4.6`) states 8.x and 9.x support, but the project is
  deprecated (security fixes only); Elastic's replacement is the Agent Builder MCP endpoint in 9.2+.

## 3. Local procedure (every developer)

Run from the repository root on this branch, with the Local stack up (`tno-elastic` on 8.19.25). Every step before
step 6 runs on 8.19 and can be undone from the snapshot. Performed on 2026-11-27 against a volume upgraded from 7.17.9
on 2026-11-01; the results are under [Local upgrade record](#local-upgrade-record-2026-11-27).

```bash
S=openshift/kustomize/elastic/scripts
```

1. **List the blockers** (read-only):

   ```bash
   $S/upgrade-9.sh --local check
   ```

   Nothing listed under the three headings means the volume is ready: go to step 6.

2. **Snapshot** the application indexes (`content*`, `unpublished_content*`, `migrations`). The snapshot restores on
   8.19 only, so it is the way back if a later step goes wrong.

   ```bash
   $S/snapshots.sh --local setup        # only if 'snapshots.sh --local list' says the repository is missing
   $S/snapshots.sh --local now pre-9-5-7 && $S/snapshots.sh --local wait pre-9-5-7
   ```

3. **Delete the unused 7.x indexes and data streams** that `check` listed without an alias. On a volume from
   2026-11-01 these are:

   ```bash
   for index in content_v1.0.10 content_v1.0.10-rollback-from-1.0.11 unpublished_content_v1.0.10 \
                unpublished_content_v1.0.10-rollback-from-1.0.11 \
                content_v1.0.11-<suffix> unpublished_content_v1.0.11-<suffix>; do
     $S/upgrade-9.sh --local delete-index "$index"
   done
   $S/upgrade-9.sh --local delete-data-stream ilm-history-5
   $S/upgrade-9.sh --local delete-data-stream .logs-deprecation.elasticsearch-default
   ```

   Use the names `check` printed. `<suffix>` is the 1.0.11 generation, e.g. `d39c00372638898aaad34724`. Never delete
   an index that an alias points at; `delete-index` refuses one.

4. **Rebuild the live 7.x indexes** with the indexing service stopped, so nothing writes to them during the copy.
   Kafka keeps its messages until it restarts.

   ```bash
   docker stop tno-indexing
   $S/upgrade-9.sh --local rebuild migrations
   $S/upgrade-9.sh --local rebuild content_evidence_v1.0.11-<suffix>
   ```

   Each copy is checked by document count, and the script stops on a mismatch.

5. **Migrate the system indexes**, then check again. All three headings must be empty.

   ```bash
   $S/upgrade-9.sh --local system-features
   $S/upgrade-9.sh --local check
   ```

6. **Upgrade.** Elasticsearch, then the API and the indexing service, whose old builds still use NEST and fail
   against 9.x. Then start the rest as usual.

   ```bash
   bash do docker refresh -n elastic
   bash do docker refresh -n api
   bash do docker refresh -n indexing
   ```

7. **Verify:**
   - `curl -u elastic:<password> localhost:40003` reports `"number" : "9.5.7"`, and `_cluster/health` is yellow
     (single node; replicas cannot be assigned) with every primary active.
   - `_cat/aliases` and the `content`, `unpublished_content` and `content_evidence` counts match what you had before.
   - `tno-indexing` logs `Subscribing to topics: index`, and editor and subscriber searches work.
   - `$S/snapshots.sh --local policy` shows the next nightly run.

**Going back:** 8.19 cannot start on an upgraded volume. Either restore `pre-9-5-7` into an 8.19.25 container on an
empty volume (`snapshots.sh --local restore-in-place pre-9-5-7 <indices>`, then re-create the aliases), or recreate
the data: `bash do docker down -v`, `bash do docker up`, `bash do docker db-apply`, `bash do docker elastic-update`,
then the indexer (hours).

### Local upgrade record (2026-11-27)

| Step | Result |
| ---- | ------ |
| 1 | 8 indexes and 2 data streams with *compatibility version < 8.0*; system features `MIGRATION_NEEDED` (`geoip`, `tasks`). The live `content` (1.0.13) and `unpublished_content` (1.0.12) indexes were created on 8.x and needed nothing. |
| 2 | `pre-9-5-7`: 11 indexes, 1.6 s. |
| 3 | Deleted the six unaliased 1.0.10/1.0.11 leftovers (about 640 MB) and both data streams. |
| 4 | `migrations` (11 documents) and `content_evidence_v1.0.11-…` (11,937 documents; 43,208 including nested) rebuilt. Every evidence document's `_version` matched a restored copy of the snapshot (0 mismatches, 0 missing). |
| 5 | No critical deprecations; `NO_MIGRATION_NEEDED`. |
| 6 | Started on 9.5.7: the same 15 primary shards active, and only the transient `.security` password-hash warnings while it recovered. API and indexing rebuilt and healthy. |
| 7 | Index and alias document counts identical before and after. The client's request shapes (point-in-time read, mapping, search, count, `_doc`, `_explain`, delete, HEAD) all answered as expected on 9.5.7 with security enabled. |

## 4. DEV procedure

Work from `openshift/kustomize/elastic`, logged in with `oc login`, with the gitignored `overlays/dev/secret.env` and
`route.env`. Every node move goes through `scripts/rolling-update.sh` (`hold` → apply → `run`). It stops replica
allocation, flushes, recreates one pod (highest ordinal first), waits for it to rejoin, re-enables allocation and
waits for green. It accepts yellow only while versions are mixed, which is expected: replicas of a primary on a 9.x
node cannot be allocated to an 8.19 node.

### Phase 0: preconditions (read-only)

```bash
NS=9b301c-dev scripts/rolling-update.sh status        # green, 3 nodes, all 8.19.25, one revision
E="oc exec -n 9b301c-dev elastic-0 -c elastic -- curl -s"
$E 'localhost:9200/_cat/allocation?v'                                  # disk < 80% (room for reindex copies)
$E 'localhost:9200/_migration/deprecations?pretty'                     # list critical items
$E 'localhost:9200/_migration/system_features?pretty'
$E 'localhost:9200/_all/_settings/index.version.created?flat_settings=true&expand_wildcards=all&pretty'   # 7xxxxxx = 7.x
$E 'localhost:9200/_cat/aliases?v'
$E 'localhost:9200/_all/_settings?filter_path=**.tokenizer&pretty'     # any simple_pattern tokenizer (fixed in 9.5.7)
```

Record the list of 7.x-created indexes and decide 1.2 for each.

### Phase 1: rehearse locally

Reproduce DEV on Local containers, as was done for 8.19.21: a 3-node 8.19.25 cluster with DEV's ConfigMap, a shared
snapshot volume and an index created the same way. Run phases 2–5 against it, including a full cold restart at the
end. Do not touch DEV until this passes.

### Phase 2: clear the blockers on 8.19.25 (reversible)

1. Deploy the 1.1 client change (API, services, migration tool image) to DEV and confirm searches, publishing, reports,
   Filter AI and the orphan scan still work on 8.19.25.
2. Take the phase 3 Elasticsearch snapshot now, before anything is deleted.
3. Same as the Local steps 3–5, without `--local` (`NS=9b301c-dev`): delete the unaliased leftovers and old data
   streams, then scale the indexing services to 0 and `rebuild` each live 7.x index (expect `migrations` and
   `content_evidence_v1.0.11-…`), then `system-features`. Scale indexing back up afterwards.
4. `scripts/upgrade-9.sh check` must list nothing.

### Phase 3: back up

```bash
scripts/snapshots.sh now pre-9-5-7 && scripts/snapshots.sh wait pre-9-5-7     # restorable on 8.19 only
scripts/snapshots.sh volume pre-9-5-7                                          # CSI snapshot of each data PVC
oc get volumesnapshot -n 9b301c-dev -l snapshot-label=pre-9-5-7                # all READYTOUSE=true
```

Check the `data-elastic-0` volume snapshot's `creationTime` (it came back stale on 2026-11-01; see UPGRADE-8.19.21.md).
Delete any snapshot taken with `now` that is no longer needed first; the PVC holds about two full copies.

### Phase 4: upgrade (one-way)

Do not run `elastic-migration` while versions are mixed. Indexing can keep running.

```bash
NS=9b301c-dev scripts/rolling-update.sh hold
oc kustomize overlays/dev | oc apply -f -
NS=9b301c-dev scripts/rolling-update.sh run
```

Elastic recommends upgrading the elected master last. All three nodes are master-eligible; if `elastic-0` is not the
elected master (`_cat/nodes`, `master` column `*`), consider moving the master first or accept the extra election.

### Phase 5: verify

1. `scripts/rolling-update.sh status`: all three nodes on 9.5.7, roles unchanged (`cdfhilmrstw`), green.
2. `_cat/aliases` unchanged from phase 0; document counts unchanged.
3. Editor and subscriber searches, publishing a story (index → published index → notification), reports with
   charts, the orphan scan.
4. No Elasticsearch errors in the indexing service, API and `api-services` logs.
5. `_migration/deprecations` on 9.5.7, for items to fix before 10.x.
6. `scripts/snapshots.sh now post-9-5-7 && scripts/snapshots.sh wait post-9-5-7`, and `scripts/snapshots.sh policy`
   shows the next nightly.
7. After a few stable days: `scripts/snapshots.sh volume-delete pre-9-5-7`.

## 5. Rollback

| When                                 | How                                                                                       |
| ------------------------------------ | ----------------------------------------------------------------------------------------- |
| Phases 0–3                           | Nothing upgraded. Revert the client change if needed.                                     |
| Phase 4, a node fails to start       | Stop. The other nodes keep serving. Read the pod log, fix, then `hold` → apply → `run` (resumes). |
| After any node runs 9.x              | Elasticsearch has no downgrade, and a 9.x snapshot cannot be restored on 8.x. Use one of: |
|                                      | **a. Volume snapshots:** scale `elastic` to 0, recreate each `data-elastic-N` PVC from `data-elastic-N-pre-9-5-7`, restore the 8.19.25 image, scale to 3. |
|                                      | **b. Elasticsearch snapshot:** fresh 8.19.25 cluster on empty volumes, `snapshots.sh setup`, `snapshots.sh restore-in-place pre-9-5-7 <indices>`, re-create the aliases. Writes since the snapshot are lost; re-run the indexer for that window. |
|                                      | **c. Rebuild:** DEV search data is a projection of Postgres: fresh indexes with the migration tool, then the indexer (hours). |
