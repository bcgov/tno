# Confluent Kafka

- [Learn Kafka Videos](https://developer.confluent.io/learn-kafka/)
- [Confluent Platform](https://docs.confluent.io/platform/current/overview.html)
- [Demo](https://github.com/confluentinc/cp-demo)
- [Examples](https://github.com/confluentinc/examples)
- [REST API v3 Demo](https://github.com/confluentinc/demo-scene/tree/master/adminrest)
- [REST Proxy API](https://docs.confluent.io/platform/current/kafka-rest/api.html)
- [Demo-Scene](https://github.com/confluentinc/demo-scene)

## Components

| Name            | Default Port | Description                               |
| --------------- | -----------: | ----------------------------------------- |
| Broker          |         9092 | Kafka server manages data and REST API v3 |
| Schema Registry |         8081 | Manages data schema                       |
| Schema Connect  |         8083 | Connect schema with Control Center        |
| KSQL DB         |         8088 | Streaming services                        |
| REST API        |         8082 | REST API v2                               |
| Kafbat Kafka-UI            |         8080 | UI to manage Kafka                        |

## Kafka-UI (Kafbat)

The image is pinned to `ghcr.io/kafbat/kafka-ui:v1.5.0` ([upstream release](https://github.com/kafbat/kafka-ui/releases/tag/v1.5.0)).
Configuration uses [Kafbat environment variables](https://ui.docs.kafbat.io/configuration/configuration-file).
Dynamic cluster configuration is disabled. The JVM heap is capped at 512 MiB within a 1 GiB container limit.

### Local Docker

From the repository root, start the UI and broker:

```bash
docker-compose -f docker-compose.yml -f docker-compose.override.yml \
  -f dependencies/kafka/docker-compose.yml -f src/services/docker-compose.yml \
  --profile kafka-ui up -d --build kafka-ui
```

The local UI binds only to loopback and does not require login. Open http://localhost:40180. Set `KAFKA_UI_PORT` in the root `.env` to override the port;
existing `KAFKA_KOWL_PORT` settings remain a fallback. No UI-specific `.env` is required.
The local Kafka UI image is built from `dependencies/kafka-ui/`. Its settings are in
`dependencies/kafka/docker-compose.yml`; it does not require a local `.env` file.
`make move` relocates any custom Kafka UI environment files from either the old
`db/kafka/kafka-ui/` or `dependencies/kafka/kafka-ui/` directory.

Kowl's source directory has been removed. Its old `.env` files are unused and are
not migrated. Remove the old local container with
`docker rm -f tno-kowl` if it exists after verifying the replacement.

### OpenShift migration

The manifests use `bcgov-c4awhwfpcremdbga.azurecr.io/kafka-ui:v1.5.0`.
Import the image and check the ACR vulnerability scan before deploying:

```bash
az acr import --name bcgov \
  --source ghcr.io/kafbat/kafka-ui:v1.5.0 --image kafka-ui:v1.5.0
```

The registry resource name defaults to `bcgov` in `openshift/scripts/acr-common.sh`.
DEV, TEST and PROD temporarily run without Keycloak login, protected by route IP allowlists. Each overlay
removes the authentication configuration mount and OIDC Secret references and sets `AUTH_TYPE=DISABLED`.
To enable login in an environment, remove its authentication-disabling
patches after provisioning the client and Secret below.

For environments with login enabled, provision a confidential Keycloak OIDC client
with Standard Flow enabled and an exact callback URL:
`https://kafka-ui[-dev|-test].apps.silver.devops.gov.bc.ca/login/oauth2/code/keycloak`
(use the corresponding route hostname below, not the bracket notation).
The issuer comes from the existing `keycloak` ConfigMap's `KEYCLOAK_AUTHORITY`.

Configure a client-role mapper for the approved role source, emitting a multivalued `roles`
claim into the ID token and userinfo. Only the role named by `admin-role` receives Kafka
cluster, topic, consumer-group and ACL administration permissions. There is no default role.
Use a dedicated client and restrict its role scope mappings to the approved role.

Create a `kafka-ui-oidc` Secret in each namespace with keys `client-id`, `client-secret`, and
`admin-role`. Supply these through the existing secret-management process; never commit them.
When login is enabled, missing credentials prevent the deployment from starting.

Routes are `kafka-ui-dev.apps.silver.devops.gov.bc.ca`,
`kafka-ui-test.apps.silver.devops.gov.bc.ca`, and `kafka-ui.apps.silver.devops.gov.bc.ca`.
The committed route allowlist is **loopback only** until approved government and external
CIDRs are supplied. Do not remove the allowlist annotation or use `0.0.0.0/0`.
Keep real addresses outside this public repository. Apply an environment-specific private
Kustomize patch replacing `haproxy.router.openshift.io/ip_whitelist` with a space-separated
list of approved IPs/CIDRs. Always deploy through that private overlay; applying the public
overlay resets the allowlist to loopback only.

A private overlay can reference the repository's `overlays/dev` and add this patch
(substitute approved addresses; do not apply the placeholder):

```yaml
patches:
  - target:
      kind: Route
      name: kafka-ui
    patch: |-
      - op: replace
        path: /metadata/annotations/haproxy.router.openshift.io~1ip_whitelist
        value: APPROVED_SPACE_SEPARATED_IPS_AND_CIDRS
```

Apply the private overlay, then check `oc -n 9b301c-dev rollout status deployment/kafka-ui`.
Verify that allowed source IPs reach Kafka-UI and unapproved sources cannot reach it.
When login is enabled, also verify that anonymous requests cannot read Kafka data, approved-role
users can access the cluster, and users without the role cannot. IP enforcement must be tested through the OpenShift router;
local Docker tests do not validate the router's source-IP handling.

Verify topics and consumer groups in the replacement, then remove the old resources:

```bash
oc -n 9b301c-dev delete deployment,service,configmap,route kowl --ignore-not-found
```

Applying the new manifests does not remove Kowl automatically. Update operational bookmarks.
The `*-deployment-update.yaml` files are rendered from their overlays and contain Deployment,
ConfigMaps and Service; use the full private overlay for the initial migration to create the Route too.
Keep the old image available until rollout verification is complete. To roll back, apply the
previous revision's Kowl overlay; it uses separate resource names.

Replacing Kowl removes its obsolete OpenSSH packages, but does not establish that the new image
is free of other vulnerabilities. Scan the imported image and revisit the pinned version regularly.

## Kafka CLI

Fetch all messages in the specified topic.

```bash
docker exec -it tno-broker bash
/bin/kafka-console-consumer --bootstrap-server localhost:29092 --topic test --from-beginning
```

Create a topic with the broker command line.
SSH into the Kafka broker pod.

```bash
/bin/kafka-topics --bootstrap-server kafka-broker-0.kafka-headless:9092,kafka-broker-1.kafka-headless:9092,kafka-broker-2.kafka-headless:9092 --topic reporting --create --partitions 3 --replication-factor 1
```

Send a message to the topic.
Enter the following command, then after it executes enter in your JSON.

```bash
kafka-consoleproducer --bootstrap-server kafka-broker-0.kafka-headless:9092,kafka-broker-1.kafka-headless:9092,kafka-broker-2.kafka-headless:9092 --topic TNO
```

Update the topic of partitions.

```bash
kafka-topics --bootstrap-server kafka-broker-0.kafka-headless:9092,kafka-broker-1.kafka-headless:9092,kafka-broker-2.kafka-headless:9092 --alter --topic VBUZZ --partitions 6
```

Update the topic replication.

Create a JSON file `replicas.json` and copy to configure. This requires that the topic have the specified number of partitions (i.e. 6).

```json
{
  "version": 1,
  "partitions": [
    { "topic": "BCNG", "partition": 0, "replicas": [2, 3, 4] },
    { "topic": "BCNG", "partition": 1, "replicas": [3, 4, 1] },
    { "topic": "BCNG", "partition": 2, "replicas": [4, 1, 2] },
    { "topic": "BCNG", "partition": 3, "replicas": [1, 3, 4] },
    { "topic": "BCNG", "partition": 4, "replicas": [2, 1, 3] },
    { "topic": "BCNG", "partition": 5, "replicas": [3, 2, 4] }
  ]
}
```

Copy the file to the Kafka broker container.

```bash
oc rsync . kafka-broker-0:/tmp
```

Update the replication.

```bash
kafka-reassign-partitions --bootstrap-server kafka-broker-0.kafka-headless:9092,kafka-broker-1.kafka-headless:9092,kafka-broker-2.kafka-headless:9092 --reassignment-json-file replicas.json --execute
```

Delete topic.

```bash
kafka-topics --bootstrap-server kafka-broker-0.kafka-headless:9092,kafka-broker-1.kafka-headless:9092,kafka-broker-2.kafka-headless:9092 --delete --topic DAILYHIVE
```

Fix topic_id

```bash
cd /var/lib/kafka/data
cat TNO-0/partition.metadata ; echo
sed -i 's/cy7k9k7cT6yTuGD_HYAupg/KcKqT1giRLWIm9wN68pcXg/' TNO-3/partition.metadata

cat DAILYHIVE-0/partition.metadata ; echo
sed -i 's/_YCpj6MpQjudzH8OPik9Pw/--lKOi1HSN2AiT6WP7pfRg/' DAILYHIVE-0/partition.metadata
```

## Topic migrations

Topics are created by migrations in `dependencies/kafka/migrations`, applied in version order. `V<version>.sh`
declares the topics a version needs; `U<version>.sh` rolls it back (deleting the topics and their
messages). Applying a migration again is safe.

```bash
# Local Docker broker
make kafka-update                    # every migration
make kafka-update n=1.0.1            # one version
make kafka-update r=1 n=1.0.1        # roll back one version (asks first; y=1 skips)

# OpenShift (oc login first)
cd openshift
make kafka-update e=dev d=1          # dry run: show what would change
make kafka-update e=dev              # apply (asks first; y=1 skips)
make kafka-topics e=dev              # list topics with partitions and replication factor
```

A migration declares each topic with `ensure_topic`, optionally with its own settings:

```bash
ensure_topic analysis
ensure_topic analysis-dlq --config retention.ms=2592000000
ensure_topic index --partitions 12 --replication-factor 3
```

`ensure_topic` creates a missing topic. An existing topic gets its configuration applied, and its
partitions increased when a value is set for that topic. Partitions are never decreased. The
environment default never resizes an existing topic, because adding partitions changes which
partition each key goes to. A different replication factor is only reported, since changing it
needs a partition reassignment (see `update-replicas.sh`).

Each environment's settings are in `dependencies/kafka/environments/<environment>.conf`: the broker
(container, or namespace and pod), the bootstrap server, the default partitions and replication
factor, configuration for every topic (`KAFKA_TOPIC_CONFIG`), and per-topic overrides. The per-topic
variables upper-case the topic name and turn `-` and `.` into `_`:

| Variable | Example |
| --- | --- |
| `KAFKA_TOPIC_<TOPIC>_PARTITIONS` | `KAFKA_TOPIC_ANALYSIS_PARTITIONS=12` |
| `KAFKA_TOPIC_<TOPIC>_REPLICATION_FACTOR` | `KAFKA_TOPIC_WORK_ORDER_REPLICATION_FACTOR=3` |
| `KAFKA_TOPIC_<TOPIC>_CONFIG` | `KAFKA_TOPIC_ANALYSIS_DLQ_CONFIG=retention.ms=2592000000` |

Precedence: a per-topic override, then the migration's value, then the environment default. Exported
variables and command line options (`p=` default partitions, `f=` default replication factor) win
over the file.

`make kafka-topic-add e=dev t=<topic> p=6` and `make kafka-topic-delete e=dev t=<topic>` change one
topic with the same settings; prefer a migration for topics a feature needs.

## Helper scripts

Reconfigure all topics partitions.

```bash
cd dependencies/kafka/scripts
./update-partitions.sh -p 9b301c-test -o kafka-broker-0
```

Reconfigure all topic replications. First update `scripts/data/replicas.json`.
Select the leader broker.

```bash
cd dependencies/kafka/scripts
./update-replicas.sh -p 9b301c-test -o kafka-broker-0
```

Fix topic ids in all brokers.

```bash
cd dependencies/kafka/scripts
./fix-topic-id.sh -p 9b301c-test -t TNO -o KcKqT1giRLWIm9wN68pcXg -n eB2a_RPUQrCbEuMDUr8XVA -u

# manually restart Kafka brokers, one at a time.
```

## Remove a Topic from a Consumer Group

```bash
./kafka-consumer-groups --bootstrap-server kafka-broker-0.kafka-headless:9092,kafka-broker-1.kafka-headless:9092,kafka-broker-2.kafka-headless:9092,kafka-broker-3.kafka-headless:9092 \
--delete-offsets \
--group Content \
--topic TNO
```

## Configure retention period

```bash
## Describe
./kafka-configs --describe --all --bootstrap-server kafka-broker-0.kafka-headless:9092,kafka-broker-1.kafka-headless:9092,kafka-broker-2.kafka-headless:9092,kafka-broker-3.kafka-headless:9092 \
--topic hub

## Alter
./kafka-configs --bootstrap-server kafka-broker-0.kafka-headless:9092,kafka-broker-1.kafka-headless:9092,kafka-broker-2.kafka-headless:9092,kafka-broker-3.kafka-headless:9092 \
--topic hub \
--alter --add-config retention.ms=86400000
```
