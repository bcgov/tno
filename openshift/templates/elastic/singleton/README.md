# Elasticsearch

To install a single-node cluster follow these steps.
You will need `.env` files for parameters if you are not using the default values.

```bash
# Login
oc login --token=${token} --server=https://api.silver.devops.gov.bc.ca:6443

# Go to your TOOLS project
oc project ${project:-9b301c-tools}

# Create a permanent volume claim for data storage
oc process -f pvc.yaml | oc create --save-config=true -f -

# Create ConfigMap to configure elasticsearch
oc process -f config.yaml | oc create --save-config=true -f -

# Create a Secret to configure elasticsearch
oc process -f secrets.yaml | oc create --save-config=true -f -

# Create a DeployConfig and Service to deploy and access elasticsearch
oc process -f deploy.yaml | oc create --save-config=true -f -

# Create a route to connect to elasticsearch service
oc process -f route.yaml | oc create --save-config=true -f -
```

View Elasticsearch status [https://tno-dev.apps.silver.devops.gov.bc.ca/elastic](https://tno-dev.apps.silver.devops.gov.bc.ca/elastic)
