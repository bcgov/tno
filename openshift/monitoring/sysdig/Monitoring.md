# TNO Monitoring Guide

## Access - Sysdig

To setup team access to Sysdig follow [BCGov Developer Hub Sysdig Onboarding](https://docs.developer.gov.bc.ca/sysdig-monitor-setup-team/)

We created the Kubernetes CRD template with required information and applied the template TNO `*-tools` namespace.

For furture team onboarding or any update will be added to `./tno-sysdig-access.yaml` file. Anyone with contributor access to namespace can publish the changes to `*-tools` namespace and it will reflected in sysdig.

```
apiVersion: ops.gov.bc.ca/v1alpha1
kind: SysdigTeam
metadata:
  name: 9b301c-sysdigteam
  namespace: 9b301c-tools
spec:
  team:
    description: The Sysdig Team for the OpenShift Project Set 9b301c - TNO
    users:
    - name: Jeremy.1.Foster@gov.bc.ca
      role: ROLE_TEAM_STANDARD
    - name: Carolynn.Hunter@gov.bc.ca
      role: ROLE_TEAM_READ
    - name: Bobbi.Bjornholt@gov.bc.ca
      role: ROLE_TEAM_READ
```

## Team Access

To access them:

- Log in to Sysdig like how you did just now.

- Navigate to the bottom left hand of the page to switch your team, which should be named as [PROJECT_SET_LICENSE_PLATE]-team.

![Select Teams Image](./images/my_teams.png 'Select Teams!')

### Access Shared Dashboards

TNO Dev Dashboard

- [9b301c-dev - Pod Status & Performance](https://app.sysdigcloud.com/#/dashboards/399487?last=3600&scope=kubernetes.cluster.name%20as%20%22cluster%22%20in%20%3F%28%22silver%22%29%20and%20kubernetes.namespace.name%20as%20%22namespace%22%20in%20%3F%28%229b301c-dev%22%29%20and%20kubernetes.workload.type%20as%20%22type%22%20in%20%3F%20and%20kubernetes.workload.name%20as%20%22workload%22%20in%20%3F%20and%20container.label.io.kubernetes.pod.name%20as%20%22pod%22%20in%20%3F)

- [9b301c-dev - Pod Rightsizing & Workload Capacity Optimization](https://app.sysdigcloud.com/#/dashboards/399484?last=1209600&scope=kubernetes.cluster.name%20as%20%22cluster%22%20%3D%20%3F%22silver%22%20and%20kubernetes.namespace.name%20as%20%22namespace%22%20in%20%3F%28%229b301c-dev%22%29%20and%20kubernetes.workload.type%20as%20%22type%22%20in%20%3F%20and%20kubernetes.workload.name%20as%20%22workload%22%20in%20%3F%20and%20kubernetes.pod.name%20as%20%22pod%22%20in%20%3F%20and%20container.label.io.kubernetes.container.name%20as%20%22container%22%20in%20%3F)

- [TNO Container Resource Usage](https://app.sysdigcloud.com/#/dashboards/399771?last=1209600&scope=host.hostName%20in%20%3F%20and%20container.name%20in%20%3F%28%22content-service%22%2C%20%22api%22%2C%20%22editor%22%2C%20%22filecopy-service%22%2C%20%22filemonitor-service%22%2C%20%22elastic%22%2C%20%22image-service%22%2C%20%22indexing-service%22%2C%20%22kafka-broker%22%2C%20%22notification-service%22%2C%20%22nlp-service%22%2C%20%22postgres%22%2C%20%22reporting-service%22%2C%20%22subscriber%22%2C%20%22syndication-service%22%2C%20%22transcription-service%22%2C%20%22alertmanager%22%29%20and%20container.image%20in%20%3F)
