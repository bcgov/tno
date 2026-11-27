# Elasticsearch upgrade: 8.19.25 → 9.5.7 (Local and DEV)

TEST and PROD (Elastic Cloud `test-mmi`, `prod-mmi`) are upgraded separately through Elastic Cloud. This plan
covers Local (`dependencies/elasticsearch`) and the DEV StatefulSet (`9b301c-dev`).

| Environment | Cluster                                                  | Version on 2026-11-27 | Target    |
| ----------- | -------------------------------------------------------- | --------------------- | --------- |
| Local       | `tno-elastic` container                                  | 8.19.25               | **9.5.7** |
| DEV         | `elastic` StatefulSet, 3 nodes, `9b301c-dev`             | 8.19.25               | **9.5.7** |
| TEST        | Elastic Cloud (local StatefulSet scaled to 0)            | 8.19.26               | Cloud     |
| PROD        | Elastic Cloud (local StatefulSet scaled to 0)            | 8.19.21               | Cloud     |

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
patch. PROD (8.19.21) can move to 9.5.5. TEST runs 8.19.26, which was published after 9.5.5, so 9.5.5 is not a
supported target for TEST.

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

**Not yet verified:** a 3-node rolling upgrade with DEV's ConfigMap (rehearse it in phase 1). Data from 7.x was
prepared and upgraded on Local (section 3).

**Related, not changed here (decide separately):**

- `.github/workflows/elastic-migration-cicd.yml` now tests 8.19.25, 9.5.5 and 9.5.7 (7.17.9 dropped). It does not
  test what Elastic Cloud runs today (TEST 8.19.26, PROD 8.19.21); which versions to test for them is undecided.
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

   `wait` fails unless the snapshot succeeded. Aliases are not part of a snapshot, so keep the alias list: going
   back needs it. `curl -u elastic` asks for the password instead of putting it on the command line.

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

Do not touch DEV until this passes. The one-node equivalent of item 2 passed on 2026-11-27 (see
[Going back to 8.19.25 (Local)](#going-back-to-81925-local)); the 3-node case is not verified yet.

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

## 5. Rollback

Elasticsearch has no downgrade: 8.19 does not start on a data volume that 9.x has opened, and 8.19 refuses to
restore a snapshot taken on 9.x. Once any node runs 9.x, going back means new, empty data volumes.

| When                                 | How                                                                                       |
| ------------------------------------ | ----------------------------------------------------------------------------------------- |
| Phases 0–1                           | Nothing changed.                                                                          |
| Phase 2                              | Nothing upgraded. A rebuild that stopped: rerun it (or abandon it, see [Rebuilding an index](#rebuilding-an-index)). An index deleted by mistake: `snapshots.sh restore-in-place pre-9-5-7-cleanup <index>`, then re-create its aliases. Revert the client change if needed. |
| Phase 3                              | Nothing upgraded.                                                                         |
| Phase 4, a node fails to start       | Stop. The other nodes keep serving. Read the pod log, fix, then `hold` → apply → `run` (resumes). |
| After any node runs 9.x              | **a. Elasticsearch snapshot (the supported rollback).** Rehearsed in phase 1; follow the commands recorded there. In outline: stop the writers (indexing services to 0), scale `elastic` to 0, delete the three `data-elastic-N` PVCs and keep the `elastic-snapshots` PVC, set `newTag: 8.19.25` in `overlays/dev`, and start a new cluster. A new cluster needs `cluster.initial_master_nodes` for its first start, which the ConfigMap no longer sets; add it for that start and remove it afterwards. Then `snapshots.sh setup` (all three nodes must verify), `snapshots.sh restore-in-place pre-9-5-7 <indices>`, re-create the aliases from the phase 0 list, compare the counts, and start the writers. Writes since the snapshot are lost; run the indexer for that window. |
|                                      | **b. Rebuild from PostgreSQL.** DEV search data is a projection of PostgreSQL. On the same new 8.19.25 cluster: create the indexes with the migration tool, then run the indexer (hours). Use it when the snapshot cannot be restored. |
|                                      | **c. Volume snapshots (best effort, only if they were taken).** Scale `elastic` to 0, recreate each `data-elastic-N` PVC from `data-elastic-N-pre-9-5-7`, restore the 8.19.25 image, scale to 3. They were taken from running nodes at different moments, so the cluster may not form or may recover with missing or stale shards. It is a last resort for when a cannot be used; be ready to fall back to b. |
