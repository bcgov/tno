# Elasticsearch upgrade: 8.19.25 → 9.5.7 (Local and DEV)

TEST and PROD (Elastic Cloud `test-mmi`, `prod-mmi`) are upgraded separately through Elastic Cloud. This plan
covers Local (`dependencies/elasticsearch`) and the DEV StatefulSet (`9b301c-dev`).

| Environment | Cluster                                                  | Version on 2026-11-27 | Target    |
| ----------- | -------------------------------------------------------- | --------------------- | --------- |
| Local       | `tno-elastic` container                                  | 8.19.25               | **9.5.7**, done 2026-11-27 |
| DEV         | `elastic` StatefulSet, 3 nodes, `9b301c-dev`             | 8.19.25               | **9.5.7**, done 2026-11-27 ([record](#dev-upgrade-record-2026-11-27)) |
| TEST        | Elastic Cloud (local StatefulSet scaled to 0)            | 8.19.26               | **9.5.7** (Cloud) |
| PROD        | Elastic Cloud (local StatefulSet scaled to 0)            | 8.19.21               | **9.5.7** (Cloud) |

Every environment targets 9.5.7 (decided 2026-11-27). TEST and PROD are upgraded through Elastic Cloud, not with
this plan, but the client changes and the 7.x-created index checks in section 1 apply to them too.

## Why 9.5.7 and not 9.5.5

Elastic only supports upgrading to a release that was **published after** the one you run
(<https://www.elastic.co/docs/deploy-manage/upgrade/deployment-or-cluster#out-of-order-releases>). The 8.19 and
9.5 lines are patched in parallel:

| Release | Published  |
| ------- | ---------- |
| 8.19.21 | 2026-09-01 |
| 9.5.5   | 2026-10-06 |
| 8.19.25 | 2026-11-10 |
| 8.19.26 | 2026-11-23 |
| 9.5.7   | 2026-11-23 |

Local and DEV run 8.19.25, which is newer than 9.5.5, so 9.5.5 is not a supported target. 9.5.7 is, and it also
carries the 9.5.6/9.5.7 security fixes and the `simple_pattern` tokenizer fix (shards failed to open on 9.x).
The same rule applies on Elastic Cloud. TEST runs 8.19.26, published after 9.5.5, so 9.5.5 is not a supported
target for TEST; PROD (8.19.21) could have taken it. TEST and PROD therefore also go to 9.5.7, so every environment
runs one version. Elastic Cloud may offer a later 9.5 patch by the time TEST and PROD are upgraded; the target must
still be published after 8.19.26.

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
| Behind a live alias (`content_evidence`) or written by the app (`migrations`) | `upgrade-9.sh rebuild <index>`: recreates it under the same name with the same mappings, settings, aliases and document versions (`_create_from` + `_reindex` with external versions, through a temporary copy `<index>-rebuild-9` that serves reads meanwhile). Stop every writer first; see [Rebuilding an index](#rebuilding-an-index). |
| Unused leftover (old versions, `*-rollback-*`, `*-backup-*`) | `upgrade-9.sh delete-index <index>`: one index name at a time (lists and wildcards are refused), and it refuses an index that has an alias. Or mark it read-only with `PUT /<index>/_block/write` (sets `index.verified_read_only`; it stays read-only on 9.x). |
| Data stream from 7.x (`ilm-history-5`, `.logs-deprecation.elasticsearch-default`) | `upgrade-9.sh delete-data-stream <name>`. Both are Elasticsearch's own history and logs and are recreated on demand. |
| System index (`.security-7`, `.tasks`, …)                 | `upgrade-9.sh system-features` (runs `POST /_migration/system_features` and waits). It fails when the migration ends in `ERROR`, printing the cause, or when it is not done within `SYSTEM_FEATURES_TIMEOUT` seconds (default 1800). |

Every command stops with an error when Elasticsearch does not answer or answers with anything but HTTP 2xx. A
stopped container, a refused connection or a timeout is therefore never read as "no aliases" or "already exists".
Index and data stream arguments are single lowercase names: `,` `*` `/` `?` `#` `"` and `_all` are refused. With
`--local` the password is read from `dependencies/elasticsearch/.env` and handed to `curl` on standard input, so it
does not appear in a process list.

#### Rebuilding an index

**Stop every writer first**, and keep them stopped until `rebuild` prints `Rebuilt '<index>'`:

| Writer | What it writes | Local | DEV |
| ------ | -------------- | ----- | --- |
| Indexing service | `content`, `unpublished_content` and `content_evidence`, through their aliases | `docker stop tno-indexing` | Scale every indexing deployment that points at this cluster to 0. |
| Elasticsearch migration tool | `migrations`; it also creates and copies indexes | Do not run `bash do docker elastic-update`. | Do not run the `elastic-migration` Job. |
| Indexer tool (`src/tools/indexer`) | Bulk writes to all three content indexes | Do not run it. | Do not run it. |
| API, orphaned-content clean-up | An administrator's **Delete** on the editor's *Orphaned Content* page (`DELETE /api/admin/orphaned-content/{id}`) deletes the document from all three content indexes through their aliases. The orphan scan itself only reads. | Do not use the page, or `docker stop tno-api`. | Make sure no administrator uses the page. The API has no setting that turns it off, so the only way to be certain is to scale the API to 0. |

`rebuild` works through a temporary copy named `<index>-rebuild-9`:

| Step | What it does | If it stops during this step |
| ---- | ------------ | ---------------------------- |
| 1 | Puts a write block on the original, creates the copy (`_create_from`) and copies every document with its version. The copy is checked twice: the `_reindex` response must report no failures, no time-out and no cancellation, and both document counts must match. | The original is intact. A rerun deletes the partial copy and starts again. |
| 2 | Puts a write block on the copy (`_create_from` does not carry the original's block over), refreshes it, then moves the aliases onto it in one atomic call. | The original is intact. A rerun starts again, or carries on from the copy once the aliases point at it. |
| 3 | Deletes the original. | **From here the copy is the only complete copy.** A rerun resumes from it. |
| 4 | Recreates the original from the copy, with the same two checks. | A rerun deletes the partial original and copies it again from the copy. |
| 5 | Moves the aliases back in one atomic call. | A rerun finishes the last two steps. |
| 6 | Deletes the copy. | A rerun finishes this step. |

- **Writes are refused, not lost.** From step 1 until step 5 the index that the aliases point at carries a write
  block. A writer that was left running gets `403 cluster_block_exception` on every index, update and delete, so
  the indexing service's write fails and the API answers an *Orphaned Content* delete with an error. Without the
  block on the copy, those writes would land in an index that step 6 deletes. Reads work throughout. The block
  covers writes made through an alias. An index that is written by its own name (`migrations`) does not exist
  between steps 3 and 4 and is open while step 4 fills it, so nothing protects a write to it then: do not run the
  migration tool during a rebuild.
- **After a failure, rerun the same command.** It works out how far the last run got from the two indexes (which
  one is older, which has a write block, where the aliases are, the document counts) and resumes as in the table. It
  never deletes the only complete copy. When it finds a state it did not leave itself, it changes nothing and prints
  what it found.
- **A copy can outlive the script.** `_reindex` keeps running inside Elasticsearch when the script is interrupted,
  or when one call takes longer than `ES_TIMEOUT` seconds (default 3600). A rerun then stops and names the running
  task. Wait until `GET /_tasks/<id>` answers 404, then rerun.
- **To abandon** a rebuild that stopped before step 3 (the script says `'<index>' is intact`), delete the copy and
  remove the original's write block. Until then the original refuses writes.

  ```bash
  upgrade-9.sh [--local] delete-index <index>-rebuild-9     # only if it exists
  upgrade-9.sh [--local] unblock <index>
  ```

  `delete-index` refuses the copy while it has aliases, while a copy task is running, or when the original is
  missing or holds fewer documents. `unblock` refuses while the copy exists. After step 3 a rebuild cannot be
  abandoned: rerun it.

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
| `openshift/kustomize/elastic/overlays/{test,prod}/kustomization.yaml` | `newTag: 8.19.21`: what they rendered before the base moved. Their StatefulSets are scaled to 0 and must not change version when an overlay is applied. |

No `elasticsearch.yml` or ConfigMap change is needed. None of the settings removed in 9.0 are used, and the 9.x
image runs as `1000:0` with the same entrypoint (Red Hat UBI 9 base instead of Ubuntu; the Local Dockerfile only uses
`mkdir`, `chown` and `chmod`). Verified on 2026-11-27:

- The Local Dockerfile and `elasticsearch.yml` start on 9.5.5/9.5.7 with no warnings, and the `fs` snapshot repository
  registers.
- A Local volume created on 8.19.25 (index + alias) started on 9.5.7: data and alias intact. The only warnings were
  the transient `failed to retrieve password hash for reserved user [elastic]` messages while `.security` recovers,
  the same as on 8.19.

A 3-node rolling upgrade with DEV's ConfigMap was rehearsed on 2026-11-27
([rehearsal record](#rehearsal-record-2026-11-27)). Data from 7.x was prepared and upgraded on Local (section 3).

**Related, not changed here (decide separately):**

- `.github/workflows/elastic-migration-cicd.yml` now tests 8.19.21 (PROD), 8.19.25 (Local, DEV), 8.19.26 (TEST)
  and 9.5.7 (every environment's target). 7.17.9 and 9.5.5 are no longer tested.
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

   The volume needs no clean-up when the deprecations are `none`, the system features report
   `NO_MIGRATION_NEEDED` and no index is listed. In that case still do step 2, then skip steps 3 to 5 and continue
   at step 6.

2. **Snapshot, always.** It is the only way back to 8.19, so take it and check it even when step 1 found nothing.
   It holds the application indexes (`content*`, `unpublished_content*`, `migrations`) and restores on 8.19 only.

   ```bash
   $S/snapshots.sh --local setup        # only if 'snapshots.sh --local list' says the repository is missing
   $S/snapshots.sh --local now pre-9-5-7 && $S/snapshots.sh --local wait pre-9-5-7
   $S/snapshots.sh --local list         # pre-9-5-7: SUCCESS, failed_shards 0
   curl -s -u elastic 'localhost:40003/_cat/aliases?h=alias,index&s=alias'    # keep this list
   ```

   `wait` fails unless the snapshot succeeded. `snapshots.sh restore-in-place` restores indexes without
   their aliases, so keep the alias list: going back needs it. `curl -u elastic` asks for the password instead of putting it on the command line.

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
   an index that an alias points at; `delete-index` takes one name at a time and refuses one that has an alias.

4. **Rebuild the live 7.x indexes** with every writer stopped (the table under
   [Rebuilding an index](#rebuilding-an-index)): stop the indexing service, do not run the migration tool or the
   indexer, and do not delete from the editor's *Orphaned Content* page. Kafka keeps the indexing service's messages
   until it restarts.

   ```bash
   docker stop tno-indexing
   $S/upgrade-9.sh --local rebuild migrations
   $S/upgrade-9.sh --local rebuild content_evidence_v1.0.11-<suffix>
   ```

   Each one must end with `Rebuilt '<index>'`. If a rebuild stops, read its last lines and rerun the same command:
   it resumes, and it says so when `<index>-rebuild-9` holds the only complete copy. Do not delete that index by
   hand and do not start the indexing service until the rebuild has finished.

5. **Migrate the system indexes**, then check again: the result must be the one described in step 1.

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
   - `curl -u elastic localhost:40003` reports `"number" : "9.5.7"`, and `_cluster/health` is yellow
     (single node; replicas cannot be assigned) with every primary active.
   - `_cat/aliases` and the `content`, `unpublished_content` and `content_evidence` counts match what you had before.
   - `tno-indexing` logs `Subscribing to topics: index`, and editor and subscriber searches work.
   - `$S/snapshots.sh --local policy` shows the next nightly run.

### Going back to 8.19.25 (Local)

8.19 does not start on a data volume that 9.x has opened. Going back means an empty Elasticsearch data volume and a
restore of `pre-9-5-7`. Replace **only** `tno-elastic-data`. Never use `bash do docker down -v` for this: it deletes
every volume of the project, including PostgreSQL (`tno-database-data`) and `tno-elastic-snapshots`, which holds the
snapshot.

```bash
docker stop tno-indexing              # it would create plain indexes named like the aliases before they exist
docker rm -f tno-elastic              # the container only
docker volume rm tno-elastic-data     # Elasticsearch data only; tno-elastic-snapshots and tno-database-data stay
```

Pin 8.19.25 in `dependencies/elasticsearch/Dockerfile` (do not commit it), then rebuild and restore:

```dockerfile
FROM docker.elastic.co/elasticsearch/elasticsearch:8.19.25
```

```bash
bash do docker refresh -n elastic     # builds 8.19.25 and starts it on a new, empty tno-elastic-data
$S/snapshots.sh --local setup         # registers the repository again; the snapshots are still in the volume
$S/snapshots.sh --local list          # pre-9-5-7 must be listed
$S/snapshots.sh --local restore-in-place pre-9-5-7 '<index>,<index>,…,migrations'
```

Name the index behind each alias in the list kept in step 2, plus `migrations`. When they are green, re-create each
alias (`POST /_aliases`, as in [SNAPSHOTS.md](SNAPSHOTS.md#restore)), compare the counts, then
`docker start tno-indexing`. The API and indexing builds from this branch work on 8.19 and need no change. Content
indexed after the snapshot is missing until it is indexed again. A snapshot taken on 9.5.7 stays listed, but 8.19
refuses to restore it.

The snapshot was taken before the clean-up, so the indexes restored from it are the 7.x-created ones again: run
`check` and repeat the steps it calls for before the next upgrade. Without a usable snapshot the search data has to be recreated from PostgreSQL on the empty
8.19.25 volume: `bash do docker elastic-update`, then the indexer (hours).

Verified on 2026-11-27 with throwaway containers and volumes (one node): an 8.19.25 volume with `pre-9-5-7` in its
snapshot volume was upgraded to 9.5.7, which then wrote `post-9-5-7` to the same repository. 8.19.25 exited on the
upgraded data volume. On a new data volume with the same snapshot volume, 8.19.25 registered the repository, listed
both snapshots, refused `post-9-5-7` and restored `pre-9-5-7` with its documents and their `_version`.

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
scripts/upgrade-9.sh check                             # what blocks 9.x: deprecations, system features, 7.x indexes
E="oc exec -n 9b301c-dev elastic-0 -c elastic -- curl -s"
$E 'localhost:9200/_cat/allocation?v'                                  # disk < 80% (room for reindex copies)
$E 'localhost:9200/_migration/deprecations?pretty'                     # list critical items
$E 'localhost:9200/_migration/system_features?pretty'
$E 'localhost:9200/_all/_settings/index.version.created?flat_settings=true&expand_wildcards=all&pretty'   # 7xxxxxx = 7.x
$E 'localhost:9200/_cat/aliases?v'                                     # keep this list: a restore needs it
$E 'localhost:9200/_all/_settings?filter_path=**.tokenizer&pretty'     # any simple_pattern tokenizer (fixed in 9.5.7)
oc exec -n 9b301c-dev elastic-0 -c elastic -- df -h /usr/share/elasticsearch/snapshots   # room for two more snapshots
```

Record the list of 7.x-created indexes and decide 1.2 for each. Keep the alias list: aliases are not part of a
snapshot, so the rollback re-creates them from it.

### Phase 1: rehearse locally

Reproduce DEV on Local containers, as was done for 8.19.21: a 3-node 8.19.25 cluster with DEV's ConfigMap, a shared
snapshot volume and an index created the same way. Run phases 2–5 against it, including a full cold restart at the
end. The rehearsal must also prove the two things DEV cannot be used to find out:

1. **An interrupted rebuild resumes.** Stop `upgrade-9.sh rebuild` part way (once before the original is deleted,
   once after), rerun it, and compare the documents and their `_version` with the snapshot.
2. **The rollback works** ([section 5](#5-rollback), row a). After the rehearsal cluster runs 9.5.7 and has written
   a snapshot of its own to the repository, as the nightly policy will on DEV, start a new 3-node 8.19.25 cluster
   on empty data volumes with the same snapshot volume. Run `snapshots.sh setup`, restore `pre-9-5-7`, re-create
   the aliases, and compare the document counts and a sample of `_version` values with the ones recorded before
   the upgrade. Write down the exact commands that worked, including how the new cluster was bootstrapped; they
   are the rollback procedure.

Do not touch DEV until this passes. Both items passed on three nodes on 2026-11-27; the record follows.
DEV was upgraded earlier that day, by decision, before the rehearsal had finished. The rehearsal was then run again
from the start, so the record describes one complete run.

#### Rehearsal record (2026-11-27)

**Set-up.** Throwaway Docker containers, volumes and network, removed afterwards. Three nodes named `elastic-0`,
`elastic-1` and `elastic-2`, on the stock `elasticsearch:8.19.25` and `elasticsearch:9.5.7` images. Every node ran
the `elasticsearch.yml` of `base/config-map.yaml` unchanged, with the StatefulSet's environment (`CLUSTER_NAME`,
`NODE_NAME`, `DISCOVERY_SEED_HOSTS=elastic-headless`, `path.repo`) and security off. Each node had its own data
volume, and all three mounted one snapshot volume. The indexes were created from the migration tool's own
definitions (1 shard, 2 replicas), each behind its alias:

| Index | Alias | Documents |
| ----- | ----- | --------- |
| `content_v1.0.13-…` | `content` | 20,000 |
| `unpublished_content_v1.0.12-…` | `unpublished_content` | 30,000 |
| `content_evidence_v1.0.11-…` | `content_evidence` | 150,000 (525,000 including nested) |
| `migrations` | none | 11 |

The three content indexes were written through their aliases with `version_type=external`; 92% of their documents
have a version above 1 (the highest is 33). All four indexes were created on 8.19.25: `rebuild` treats an index the
same whichever version created it, and the 7.x-created case was run on Local and on DEV. "Identical" below means,
for every index, the same document count and the same SHA-256 over every (`_id`, `_version`) pair.

| What | Result |
| ---- | ------ |
| 8.19.25 cluster | Formed on empty volumes with `cluster.initial_master_nodes` set for the first start (see [the bootstrap setting](#the-bootstrap-setting)): green, three nodes, roles `cdfhilmrstw`. `snapshots.sh setup`: all three nodes verified the repository. `upgrade-9.sh check`: nothing to do. |
| **Item 1: an interrupted rebuild resumes** | **Passed.** `upgrade-9.sh --local rebuild content_evidence_v1.0.11-…` was killed twice with `SIGKILL` to its whole process group. *Before the original was deleted*, 30,000 documents into the first copy: the original was intact, write-blocked and still behind the alias. A rerun refused to start while the orphaned `_reindex` task was still running and named the task; once the task had ended, the next run deleted the unused copy and started again. *After the original was deleted*, 30,000 documents into the copy back: `…-rebuild-9` held the alias and its write block. A rerun again refused while the task ran. The task was then cancelled, which left the recreated original with 40,000 of 150,000 documents; the next run said so, copied it again from `…-rebuild-9` and ended with `Rebuilt`. Afterwards all four indexes were identical to the baseline, and the index's mappings, settings and alias were unchanged. |
| Phase 3 | `pre-9-5-7`: 4 indexes, 4/4 shards. Counts, version hashes and the alias list recorded. |
| Phase 4 | One node at a time, by hand, in the steps of `rolling-update.sh run`: allocation `primaries`, flush, replace the container with 9.5.7 on the same data volume, wait for it to rejoin, re-enable allocation, wait for green. Order: `elastic-2`, `elastic-0`, then the elected master `elastic-1`. Green after every node, also while the versions were mixed. On 9.5.7 the indexes were identical to the recorded state and the aliases unchanged. |
| Phase 5 | On 9.5.7, documents were added, updated to higher versions and deleted. `post-9-5-7` (`snapshots.sh now`) and a nightly snapshot (`snapshots.sh run-policy`) were written to the same repository. |
| **Item 2: the rollback works** | **Passed**, with the `docker` equivalents of the steps under [Rollback a](#rollback-a-restore-pre-9-5-7-on-a-new-81925-cluster). The 9.5.7 nodes were stopped. 8.19.25 started on one of their data volumes exited with `fatal exception while booting Elasticsearch` (`indexCreatedVersionMajor is in the future: 10`). The three data volumes were deleted and recreated empty; the snapshot volume was kept. A new three-node 8.19.25 cluster formed with the bootstrap setting and held no index. `snapshots.sh setup` named all three nodes; `list` showed `pre-9-5-7-cleanup`, `pre-9-5-7`, `post-9-5-7` and the nightly. `restore-in-place post-9-5-7` was refused with HTTP 500, `snapshot_restore_exception … the snapshot was created with version [8.19.26-[9111002]] which is higher than the version of this node [8.19.9-8.19.25]`, and created nothing. `restore-in-place pre-9-5-7` with the four index names: green with every replica assigned, and no alias. After the aliases were re-created, the indexes were identical to the recorded state and the alias list matched. |
| The bootstrap setting removed | A node restarted with the setting still present only logged `this node is locked into cluster UUID […] but [cluster.initial_master_nodes] is set to […]; remove this setting to avoid possible data loss caused by subsequent cluster bootstrap attempts`. Each node was then replaced in turn (`elastic-2`, `elastic-1`, `elastic-0`) by one without the setting: green after each, no node reported the setting and no log mentioned it. A full cold restart followed (all three stopped, then started together): the same cluster UUID, green, indexes identical to the recorded state, aliases unchanged. |
| After the rollback | Writes through the aliases were accepted. 8.19.25 took a new snapshot (`post-rollback`, 4/4 shards) into the repository that also holds the 9.5.7 snapshots, and `snapshots.sh policy` showed the next nightly run. |

Not covered: the OpenShift commands themselves (no `oc` command was run; see
[Rollback a](#rollback-a-restore-pre-9-5-7-on-a-new-81925-cluster) for what each one stands on), DEV's data size
(about 24 GB) and therefore how long its restore takes, the real writers (indexing service, API), and a cluster
with security enabled.

### Phase 2: clear the blockers on 8.19.25 (reversible)

1. Deploy the 1.1 client change (API, services, migration tool image) to DEV and confirm searches, publishing, reports,
   Filter AI and the orphan scan still work on 8.19.25.
2. Take an Elasticsearch snapshot before anything is deleted. It is the way back from a mistake in this phase, and
   it is not the rollback snapshot: phase 3 takes that one, under another name, after the clean-up.

   ```bash
   scripts/snapshots.sh now pre-9-5-7-cleanup && scripts/snapshots.sh wait pre-9-5-7-cleanup
   ```

3. Same as the Local steps 3–5, without `--local` (`NS=9b301c-dev`): delete the unaliased leftovers and old data
   streams. Then stop every writer listed under [Rebuilding an index](#rebuilding-an-index): scale the indexing
   services to 0, run no `elastic-migration` Job and no indexer, and make sure no administrator deletes from the
   *Orphaned Content* page. `rebuild` each live 7.x index (expect `migrations` and `content_evidence_v1.0.11-…`);
   each must end with `Rebuilt '<index>'`. If one stops, rerun the same command and do not delete
   `<index>-rebuild-9` by hand. Then run `system-features`. Scale indexing back up only after the last rebuild has
   finished.
4. `scripts/upgrade-9.sh check` must report no critical deprecation, `NO_MIGRATION_NEEDED` and no index.

### Phase 3: back up

The Elasticsearch snapshot is the rollback. Take it after phase 2, as close to phase 4 as possible: everything
indexed after it has to be indexed again after a rollback.

```bash
scripts/snapshots.sh now pre-9-5-7 && scripts/snapshots.sh wait pre-9-5-7     # restorable on 8.19 only
scripts/snapshots.sh list                                                      # pre-9-5-7: SUCCESS, failed_shards 0
```

`wait` fails unless the snapshot succeeded. The repository PVC holds about two full copies, so check its free space
(phase 0) and delete snapshots taken with `now` that are no longer needed. `pre-9-5-7-cleanup` can go once DEV has
run on the cleaned-up indexes without a problem; keep `pre-9-5-7` until the upgrade is accepted. The nightly
policy's retention only removes the snapshots that the policy itself created.

Volume snapshots are an optional second copy, not the rollback:

```bash
scripts/snapshots.sh volume pre-9-5-7                                          # CSI snapshot of each data PVC
oc get volumesnapshot -n 9b301c-dev -l snapshot-label=pre-9-5-7                # all READYTOUSE=true
```

They are taken while Elasticsearch is running, one volume after another, so each one is crash-consistent at best and
the three are not from the same moment. Nothing checks that a cluster starts from them. Also check the
`data-elastic-0` volume snapshot's `creationTime`: it came back stale on 2026-11-01 (see UPGRADE-8.19.21.md).

### Phase 4: upgrade (one-way)

Do not run `elastic-migration` while versions are mixed. Indexing can keep running.

```bash
NS=9b301c-dev scripts/rolling-update.sh hold
oc kustomize overlays/dev | oc apply -f -
NS=9b301c-dev scripts/rolling-update.sh run
```

`oc kustomize overlays/dev` needs the gitignored `secret.env` and `route.env`, and applying it also replaces the
`elastic` Secret and Route. Without those files, or to change nothing but the nodes, apply only the StatefulSet:
render the overlay in a temporary copy with stub `secret.env` and `route.env` files, keep the `StatefulSet` document,
confirm the image is its only difference from the live object (compare `.spec` with `oc get sts elastic -o json`),
and `oc apply` that one document between `hold` and `run`. This is what was done on DEV on 2026-11-27.

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
   shows the next nightly. Do not delete `pre-9-5-7`: it is the only snapshot that 8.19 can restore.
7. After a few stable days: `scripts/snapshots.sh volume-delete pre-9-5-7` if volume snapshots were taken. Delete
   `pre-9-5-7` itself (`scripts/snapshots.sh delete pre-9-5-7`) only when going back to 8.19 is no longer wanted.

### DEV upgrade record (2026-11-27)

| Phase | Result |
| ----- | ------ |
| 0 | Green, 3 nodes on 8.19.25, `elastic-2` elected master. Blockers: `migrations` (11 documents, no alias) and `content_evidence_v1.0.11-…` (3,446 documents; 13,228 including nested; alias `content_evidence`), the data streams `ilm-history-5` and `.logs-deprecation.elasticsearch-default`, and system features `tasks` and `geoip`. No unaliased leftovers and no `simple_pattern` tokenizer. Data volumes 39% used; the snapshot volume 88% (6.1 GB free). |
| 1 | Not finished before the upgrade, by decision. Run again from the start later the same day; both items passed ([rehearsal record](#rehearsal-record-2026-11-27)). |
| 2 | Deleted the old snapshots `pre-8-19-25-20261119`, `post-8-19-25-20261119` and the 2026-11-26 nightly: the snapshot volume went to 47% (27 GB free). `pre-9-5-7-cleanup` taken. Both data streams deleted. `indexing-service` scaled to 0, both indexes rebuilt: document IDs and `_version` values identical to the baseline (hash compared), aliases unchanged. System features migrated; `check` clean. Indexing scaled back to 1. |
| 3 | `pre-9-5-7` (4 indexes, 4/4 shards). Volume snapshots `data-elastic-{0,1,2}-pre-9-5-7` ready, with fresh creation times. |
| 4 | Only the StatefulSet applied (the overlay's secret and route files were not available); its image was the only difference. `rolling-update.sh run` replaced `elastic-2`, `elastic-1`, `elastic-0` in that order; green after each. `elastic-1` is the elected master afterwards. |
| 5 | Green, 3 nodes on 9.5.7, roles `cdfhilmrstw`. Aliases unchanged (plus the system alias `.tasks` → `.tasks-reindexed-for-9` from the system-feature migration). `migrations` and `content_evidence` identical to the baseline. Searches through all three aliases work; no Elasticsearch errors in the indexing, API, `api-services` and reporting logs. `post-9-5-7` taken; the nightly policy runs next on 2026-11-28 09:30 UTC. On 9.5.7 the deprecation API lists every 8.x-created index and data stream as critical, and `upgrade-9.sh check` again shows `MIGRATION_NEEDED` for the system features: on 9.x these are what a future 10.x upgrade needs, and nothing blocks 9.x. The functional checks in phase 5 item 3 were done in the browser and passed. |

Keep `pre-9-5-7`: it is the only snapshot 8.19 can restore. `pre-9-5-7-cleanup` and the volume snapshots can be
deleted after a few stable days.

## 5. Rollback

Elasticsearch has no downgrade: 8.19 does not start on a data volume that 9.x has opened, and 8.19 refuses to
restore a snapshot taken on 9.x. Once any node runs 9.x, going back means new, empty data volumes.

| When                                 | How                                                                                       |
| ------------------------------------ | ----------------------------------------------------------------------------------------- |
| Phases 0–1                           | Nothing changed.                                                                          |
| Phase 2                              | Nothing upgraded. A rebuild that stopped: rerun it (or abandon it, see [Rebuilding an index](#rebuilding-an-index)). An index deleted by mistake: `snapshots.sh restore-in-place pre-9-5-7-cleanup <index>`, then re-create its aliases. Revert the client change if needed. |
| Phase 3                              | Nothing upgraded.                                                                         |
| Phase 4, a node fails to start       | Stop. The other nodes keep serving. Read the pod log, fix, then `hold` → apply → `run` (resumes). |
| After any node runs 9.x              | **a. Elasticsearch snapshot (the supported rollback).** Stop the writers and Elasticsearch, replace the three data volumes with empty ones, start a new 8.19.25 cluster and restore `pre-9-5-7` into it. The commands are under [Rollback a](#rollback-a-restore-pre-9-5-7-on-a-new-81925-cluster). Their Elasticsearch side was rehearsed on three nodes on 2026-11-27; the `oc` commands are a translation and were not run. Writes since the snapshot are lost; run the indexer for that window. |
|                                      | **b. Rebuild from PostgreSQL.** DEV search data is a projection of PostgreSQL. On the same new 8.19.25 cluster: create the indexes with the migration tool, then run the indexer (hours). Use it when the snapshot cannot be restored. |
|                                      | **c. Volume snapshots (best effort, only if they were taken).** Scale `elastic` to 0, recreate each `data-elastic-N` PVC from `data-elastic-N-pre-9-5-7`, restore the 8.19.25 image, scale to 3. They were taken from running nodes at different moments, so the cluster may not form or may recover with missing or stale shards. It is a last resort for when a cannot be used; be ready to fall back to b. |

### Rollback a: restore `pre-9-5-7` on a new 8.19.25 cluster

**What is verified and what is not.** Every step was carried out on 2026-11-27 on a local three-node cluster
([rehearsal record](#rehearsal-record-2026-11-27)), with `docker` commands standing in for `oc`. The lines that talk
to Elasticsearch (`snapshots.sh`, every line that starts with `$E`, and the two that build `INDICES` and `ALIASES`)
were run as written here against that cluster. **No `oc` command below has been run on OpenShift.** They are a
translation of the `docker` commands that worked. [What each step stands on](#what-each-step-stands-on) lists the
local equivalent of each one and what stays untested until this is done on DEV.

As a last check, every `bash` block of this section was run unchanged, in order, with a stand-in for `oc` first in
`PATH`. The stand-in turned `oc exec` into `docker exec` on the rehearsal node, applied `oc patch`, `oc scale` and
`oc get` of the StatefulSet to a local copy of the one rendered from `overlays/dev` (`kubectl patch --local`), and
only printed every other `oc` command. `rolling-update.sh` was replaced by a stub. That run restored `pre-9-5-7`
once more with the same counts, version hashes and aliases, and the two patches and their check lines produced
the output described in steps 4 and 9. It shows that the commands are well formed; it says nothing about OpenShift.

Work from `openshift/kustomize/elastic`, logged in with `oc login`:

```bash
export NS=9b301c-dev
E="oc exec -n $NS elastic-0 -c elastic -- curl -sS"
```

**1. Stop the writers.** Note how many replicas the indexing service runs (1 on 2026-11-27); step 10 scales it back.

```bash
oc get deployment -n $NS -o custom-columns=NAME:.metadata.name,REPLICAS:.spec.replicas | grep -E 'NAME|indexing'
oc scale deployment/indexing-service -n $NS --replicas=0
oc get pods -n $NS -l name=indexing-service          # repeat until: No resources found
```

Scale every other indexing deployment that points at this cluster the same way (`indexing-service-migration`, if a
stepped migration left one running). Run no `elastic-migration` Job and no indexer, and make sure no administrator
uses the *Orphaned Content* page until step 10. The indexing service must stay stopped until the aliases exist
again: started earlier, it would create plain indexes named like the aliases.

**2. Stop Elasticsearch.**

```bash
oc scale statefulset/elastic -n $NS --replicas=0
oc get pods -n $NS -l statefulset=elastic-cluster    # repeat until: No resources found (a pod has 120 s to stop)
```

**3. Delete the three data volumes and keep the snapshot volume.** 8.19.25 does not start on a volume that 9.5.7
has opened, so the data volumes cannot be reused.

```bash
oc get pvc -n $NS | grep -E 'NAME|elastic'           # data-elastic-0, data-elastic-1, data-elastic-2, elastic-snapshots
oc delete pvc data-elastic-0 data-elastic-1 data-elastic-2 -n $NS
oc get pvc -n $NS | grep -E 'NAME|elastic'           # only elastic-snapshots is left
```

Name the three claims. Never delete by label or with `--all`: `elastic-snapshots` carries the same
`component=elastic` label, and it holds `pre-9-5-7`, the only way back.

**4. Set the image to 8.19.25 and add the bootstrap setting**, while the StatefulSet has no pods. In the repository,
so that the overlay says what DEV runs and the next apply does not upgrade it again (commit this change):

```bash
sed -i 's/newTag: 9.5.7/newTag: 8.19.25/' overlays/dev/kustomization.yaml
```

On DEV, in one patch: no rollout hold left over (`partition: 0`), the image, and `cluster.initial_master_nodes`
(see [The bootstrap setting](#the-bootstrap-setting)):

```bash
oc patch statefulset elastic -n $NS --type json -p '[
  {"op":"replace","path":"/spec/updateStrategy","value":{"type":"RollingUpdate","rollingUpdate":{"partition":0}}},
  {"op":"replace","path":"/spec/template/spec/containers/0/image","value":"docker.elastic.co/elasticsearch/elasticsearch:8.19.25"},
  {"op":"add","path":"/spec/template/spec/containers/0/env/-","value":{"name":"cluster.initial_master_nodes","value":"elastic-0,elastic-1,elastic-2"}}]'
oc get statefulset elastic -n $NS -o jsonpath='{.spec.replicas}{" replica(s), partition "}{.spec.updateStrategy.rollingUpdate.partition}{"\n"}{.spec.template.spec.containers[0].image}{"\n"}{range .spec.template.spec.containers[0].env[*]}{.name}={.value}{"\n"}{end}'
```

The second command must print `0 replica(s), partition 0`, the 8.19.25 image, and
`cluster.initial_master_nodes=elastic-0,elastic-1,elastic-2` as the last variable. Do not `oc apply` the overlay
during the rollback: it sets `replicas: 3` and has no bootstrap setting, so it starts three nodes that wait for a
cluster that does not exist.

**5. Start the new cluster.**

```bash
oc scale statefulset/elastic -n $NS --replicas=3
oc rollout status statefulset/elastic -n $NS --timeout=15m
oc get pvc -n $NS | grep -E 'NAME|elastic'           # three new data-elastic-N claims, Bound, and elastic-snapshots
scripts/rolling-update.sh status                     # three pods on the 8.19.25 image; three nodes on 8.19.25; green
$E 'localhost:9200/_cat/indices?h=index'             # prints nothing: a new, empty cluster
```

Ready pods do not show that a cluster formed: the readiness probe answers before a master is elected. The proof is
the node list in `status`. If it shows an error instead of three nodes, read `oc logs elastic-0 -n $NS -c elastic
--tail=50`. `master not discovered yet … [cluster.initial_master_nodes] is empty on this node` means the patch in
step 4 did not reach the pods.

**6. Register the snapshot repository.** The snapshots are still in the volume; the new cluster does not know the
repository yet.

```bash
scripts/snapshots.sh setup       # its second line, {"nodes":{…}}, must name elastic-0, elastic-1 and elastic-2
scripts/snapshots.sh list        # pre-9-5-7: SUCCESS, failed_shards 0
```

`setup` also re-creates the nightly policy. The snapshots taken on 9.5.7 (`post-9-5-7`, the nightlies) are listed
too. 8.19.25 refuses to restore them (`the snapshot was created with version […] which is higher than the version
of this node`), and a refused restore creates nothing.

**7. Restore `pre-9-5-7`** under the original index names, taken from the snapshot itself, and wait for green.

```bash
INDICES="$($E 'localhost:9200/_snapshot/snapshots/pre-9-5-7?filter_path=snapshots.indices' \
  | python3 -c 'import json, sys; print(",".join(sorted(json.load(sys.stdin)["snapshots"][0]["indices"])))')"
echo "$INDICES"                  # four names: content_evidence_v…, content_v…, migrations, unpublished_content_v…
scripts/snapshots.sh restore-in-place pre-9-5-7 "$INDICES"
$E 'localhost:9200/_cluster/health?wait_for_status=green&wait_for_no_initializing_shards=true&timeout=60m&filter_path=status,timed_out,unassigned_shards'
$E 'localhost:9200/_cat/recovery?v&active_only=true&h=index,shard,type,stage,bytes_percent,target_node'
```

The health call returns when the cluster is green or after 60 minutes: it must say `"status":"green"` and
`"timed_out":false`. If the connection drops first, run it again. The last line shows what is still being copied,
from another terminal or afterwards (nothing listed means nothing is in progress).

**8. Re-create the aliases and compare.** `restore-in-place` restores the indexes without their aliases. DEV has
three (`content`, `unpublished_content`, `content_evidence`), and the migration tool names the index behind each
one `<alias>_v<version>…`; `migrations` has none. The first command stops with an error unless every alias matches
exactly one of the restored indexes; if it does, write the `_aliases` request by hand from the phase 0 list.

```bash
ALIASES="$(python3 -c '
import json, re, sys
indices, actions = sys.argv[1].split(","), []
for alias in ("content", "unpublished_content", "content_evidence"):
    found = [i for i in indices if re.match(re.escape(alias) + r"_v[0-9]", i)]
    if len(found) != 1:
        sys.exit(f"ERROR: expected exactly one index for alias {alias}, found {found}")
    actions.append({"add": {"index": found[0], "alias": alias}})
print(json.dumps({"actions": actions}))' "$INDICES")"
echo "$ALIASES"
$E -X POST -H 'Content-Type: application/json' localhost:9200/_aliases -d "$ALIASES"
$E 'localhost:9200/_cat/aliases?h=alias,index&s=alias'
$E 'localhost:9200/_cat/indices/content*,unpublished_content*,migrations?v&h=health,index,pri,rep,docs.count&s=index'
for name in content unpublished_content content_evidence migrations; do
  printf '%s ' "$name"; $E "localhost:9200/$name/_count?filter_path=count"; echo
done
```

The alias list must equal the one kept in phase 0, and the four counts the ones from before the upgrade (on
2026-11-27: `migrations` 11, `content_evidence` 3,446). The `.tasks` alias in the DEV upgrade record belonged to the
9.5.7 cluster's own system indexes and is not re-created.

**9. Remove the bootstrap setting** with a rolling restart. `hold` comes first: without it the StatefulSet restarts
the pods by itself, one after another, without waiting for green.

```bash
scripts/rolling-update.sh hold
oc patch statefulset elastic -n $NS --type strategic -p '{"spec":{"template":{"spec":{"containers":[{"name":"elastic","env":[{"name":"cluster.initial_master_nodes","$patch":"delete"}]}]}}}}'
scripts/rolling-update.sh run
oc get statefulset elastic -n $NS -o jsonpath='{range .spec.template.spec.containers[0].env[*]}{.name}{"\n"}{end}'
$E 'localhost:9200/_nodes/settings?filter_path=nodes.*.settings.cluster.initial_master_nodes'
```

`run` must end with three nodes on 8.19.25 and green. The fourth command must not list
`cluster.initial_master_nodes`, and the last must print `{}`: no node has the setting.

**10. Start the writers and check.**

```bash
oc scale deployment/indexing-service -n $NS --replicas=1      # the number noted in step 1
oc logs deployment/indexing-service -n $NS --tail=50          # Subscribing to topics: …; no Elasticsearch errors
scripts/snapshots.sh policy                                   # the next nightly run
```

Then check editor and subscriber searches. Content indexed after `pre-9-5-7` was taken is missing: run the indexer
for that window. Before upgrading again, run `scripts/upgrade-9.sh check`.

#### The bootstrap setting

A node on an empty data volume never starts a cluster by itself. It joins one that exists, or it forms one with the
nodes named in `cluster.initial_master_nodes`. DEV's ConfigMap has not set that since the 8.19.21 upgrade, because a
cluster that has formed must not keep it. Rehearsed without it: three 8.19.25 nodes on empty volumes all answered
the readiness probe (`/_cluster/health?local=true`, HTTP 200) while `/_cat/nodes` answered 503 and the log repeated
`master not discovered yet, this node has not previously joined a bootstrapped cluster, and
[cluster.initial_master_nodes] is empty on this node`. Restarted with the setting, the same nodes formed a cluster.

It can go in `elasticsearch.yml` (the ConfigMap) or in the pod's environment. Both formed a cluster in the
rehearsal. This procedure uses an **environment variable on the StatefulSet**:

- The Elasticsearch image turns a lowercase, dotted environment variable into a setting. `path.repo` is already set
  this way by `components/snapshots`, and the rehearsal used exactly this name and value on 8.19.25.
- It is one patch entry to add and one to remove, and `oc get statefulset elastic -o yaml` shows whether it is
  there. The ConfigMap holds `elasticsearch.yml` as one string, so adding a line means replacing the whole file.
- Removing it changes the pod template, so it goes through `rolling-update.sh`, and that restart proves that every
  node starts without it. The pods mount the ConfigMap with `subPath`, which is read only when a pod starts:
  removing the line there restarts nothing, and each node keeps the setting until whenever it next restarts.

The price is one more rolling restart, and the variable has to be removed with the patch in step 9. Do not count
on a later `oc apply` of the overlay to remove it: apply only removes what an earlier apply added. A node that
restarts while the variable is still set only logs a warning (`this node is locked into cluster UUID […] but
[cluster.initial_master_nodes] is set […]; remove this setting`), so an unplanned pod restart between steps 5 and 9
does no harm.

#### What each step stands on

| Step | Run locally in its place | Not covered until it is run on OpenShift |
| ---- | ------------------------ | ---------------------------------------- |
| 1, 10 | Nothing wrote during the rehearsal. After the restore, writes through the aliases with external versions were accepted. | Everything about the indexing service, the API and Kafka. |
| 2 | `docker stop -t 120` and `docker rm` of the three 9.5.7 containers. | The `oc` commands themselves. |
| 3 | `docker volume rm` of the three data volumes and `docker volume create` of three empty ones; the snapshot volume untouched. | That the StatefulSet's `volumeClaimTemplates` create new claims when it scales up, and what the storage class does with the old volumes and with the `data-elastic-N-pre-9-5-7` volume snapshots taken from them (row c may no longer be possible afterwards). |
| 4 | Each node started as `docker run -e cluster.initial_master_nodes=elastic-0,elastic-1,elastic-2 … elasticsearch:8.19.25`; the nodes reported the setting. Both patches were applied with `kubectl patch --local` to the StatefulSet rendered from `overlays/dev`, and gave the intended image, variables and partition; with `newTag: 8.19.25` the overlay renders the 8.19.25 image. | That the live StatefulSet accepts the patches, and that pods created by scaling up from 0 use the patched template. |
| 5 | The three containers started together and formed a green three-node cluster within a minute. | `oc rollout status`, scheduling and volume provisioning. `rolling-update.sh` was not run locally (it needs `oc`); it carried out the DEV upgrade on 2026-11-27. |
| 6, 7, 8 | The same commands, with `snapshots.sh --local` and `E="docker exec <node> curl -sS"`. | DEV's own index names, and how long about 24 GB takes to restore (the rehearsal's four indexes were small and restored in seconds). |
| 9 | Each node replaced in turn (`elastic-2`, `elastic-1`, `elastic-0`) by a container without the variable, in the steps of `rolling-update.sh run`; then a full cold restart. | `rolling-update.sh hold` and `run` around this patch. |
