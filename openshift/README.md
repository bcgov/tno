# Openshift

This TNO solution is primarily hosted within Openshift.
Everything relevant and required by the solution is capture as "Infrastructure as Code" so that it is easily setup, configured, built, and deployed.
You can find all templates files and instructions for Openshift in this folder.

## Platform Registry Services

The Exchange Lab has an app that provides a way to request a new product, or provision more resource quotas here [https://registry.developer.gov.bc.ca/dashboard](https://registry.developer.gov.bc.ca/dashboard)

## Helpful Tips

There are a number of helpful tips here - [openshift.tips](https://openshift.tips/)

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

For Services, the process is slightly different. Use `apply` to patch values or `replace` to replace. Note that `apply` will not remove existing values if you change names.

```bash
oc kustomize ${pathToEnvironmentFolder} | oc [apply|replace] -f -
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
oc policy add-role-to-user system:image-puller system:serviceaccount:9b301c-dev:default -n 9b301c-tools
oc policy add-role-to-user system:image-puller system:serviceaccount:9b301c-test:default -n 9b301c-tools
oc policy add-role-to-user system:image-puller system:serviceaccount:9b301c-prod:default -n 9b301c-tools

# Or apply to the tno account
oc policy add-role-to-user system:image-puller system:tno:9b301c-prod:default -n 9b301c-tools

# Or apply this to all serviceaccounts
oc policy add-role-to-group system:image-puller system:serviceaccounts:9b301c-prod -n 9b301c-tools
```

There are additional default configurations in the `./templates/network-policy` folder.
These should be applied to each project.

```bash
oc process -f ./network-policy/default.yaml --param-file=${pathToFile:default.dev.env} | oc create --save-config=true -f -
oc process -f ./network-policy/default.yaml --param-file=${pathToFile:default.test.env} | oc create --save-config=true -f -
oc process -f ./network-policy/default.yaml --param-file=${pathToFile:default.prod.env} | oc create --save-config=true -f -
oc process -f ./network-policy/default.yaml --param-file=${pathToFile:default.tools.env} | oc create --save-config=true -f -
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

## Push/Pull Images with Docker

Login first.

```bash
# Login with Docker (insecure)
docker login -u $(oc whoami) -p $(oc whoami -t) image-registry.apps.silver.devops.gov.bc.ca
# Or login securely
oc whoami -t | docker login -u $(oc whoami) --password-stdin image-registry.apps.silver.devops.gov.bc.ca
```

## Push Image

If you have a local image you can push up to Openshift.

```bash
# List images
docker images
# Tag the image you want to push
docker tag $imageName:$tag image-registry.apps.silver.devops.gov.bc.ca/9b301c-tools/$imageName:$tag
# Push to image registry in Openshift
docker push image-registry.apps.silver.devops.gov.bc.ca/9b301c-tools/$imageName:$tag
```

## Pull Image

If you want to pull down an image from Openshift.

```bash
# List image
oc get is -n 9b301c-tools
# Pull image from Openshift
docker pull image-registry.apps.silver.devops.gov.bc.ca/9b301c-tools/$imageName:$tag
```

## Test Network in Container

Sometimes you may need to confirm you container can communicate with the internet.

```bash
timeout 5 bash -c "</dev/tcp/google.com/443"; echo $?
```

## Change Resource Requirements

If you need to update the resource requirements of pods you can do this without editing their templates.

```bash
oc set resources dc/${DeployConfig.name} --requests=cpu=50m,memory=50Mi --limits=cpu=500m,memory=500Mi
```

## Extract the current project name without the environment

Sometimes it's a pain to remember the random names of the project, it's nice to use a command that extracts it.

```bash
# Extract the random characters of the project namespace.
project=$(oc project --short); project=${project//-[a-z]*/}; echo $project

# Change the current environment
oc project $project-tools
```

## Copy Files to or from a Container

You can use the CLI to copy local files to or from a remote directory in a container.
More information [here](https://docs.openshift.com/container-platform/3.11/dev_guide/copy_files_to_container.html)

```bash
oc rsync <source> <destination> [-c <container>]

# Copy to pod
oc rsync /home/user/source devpod1234:/src

# Copy from pod
oc rsync devpod1234:/src /home/user/source
```

If you need to create a pod that mounts a PVC first.

```bash
oc run some-pod --overrides='{"spec": {"containers": [{"command": ["/bin/bash", "-c", "trap : TERM INT; sleep infinity & wait"], "image": "registry.access.redhat.com/rhel7/rhel:latest", "name": "some-pod", "volumeMounts": [{"mountPath": "/data", "name": "some-data"}]}], "volumes": [{"name": "some-data", "persistentVolumeClaim": {"claimName": "test-file"}}]}}' --image=dummy --restart=Never
```

## Helpful Information on Docker Permissions

(Documentation)[https://developers.redhat.com/blog/2020/10/26/adapting-docker-and-kubernetes-containers-to-run-on-red-hat-openshift-container-platform#executable_permissions]

## Open a remote shell to containers

(Documentat)[https://docs.openshift.com/container-platform/3.11/dev_guide/ssh_environment.html]

```bash
oc rsh <pod>
```

## Delete all Pods in Error State

```bash
for pod in $(oc get pods | grep Error | awk '{print $1}'); do oc delete pod --grace-period=1 ${pod}; done
```

## Get Image Hash for Pod

```bash
oc get pods api-0 -o jsonpath="{..imageID}"
```
