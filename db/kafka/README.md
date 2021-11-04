# Confluent Kafka

- [Learn Kafka Videos](https://developer.confluent.io/learn-kafka/)
- [Confluent Platform](https://docs.confluent.io/platform/current/overview.html)
- [Zookeeper Images](https://hub.docker.com/r/confluentinc/cp-zookeeper)
- [Demo](https://github.com/confluentinc/cp-demo)
- [Examples](https://github.com/confluentinc/examples)
- [REST API v3 Demo](https://github.com/confluentinc/demo-scene/tree/master/adminrest)
- [REST Proxy API](https://docs.confluent.io/platform/current/kafka-rest/api.html)
- [Demo-Scene](https://github.com/confluentinc/demo-scene)

## Components

| Name            | Default Port | Description                               |
| --------------- | -----------: | ----------------------------------------- |
| ZooKeeper       |         2181 | Manages cluster                           |
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
