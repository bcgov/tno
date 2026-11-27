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

For each one, choose:

| Index kind                                                | Action                                                                                    |
| --------------------------------------------------------- | ----------------------------------------------------------------------------------------- |
| Behind a live alias (`content`, `unpublished_content`, `content_evidence`) or written by the app (`migrations`) | Reindex through a new Elasticsearch migration version (versioned copy, second indexer, alias cutover), run on 8.19. |
| Unused leftover (old versions, `*-backup-*`)              | Delete it, or mark it read-only with `PUT /<index>/_block/write` (sets `index.verified_read_only`; it stays read-only on 9.x). |
| System index (`.security-7`, …)                           | `GET /_migration/system_features`, then `POST /_migration/system_features` if it reports `MIGRATION_NEEDED`. |

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

**Not yet verified:** a 3-node rolling upgrade with DEV's ConfigMap (rehearse it in phase 1), and 7.x-created data
(the rehearsal volume was created on 8.19.25).

**Related, not changed here (decide separately):**

- `.github/workflows/elastic-migration-cicd.yml` now tests 8.19.25, 9.5.5 (the Cloud target) and 9.5.7 (7.17.9 dropped).
- `src/app` `@elastic/elasticsearch` 8.19.2 is only a type import (`esTypes.ts`); no runtime effect.
- `elasticsearch-mcp` (`docker.elastic.co/mcp/elasticsearch:0.4.6`) states 8.x and 9.x support, but the project is
  deprecated (security fixes only); Elastic's replacement is the Agent Builder MCP endpoint in 9.2+.

## 3. Local procedure (every developer)

1. If your volume dates from 7.17 (any developer who upgraded on 2026-11-01 rather than recreating), check for
   blockers first, on 8.19.25:
   `curl -s -u elastic:<password> 'localhost:40003/_migration/deprecations?pretty'`. Delete or rebuild any
   7.x-created index, or simply recreate the data (below).
2. Optional backup: `openshift/kustomize/elastic/scripts/snapshots.sh --local now pre-9-5-7` (restorable on 8.19 only).
3. `make refresh n=elastic`. The volume upgrades in place on first start.
4. `curl -u elastic:<password> localhost:40003` reports `"number" : "9.5.7"`; editor and subscriber searches work.

**Going back:** 8.19 cannot start on an upgraded volume. Restore the snapshot into an 8.19 container, or recreate:
`make down v=1`, `make up`, `bash do docker db-apply`, `make elastic-update`, then the indexer.

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
2. Run the Elasticsearch migration(s) that reindex the live 7.x-created indexes, then delete or `_block/write` the
   rest. Run `POST /_migration/system_features` if needed.
3. Re-run phase 0: `_migration/deprecations` must have no `critical` items.

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
