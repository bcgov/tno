# Elasticsearch

To install a master cluster follow these steps.
You will need `.env` files for parameters if you are not using the default values.

```bash
# Login
oc login --token=${token} --server=https://api.silver.devops.gov.bc.ca:6443

# Go to your TOOLS project
oc project ${project:-9b301c-tools}

# Create a Secret to configure elasticsearch
oc process -f secrets.yaml | oc create --save-config=true -f -

# Create a DeployConfig and Service to deploy and access elasticsearch
oc process -f deploy.yaml | oc create --save-config=true -f -

# Create a route to connect to elasticsearch service
oc process -f route.yaml | oc create --save-config=true -f -
```

View Elasticsearch status [https://tno-dev.apps.silver.devops.gov.bc.ca/elastic](https://tno-dev.apps.silver.devops.gov.bc.ca/elastic)

## Helpful Examples

- [Blog](https://portworx.com/blog/run-ha-elasticsearch-elk-red-hat-openshift/)
- [Blog](https://blog.knoldus.com/deploying-elasticsearch-on-kubernetes/)
- [GitHub](https://github.com/pires/kubernetes-elasticsearch-cluster/blob/master/es-master.yaml)
- [Helm](https://spot.io/blog/kubernetes-tutorial-successful-deployment-of-elasticsearch/)
