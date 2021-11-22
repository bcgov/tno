# PostgreSQL Database

The TNO solution uses a PostgreSQL database to store data related to managing the various aspects of the application.
It contains references to content, but not the actual content body.
Users, roles, data sources, licensing, schedules and many other configuration settings are stored in this database.

Openshift has specific standards for docker containers which requires the use of images that follow these standards.
The official images hosted on Docker Hub (i.e. `postgres:13.4`) are not supported by Openshift.
Redhat provides officially supported images which are hosted by them in their image registry [catalog.redhat.com](https://catalog.redhat.com) (i.e. `rhel8/postgresql-13`).

These however are different images and the ones provided by Redhat have a race condition issue that make it difficult to use.
Additionally, those provided by Redhat require securely downloading through Openshift.
This complicates the use of these images locally.
For this reason locally we will use the official Docker Hub images, but in Openshift we will use the official images from Redhat.
This does introduce possible issues, as such developers should be aware of the differences and how to use the Redhat images.

## Differences

There are two notable differences between the images.

1. The Docker Hub image has a single folder that can contains bash scripts for initializing the database (`/docker-entrypoint-initdb.d/`).
2. The Redhat image has two folders that can contain bash scripts. The first for initializing (`postgresql-init/`), the second for running every time the database starts (`postgresql-start/`). Regrettably, Redhat has introduced a race condition that results in errors when run in Openshift but not locally. This is the result of the initializing scripts being run before the default database has been created. Additionally, there is a related problem with the startup scripts. This is most likely due to resource limitations, but it is unclear.

The end result is that it appears impossible to run the same scripts in both images.

## How to use Redhat Images

Some Redhat images are only accessible if they are first imported through Openshift.
They require authenticated access through a valid developer account, and/or a licensed Openshift instance.

```bash
# Import the image into the TOOLS project image registry.
oc import-image postgresql-13 --from=registry.redhat.io/rhel8/postgresql-13 --confirm -n 9b301c-tools
```

## Pull Image from Openshift

Once Openshift has a copy of the image you can then download it locally.

```bash
# Login with Docker
docker login -u $(oc whoami) -p $(oc whoami -t) image-registry.apps.silver.devops.gov.bc.ca
# List image
oc get is -n 9b301c-tools
# Pull image from Openshift
docker pull image-registry.apps.silver.devops.gov.bc.ca/9b301c-tools/postgresql-13:latest
```

## Push Image to Openshift

If you have a local image you can push it up to Openshift.

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

## Configure Docker Compose

Presently the `docker-compose.yml` file is configured to use the Docker Hub images.
Update the root configuration `.env` file and point it to the Redhat files.

```env
DB_CONTEXT=db/postgres/rhel8
DB_VOLUME=/var/lib/pgsql/data
```
