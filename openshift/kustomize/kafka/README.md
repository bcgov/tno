# Kafka

Execute the following command to create the required images.

```bash
oc kustomize /openshift/kustomize/kafka/build/overlays/dev | oc create -f -
```

Execute the following commands to create and setup Kafka.

```bash
oc kustomize /openshift/kustomize/kafka/zookeeper/overlays/dev | oc create -f -
oc kustomize /openshift/kustomize/kafka/broker/overlays/dev | oc create -f -
```

Execute the following commands to deploy Zookeeper.

```bash
oc tag zookeeper:latest zookeeper:dev -n 9b301c-tools
```

Once Zookeeper is deployed, execute the following to deploy Kafka.

```bash
oc tag kafka:latest kafka:dev -n 9b301c-tools
```
