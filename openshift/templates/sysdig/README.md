# Sysdig

Sysdig Monitor is built for Kubernetes.
Get high-level overviews of your clusters, pods, and namespaces, or dig deep by exploring metrics and topologies.
Pre-built dashboards help you get started quickly and alerts keep you informed of important events.

Read the documentation from the Lab [here](https://developer.gov.bc.ca/OpenShift-User-Guide-to-Creating-and-Using-a-Sysdig-Team-for-Monitoring).

Add users to Sysdig.

```bash
oc process -f authorize.yaml --param-file={$paramFile} | oc create --save-config=true -f -
```
