# Elasticsearch upgrade: 7.17.9 → 8.19.21 (Local and DEV)

TEST and PROD already run **8.19.21** on Elastic Cloud. This plan brings Local and DEV to the same version, so
every environment runs one Elasticsearch version.

| Environment | Cluster                                                    | Version on 2026-11-01 | Target  |
| ----------- | ---------------------------------------------------------- | --------------------- | ------- |
| Local       | `tno-elastic` container (`db/elasticsearch`)               | 7.17.9 → **8.19.21** (done) | 8.19.21 |
| DEV         | `elastic` StatefulSet, 3 nodes, `9b301c-dev`               | 7.17.9 → **8.19.21** (done) | 8.19.21 |
| TEST        | Elastic Cloud `test-mmi` (local StatefulSet scaled to 0)   | 8.19.21               | —       |
| PROD        | Elastic Cloud `prod-mmi` (local StatefulSet scaled to 0)   | 8.19.21               | —       |

Why 8.19.21 and not 9.x: Elasticsearch 9 cannot open indexes created on 7.x, and TEST/PROD are on 8.19. A
move to 9 is a later, all-environment change, and it needs every 7.x-created index reindexed first.

No application change is needed:

- NEST 7.17.5 (API, DAL, reporting) already sends the compatibility header (`EnableApiVersioningHeader()` in
  `libs/net/elastic/TNOElasticClient.cs`), which is how it works against TEST/PROD today.
- The indexing service and the migration tool use `Elastic.Clients.Elasticsearch` 8.19.27.

## 1. What changes

### Local (`db/elasticsearch`), done on 2026-11-01

| File                 | Change                                                                                              |
| -------------------- | --------------------------------------------------------------------------------------------------- |
| `Dockerfile`         | `FROM docker.elastic.co/elasticsearch/elasticsearch:8.19.21`                                         |
| `elasticsearch.yml`  | Remove the `xpack.monitoring` block (`enabled` was removed in 8.0; basic monitoring is always on).    |
| `elasticsearch.yml`  | Remove `discovery.seed_hosts` and `discovery.seed_providers`. They come from empty env vars, which 7.17 ignores but **8.x refuses to start with** (`null-valued setting found for key [discovery.seed_hosts]`). Single-node discovery needs neither. |

### DEV (`openshift/kustomize/elastic`), applied on 2026-11-01

| Where                         | Change                                                                                             |
| ----------------------------- | -------------------------------------------------------------------------------------------------- |
| `base/statefulset.yaml`       | Image `docker.elastic.co/elasticsearch/elasticsearch:8.19.21`.                                       |
| `base/config-map.yaml`        | Remove `node.data`, `node.ingest` and `node.master` (removed in 8.0).                                 |
| `base/config-map.yaml`        | Remove the `xpack.monitoring` block.                                                                  |
| `base/config-map.yaml`        | Remove `cluster.initial_master_nodes`. It only bootstraps a brand-new cluster, and 8.x warns when it is still set on one that has already formed. |
| `base/statefulset.yaml`       | Remove the `NODE_DATA`, `NODE_INGEST`, `NODE_MASTER` and `CLUSTER_INITIAL_MASTER_NODES` env vars (no longer referenced). |

All of the settings changes are valid on 7.17.9, so the repository describes the final state. The base is used
only by DEV: the TEST and PROD in-cluster clusters (scaled to 0) are being removed, because those environments
use Elastic Cloud. **Do not apply `overlays/test` or `overlays/prod`.**

Because the repository already carries the 8.19.21 image, DEV must not get a full `oc apply` until phase 4.
Phase 2 applies only the ConfigMap.

**Do not replace the removed role settings with `node.roles: [master, data, ingest]`**, even though the
deprecation message suggests it. DEV nodes currently have every role (`cdfhilmrstw`), because `true` was the
default for all three settings. An explicit list would silently drop the ml, transform, remote-cluster and
data-tier roles. Deleting the settings keeps the roles exactly as they are.

### Snapshots

Both environments get an `fs` snapshot repository. See [SNAPSHOTS.md](./SNAPSHOTS.md). This is a prerequisite:
the upgrade cannot be undone in place (a node that has run 8.x cannot go back to 7.17), so the rollback path is
a snapshot.

## 2. Already done (2026-11-01)

### Old 1.0.10 indexes reconciled and removed from DEV

The 1.0.11 migration left `content_v1.0.10` and `unpublished_content_v1.0.10` in DEV (no alias pointed at them).
They held about 45% of the cluster's disk. Before deleting them, every document id was compared with the 1.0.11
indexes (a sorted, point-in-time scan of each index):

| Old index                     | Docs      | In 1.0.11 | Missing from 1.0.11 | Outcome                                                                    |
| ----------------------------- | --------- | --------- | ------------------- | -------------------------------------------------------------------------- |
| `unpublished_content_v1.0.10` | 4,695,397 | 4,695,397 | 0                   | Nothing to copy.                                                           |
| `content_v1.0.10` (published) | 617,064   | 615,629   | 1,435               | Not copied. Every one is `status = 0` (Draft) in Postgres and in `unpublished_content`. They were stale published copies of content that later went back to draft, and copying them would publish drafts. |

Both indexes were then deleted. Disk use went from 76–77% to 41–42% per node, and the cluster stayed green.
`content_evidence-backup-20261009212403` (611 docs) was kept, because it is a separate backup taken during the
1.0.11 migration.

### Rehearsed locally

All of the following was run on local containers and passed:

- **Local image:** a 7.17.9 volume upgraded in place to 8.19.21. Data and aliases were intact, 7.17 snapshots
  restored on 8.19, and new snapshots and the nightly policy worked. This is what found the
  `discovery.seed_hosts` start failure.
- **DEV procedure:** a 3-node cluster with DEV's exact config map (roles `cdfhilmrstw` and the same four critical
  deprecations) and a shared snapshot volume, running:
  1. a snapshot,
  2. a settings-only rolling restart on 7.17.9 (critical deprecations went to none, roles unchanged),
  3. a node-by-node upgrade to 8.19.21 (green after every node),
  4. the `initial_master_nodes` clean-up,
  5. a full cold restart.

## 3. Decisions

1. **Dejavu removed (2026-11-01).** Live DEV allowed `dejavu.appbase.io` (the hosted Dejavu Elasticsearch browser)
   through CORS. It also had an `elastic-dejavu` route that exposed the whole cluster, which has security
   disabled, at `tno-dev-elastic.apps.silver.devops.gov.bc.ca`. Neither was used: the CORS entry was removed when
   the snapshot setup was applied, and the route was deleted. The `elastic` route (`/elastic` on
   `tno-dev.apps…`) is unchanged.
2. **VolumeSnapshot slots.** The namespace allows 5 and 2 are used by three-year-old snapshots
   (`crunchy-postgres-zhsm-pgdata-snapshot`, `data-elastic-0-snapshot`). The upgrade needs 3, one per data
   volume, which fits exactly. Delete the stale two only if you want spare slots.

## 4. Local procedure (every developer)

The repository is already on 8.19.21. Each developer's existing `tno-elastic-data` volume upgrades the first
time they run the steps below. The procedure was run on 2026-11-01 against a 7.17.9 volume with eight indexes:
document counts and aliases were unchanged, and both the 7.17 and 8.19 snapshots work.

1. Optional backup: take a snapshot first (`openshift/kustomize/elastic/scripts/snapshots.sh --local setup`,
   then `... --local now pre-8.19.21`). The snapshot volume survives the upgrade.
2. Pull the change and run `make refresh n=elastic`. The data volume upgrades in place on first start.
   - A few `failed to retrieve password hash for reserved user [elastic]` errors during the first start are
     expected while the security index recovers.
3. Check it: `curl -u elastic:<password> localhost:40003` should report `"number" : "8.19.21"`, and the editor
   and subscriber searches should work.

**Going back:** 7.17 cannot start on an upgraded volume. To run an older branch, either restore from a snapshot
into a 7.17 container, or recreate the data with `make down v=1`, then `make up`, `make elastic-update` and the
indexer.

## 5. DEV procedure

Work from `openshift/kustomize/elastic`, logged in with `oc login`. `overlays/dev` needs its gitignored
`secret.env` and `route.env` files. Every apply below is:

```bash
oc kustomize overlays/dev | oc apply -f -
```

Phase 2 applies only the ConfigMap (`rolling-update.sh config`); phase 4 is the first full apply. Pods mount the
config map through `subPath`, so applying a change does not restart anything. Each phase applies
with rollouts held, then moves one node at a time with `scripts/rolling-update.sh`. That script:

1. stops replica allocation and flushes,
2. recreates one pod (highest ordinal first),
3. waits for it to rejoin,
4. re-enables allocation and waits for green.

It accepts yellow only while node versions are mixed, which Elasticsearch documents as expected. It stops at the
first problem, and rerunning it resumes. `NS` defaults to `9b301c-dev`.

### Phase 0: preconditions (read-only)

```bash
NS=9b301c-dev scripts/rolling-update.sh status          # green, 3 nodes, all 7.17.9, one revision
oc exec -n 9b301c-dev elastic-0 -c elastic -- curl -s 'localhost:9200/_cat/allocation?v'   # disk < 80%
```

Resolve the decisions in section 3.

### Phase 1: snapshots (see SNAPSHOTS.md), applied on 2026-11-01

Done on 2026-11-01, while the base image was still 7.17.9. **Do not repeat the full apply below now**: the base
is 8.19.21, so it would start the upgrade.

```bash
NS=9b301c-dev scripts/rolling-update.sh hold
oc kustomize overlays/dev | oc apply -f -       # adds the elastic-snapshots PVC, mount and path.repo
NS=9b301c-dev scripts/rolling-update.sh run
scripts/snapshots.sh setup                      # must report every node in the _verify output
```

### Phase 2: settings only, still 7.17.9 (reversible)

Apply only the ConfigMap from the repository, then restart each node onto it with its current template (still
7.17.9):

```bash
NS=9b301c-dev scripts/rolling-update.sh config     # applies the ConfigMap only; nothing restarts
NS=9b301c-dev scripts/rolling-update.sh restart    # hold, stamp a restart annotation, roll one node at a time
oc exec -n 9b301c-dev elastic-0 -c elastic -- curl -s 'localhost:9200/_migration/deprecations?pretty'
```

Expect no `critical` items, and `_cat/nodes` still showing `cdfhilmrstw`.

### Phase 3: back up

```bash
scripts/snapshots.sh now pre-8-19-21 && scripts/snapshots.sh wait pre-8-19-21   # Elasticsearch snapshot (7.17)
scripts/snapshots.sh volume pre-8-19-21                                          # CSI snapshot of each data PVC
oc get volumesnapshot -n 9b301c-dev -l snapshot-label=pre-8-19-21                # all READYTOUSE=true
```

### Phase 4: upgrade (one-way)

Apply the full overlay (the 8.19.21 image and the env-var clean-up) and roll:

```bash
NS=9b301c-dev scripts/rolling-update.sh hold
oc kustomize overlays/dev | oc apply -f -
NS=9b301c-dev scripts/rolling-update.sh run
```

Do not run Elasticsearch migrations (`elastic-migration`) while versions are mixed. Indexing can keep running.

### Phase 5: clean up

The `initial_master_nodes` clean-up already happened in phases 2 and 4. Confirm the pods' logs do not mention
`initial_master_nodes`.

### Phase 6: verify

1. `scripts/rolling-update.sh status`: all three nodes on 8.19.21 with role `cdfhilmrstw`, and green.
2. `_cat/aliases` still maps `content`, `unpublished_content` and `content_evidence` to the 1.0.11 indexes.
3. Editor and subscriber searches work, along with publishing a story (index → published index → notification).
4. The indexing service and API logs show no Elasticsearch errors.
5. `scripts/snapshots.sh now post-8-19-21 && scripts/snapshots.sh wait post-8-19-21` succeeds.
6. `scripts/snapshots.sh policy` shows the next nightly run.
7. Once DEV has been stable for a few days, delete the volume snapshots:
   `scripts/snapshots.sh volume-delete pre-8-19-21`.

## 6. DEV upgrade record (2026-11-01)

Indexing kept running throughout; only the `elastic` ConfigMap and StatefulSet were changed.

| Phase | Result |
| ----- | ------ |
| 1     | Snapshot volume added; `initial-2026-11-01` took 10.4 minutes and 25 GB. |
| 2     | Settings rolled out on 7.17.9; no node or cluster deprecations left; roles unchanged (`cdfhilmrstw`). |
| 3     | `pre-8-19-21` Elasticsearch snapshot (5/5 shards). Volume snapshots `data-elastic-{0-retry,1,2}-pre-8-19-21` were taken, but see the note below about `data-elastic-0`. |
| 4     | Rolling upgrade; green after every node; all nodes on 8.19.21. |
| 6     | Aliases and document counts intact. NEST-style compatibility search works. No ERROR or `initial_master_nodes` lines in node logs. The indexing service wrote 49 documents during and after the upgrade with no failures, and the API pods show no Elasticsearch errors. `post-8-19-21` snapshot succeeded. |

**Volume-snapshot quirk:** twice, the `data-elastic-0` VolumeSnapshot came back with `status.creationTime`
2023-05-26, matching the old `data-elastic-0-snapshot` on that volume. It does not look like a new point-in-time
copy. The `data-elastic-1` and `data-elastic-2` snapshots are current. Because every index has two replicas,
those two volumes alone hold every shard, so a volume rollback restores elastic-1 and elastic-2 and lets elastic-0
recover from them. The `pre-8-19-21` Elasticsearch snapshot is the primary rollback. Deleting the stale 2023
snapshot would probably fix this for next time (not done; it is not part of this change).

**Remaining deprecations (only matter for 9.x):** 5 indexes were created on 7.x (*compatibility version < 8.0*)
and 7 have date-format patterns 9.x rejects. These need a reindex (a new migration version) before any move to
9.x.

## 7. Rollback

| When                                  | How                                                                                                      |
| ------------------------------------- | -------------------------------------------------------------------------------------------------------- |
| Phases 1–2                            | Re-apply the previous ConfigMap (`git show <previous>:…/base/config-map.yaml`), then run `rolling-update.sh restart`. Still 7.17.9; nothing is lost. |
| Phase 4, one node failing to start    | Stop. The other nodes keep serving. Read the pod log, fix the config, then `hold` → apply → `run` (resumes). |
| After any node runs 8.x               | The data volumes cannot go back to 7.17. Restore one of the following:                                   |
|                                       | **a. Volume snapshots (fastest):** scale `elastic` to 0, delete `data-elastic-N`, recreate each PVC with `dataSource` pointing at `data-elastic-N-pre-8-19-21`, restore the 7.17.9 image and config, then scale to 3. |
|                                       | **b. Elasticsearch snapshot:** start a fresh 7.17.9 cluster on empty volumes, then `snapshots.sh setup` and `snapshots.sh restore-in-place pre-8-19-21 <indices>`, then re-create the aliases. A 7.17 snapshot restores into 7.17; an 8.x snapshot does not. |
|                                       | **c. Rebuild:** DEV search data is a projection of Postgres. Create fresh indexes with the migration tool and reindex with the indexer (takes hours). |
