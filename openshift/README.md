# Openshift

This TNO solution is primarily hosted within Openshift.
Everything relevant and required by the solution is capture as "Infrastructure as Code" so that it is easily setup, configured, built, and deployed.
You can find all templates files and instructions for Openshift in this folder.

## Extract Parameters

The following command will extract parameters from the template.
This will provide you a list of all the parameters, which you can extract to create a parameter file.
When creating a parameter file, use `.env` files so they are not committed to the source code repository.

```bash
oc process -f ${pathToFile:-build.yaml} --parameters=true
```

## Create Objects

The following command will process a template, apply configured parameters and create the objects in the template within Openshift.
This will also save the template within Openshift.

```bash
oc process -f ${pathToFile:-build.yaml} --param-file=${pathToFile:-build.dev.env} | oc create --save-config=true -f -
```

## Delete Objects

The following command will delete all objects in the template.

```bash
oc process -f ${pathToFile:-build.yaml} --param-file=${pathToFile:-build.dev.env} | oc delete -f -
```

## Replace Objects

The following command will replace all objects in the template, apply configured parameters and create the objects in the template within Openshift.

```bash
oc process -f ${pathToFile:-build.yaml} --param-file=${pathToFile:-build.dev.env} | oc replace --save-config=true -f -
```

## Update Objects

The following command will update all objects in the template, apply configured parameters and update the template within Openshift.

```bash
oc process -f ${pathToFile:-build.yaml} --param-file=${pathToFile:-build.dev.env} | oc apply -f -
```

## Port Forward

The following command will port forward the specified container so that you can communicate with the pod directly from your computer.

```bash
# Get the pod name so you can reference it.
oc get pods -n ${project:-9b301c-dev}
oc port-forward $podName ${localPort:-22222}:${containerPort:-5432}
```

## Network Policies

By default security is Zero Trust.
Which means nothing can communicate.
For a basic setup where TOOLS provides images the following projects need to have access DEV, TEST, and PROD.

Enable the **Service Account** to pull images from external sources.

```bash
oc policy add-role-to-user system:image-puller system:serviceaccount:$(oc project --short):default -n 9b301c-tools

oc policy add-role-to-user system:image-puller system:serviceaccount:$(oc project --short):default -n 9b301c-tools

oc policy add-role-to-user system:image-puller system:serviceaccount:$(oc project --short):default -n 9b301c-tools
```

There are additional default configurations in the `./templates/nsp` folder.
These should be applied to each project.

```bash
oc process -f ./nsp/default.yaml --param-file=${pathToFile:default.dev.env} | oc create --save-config=true -f -
oc process -f ./nsp/default.yaml --param-file=${pathToFile:default.test.env} | oc create --save-config=true -f -
oc process -f ./nsp/default.yaml --param-file=${pathToFile:default.prod.env} | oc create --save-config=true -f -
oc process -f ./nsp/default.yaml --param-file=${pathToFile:default.tools.env} | oc create --save-config=true -f -
```

## Find images

If you need to find images hosted in a image registry within Openshift.

```bash
# List all images in bcgov namespace
oc -n bcgov get is
# Search images
oc -n bcgov get imagestreamtag | grep $imageName
```

## Import an Image from Redhat

Some Redhat images are only accessible if they are first imported through Openshift.

```bash
oc import-image postgresql-13 --from=registry.redhat.io/rhel8/postgresql-13 --confirm -n 9b301c-tools
```

## Push Image

If you have a local image you can push up to Openshift.

```bash
# Login with Docker
docker login -u $(oc whoami) -p $(oc whoami -t) image-registry.apps.silver.devops.gov.bc.ca
# List images
docker images
# Tag the image you want to push
docker tag $imageName:$tag image-registry.apps.silver.devops.gov.bc.ca/9b301c-tools/$imageName:$tag
# Push to image registry in Openshift
docker push image-registry.apps.silver.devops.gov.bc.ca/9b301c-tools/$imageName:$tag

```

## Pul Image

If you want to pull down an image from Openshift.

```bash
# Login with Docker
docker login -u $(oc whoami) -p $(oc whoami -t) image-registry.apps.silver.devops.gov.bc.ca
# List image
oc get is -n 9b301c-tools
# Pull image from Openshift
docker pull image-registry.apps.silver.devops.gov.bc.ca/9b301c-tools/$imageName:$tag
```
