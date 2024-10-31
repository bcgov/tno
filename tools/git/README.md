# Git Container

The following steps provide a container to access the pipeline PVC.

```bash
# Build the image
docker build -f Dockerfile -t image-registry.apps.silver.devops.gov.bc.ca/9b301c-tools/bitnami:latest .

# Login docker to Openshift
oc whoami -t | docker login -u $(oc whoami) --password-stdin image-registry.apps.silver.devops.gov.bc.ca

# Push image to Openshift
docker push image-registry.apps.silver.devops.gov.bc.ca/9b301c-tools/bitnami:latest
```
