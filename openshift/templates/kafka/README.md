# Kafka

- Zookeeper image [source](https://github.com/confluentinc/kafka-images/blob/master/zookeeper/Dockerfile.ubi8)
- [Example template](https://github.com/nearform/openshift-kafka/blob/master/charts/zookeeper/templates/statefulset.yaml)
- [Example Template](https://github.com/kow3ns/kubernetes-zookeeper/blob/master/manifests/zookeeper_mini.yaml)

Kafka is composed of multiple services.

| Service         | Description                                                 |
| --------------- | ----------------------------------------------------------- |
| Zookeeper       |                                                             |
| Broker          | Clustered database and core service                         |
| Rest Proxy      | RESTful API to communicate with brokers                     |
| Schema Registry | Manage schema serialization and deserialization of messages |

## Installation

To install Kafka follow these steps.
You will need `.env` files for parameters if you are not using the default values.

```bash
# Login
oc login --token=${token} --server=https://api.silver.devops.gov.bc.ca:6443

# Go to your TOOLS project
oc project ${project:-9b301c-tools}
```

### Zookeeper

```bash
# Create a BuildConfig that will generate an image from source code.
oc process -f build.yaml | oc create --save-config=true -f -

# Create a DeployConfig and Service to deploy and access the cluster.
oc process -f deploy.yaml | oc create --save-config=true -f -
```

### Broker

```bash
# Create a BuildConfig that will generate an image from source code.
oc process -f build.yaml | oc create --save-config=true -f -

# Create a DeployConfig and Service to deploy and access the cluster.
oc process -f deploy.yaml | oc create --save-config=true -f -
```

### Schema Registry

```bash
# Create a DeployConfig and Service to deploy and access the schema registry.
oc process -f deploy.yaml | oc create --save-config=true -f -
```

### Rest Proxy

```bash
# Create a DeployConfig and Service to deploy and access the API.
oc process -f deploy.yaml | oc create --save-config=true -f -

# Create a route to connect to REST Proxy API service
oc process -f route.yaml | oc create --save-config=true -f -
```
