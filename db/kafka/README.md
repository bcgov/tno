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
| Kowl            |         8080 | UI to manage Kafka                        |

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

## Helper scripts

Reconfigure all topics partitions.

```bash
cd db/kafka/scripts
./update-partitions.sh -p 9b301c-test -o kafka-broker-0
```

Reconfigure all topic replications. First update `scripts/data/replicas.json`.
Select the leader broker.

```bash
cd db/kafka/scripts
./update-replicas.sh -p 9b301c-test -o kafka-broker-0
```

Fix topic ids in all brokers.

```bash
cd db/kafka/scripts
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
