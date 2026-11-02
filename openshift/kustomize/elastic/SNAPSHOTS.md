# Elasticsearch snapshots (Local and DEV)

Snapshots back up the application indexes to a shared filesystem repository named `snapshots`. A lifecycle
policy named `nightly` takes one every day and prunes old ones. Everything here works on 7.17.9 and 8.19.21, and
a 7.17 snapshot can be restored on 8.19. Day-to-day work goes through one script:

```bash
openshift/kustomize/elastic/scripts/snapshots.sh [--local] <command>   # no arguments prints the help
```

The script targets DEV by default (`NS=9b301c-dev`, through `oc exec` into `elastic-0`). `--local` targets the
`tno-elastic` container and reads `ELASTIC_PASSWORD` from `db/elasticsearch/.env`. Every command fails when
Elasticsearch returns an error.

TEST and PROD use Elastic Cloud, which has its own managed snapshots (the `found-snapshots` repository). This
setup is only for the self-hosted Local and DEV clusters.

## What is snapshotted

| Setting       | Value                                                                         | Override with                  |
| ------------- | ----------------------------------------------------------------------------- | ------------------------------ |
| Indexes       | `content*`, `unpublished_content*`, `migrations` (no system indexes or cluster state) | `INDICES`                      |
| Schedule      | `0 30 9 * * ?`: 09:30 UTC, 01:30 Pacific standard time                        | `SCHEDULE`                     |
| Retention     | expire after 14 days, always keep at least 3, never more than 14              | `RETAIN_DAYS`, `RETAIN_MIN`, `RETAIN_MAX` |

Overrides take effect when `setup` is run again. Snapshots are incremental: the first one copies every segment,
and later ones copy only new segments. Aliases are not part of a restore; the indexes are versioned, and the
aliases are recreated as part of a recovery.

## How it is wired

| Piece                         | Local                                                         | DEV                                                                    |
| ----------------------------- | ------------------------------------------------------------- | ---------------------------------------------------------------------- |
| Repository storage            | Docker volume `tno-elastic-snapshots`                          | PVC `elastic-snapshots`: 50 Gi, `ReadWriteMany`, `netapp-file-backup` (the platform also backs it up off-site) |
| Mounted at                    | `/usr/share/elasticsearch/snapshots`                          | `/usr/share/elasticsearch/snapshots` on every node                    |
| `path.repo`                   | `db/elasticsearch/elasticsearch.yml`                          | `path.repo` env var (the image turns dotted env vars into settings)   |
| Defined in                    | `docker-compose.yml`, `db/elasticsearch/Dockerfile`           | `components/snapshots`, included by `overlays/dev` only               |

The Local Dockerfile pre-creates the mount directory owned by `elasticsearch`. A new Docker volume copies that
ownership, so Elasticsearch can write to it.

### Sizing (DEV)

After the 1.0.10 indexes were removed on 2026-11-01, the primary data is about 24 GB (`unpublished_content`
about 21 GB, `content` about 2.7 GB). A full snapshot is roughly that size, and nightly increments are small. The
PVC is 50 Gi, leaving room for 14 days of changes and growth. Measured on 2026-11-01: the first full snapshot (`initial-2026-11-01`, 5 indexes) took 10.4 minutes with
indexing running and used 25 GB (49%) of the volume. Check usage with:

```bash
oc exec -n 9b301c-dev elastic-0 -c elastic -- df -h /usr/share/elasticsearch/snapshots
```

To grow it, raise `storage` in `components/snapshots/pvc.yaml` and apply (NetApp volumes expand online).
Namespace quota on 2026-11-01: `netapp-file-backup` 39 / 256 Gi with 3 / 5 PVCs used, so this PVC fits.

## Set up

### Local

```bash
make refresh n=elastic                                          # rebuild with the snapshot volume and path.repo
openshift/kustomize/elastic/scripts/snapshots.sh --local setup  # register, verify, create the nightly policy
```

### DEV

Applied on 2026-11-01. See [UPGRADE-8.19.21.md](./UPGRADE-8.19.21.md) phase 1. The base now carries the 8.19.21
image, so on a 7.17.9 cluster never run a full overlay apply just to change snapshot settings; follow that plan's
phases. The new volume needs a rolling restart, which is
`hold` → apply → `run`. Then run `scripts/snapshots.sh setup`. Its `_verify` output must list all three nodes;
that proves every node can write to the shared volume.

## Use

```bash
S=openshift/kustomize/elastic/scripts/snapshots.sh
$S list                        # snapshots, state, duration
$S policy                      # nightly schedule, last success or failure, next run
$S now before-change           # take one now (names must be lowercase, no spaces)
$S wait before-change          # block until it finishes; fails unless it succeeded
$S run-policy                  # run the nightly policy immediately
$S delete before-change
```

## Restore

**Side by side (default; nothing is overwritten):**

```bash
$S restore nightly-2026.11.02-abc123                       # every snapshotted index, as restored_<index>
$S restore nightly-2026.11.02-abc123 'unpublished_content*'
```

Compare the `restored_*` indexes with the live ones. To switch to the restored data without downtime, move the
alias in one atomic call:

```bash
curl -XPOST localhost:9200/_aliases -H 'Content-Type: application/json' -d '{"actions":[
  {"remove":{"index":"unpublished_content_v1.0.11-…","alias":"unpublished_content"}},
  {"add":{"index":"restored_unpublished_content_v1.0.11-…","alias":"unpublished_content"}}]}'
```

**In place** (the index must not exist; delete it first). This is for rebuilding a cluster from scratch:

```bash
$S restore-in-place pre-8-19-21 'content_v1.0.11-…,unpublished_content_v1.0.11-…,content_evidence_v1.0.11-…,migrations'
```

Then re-create the `content`, `unpublished_content` and `content_evidence` aliases.

## Whole-volume snapshots (DEV, CSI)

For changes that cannot be undone inside Elasticsearch, such as a version upgrade, also take a point-in-time
copy of each data volume:

```bash
$S volume pre-change           # VolumeSnapshot data-elastic-N-pre-change for each data PVC (class: netapp)
$S volume-list
$S volume-delete pre-change    # free the slots once the change is proven
```

- The namespace allows **5** VolumeSnapshots, so keep them only for the duration of a change.
- Snapshots of running nodes are crash-consistent. For a guaranteed-consistent copy, scale `elastic` to 0 first.
- To restore: scale `elastic` to 0, delete `data-elastic-N`, recreate it with
  `spec.dataSource: {kind: VolumeSnapshot, apiGroup: snapshot.storage.k8s.io, name: data-elastic-N-pre-change}`
  (same name, storage class and size), then scale back up.

## Later: S3 repository (no PVC)

Once DEV runs 8.x, the `repository-s3` module is built in. A repository on BC Gov object storage
(`*.objectstore.gov.bc.ca`) would remove the PVC. This needs:

1. A bucket or prefix and access keys, held in the Elasticsearch keystore (`s3.client.default.access_key` and
   `secret_key`, added by an init container).
2. `s3.client.default.endpoint` and `path_style_access: true`.
3. A pass of the repository analysis API (`POST _snapshot/<repo>/_analyze`) to prove the object store behaves
   the way Elasticsearch needs.

Treat this as an option, not a requirement.

## Troubleshooting

| Symptom                                                                 | Cause and fix                                                                                     |
| ----------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------- |
| `setup`: `repository_verification_exception … access_denied_exception` | Elasticsearch cannot write to the mount. Check with `oc exec … elastic-0 -c elastic -- sh -c 'id; ls -ld /usr/share/elasticsearch/snapshots; touch /usr/share/elasticsearch/snapshots/x && rm /usr/share/elasticsearch/snapshots/x'`. For NFS the directory must be writable by the pod's UID or GID 0; ask the platform team for group 0 write if needed. Locally, recreate the volume after `make refresh n=elastic`. |
| `setup`: `path.repo` / `location … doesn't match any of the locations` | The pod has not restarted since the component was applied. Run `rolling-update.sh run`.         |
| Snapshot `PARTIAL` or `FAILED`                                          | `$S status <name>` and `GET _snapshot/snapshots/<name>`. Usually a red index or a full volume.  |
| A VolumeSnapshot's `status.creationTime` is years old although it was just created | Seen on `data-elastic-0`, which still has a 2023 VolumeSnapshot (`data-elastic-0-snapshot`); the new one appears to resolve to that old snapshot. Do not rely on it. Delete the stale snapshot and take a new one, or rely on the Elasticsearch snapshot and the other nodes' volumes (every index has two replicas). |
| 8.x first start logs `failed to retrieve password hash for reserved user [elastic]` | Expected while the security index recovers; it clears on its own.                       |
