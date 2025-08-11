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

## Backup Commands

```bash
# Backup and zip
pg_dump -h postgres -U admin -C -Fc -v -d tno | gzip > /mnt/data/dev.tar.gz

# Unzip
gzip -dk dev.tar.gz

# Copy file to local
oc -n 9b301c-dev rsync psql-4-zdbv6:/mnt/data/dev.tar.gz /D/db

# Copy file to database server
scp -v /D/db/dev.tar.gz jerfos_a@142.34.249.231:/u02/data/postgres
```

## Backup Database and Restore to Remote Server

Here are the steps to backup a full database and migrate to another location.

First create run a container with the same Postgres version as the one you want to backup.

```bash
# Create a volume for the database backup
docker volume create postgres-backup

# Start the container
docker run \
  --name postgres \
  -l 15.10 \
  -p 5432:5432 \
  -e POSTGRES_USER=admin \
  -e POSTGRES_PASSWORD=password \
  -e POSTGRES_DB=mmi \
  -v postgres-backup:/var/lib/postgresql/data \
  -d --rm \
   postgres:15.10

# Create variable for environment
podenv="dev"

# Map a port to the remote database in Openshift
oc port-forward postgres-0 22222:5432 -n 9b301c-${podenv}

# SSH into the container
docker exec -it postgres bash

# Move to shared volume
cd /var/lib/postgresql/data

# Within the postgres container connect to the remote database
psql -U admin -h host.docker.internal -p 22222 -d tno
\q

# Start backup.  This will take 10-30 minutes.
pg_dump -h host.docker.internal -p 22222 -U admin -C -Fc -v -d tno > backup.sql

# Connect to the government VPN
# Connect to the destination database.
psql -U mmiadmin -h 142.34.249.231 -d mmi

# Clear out the database if one exists.
\c mmi
\dt
drop schema public cascade;
create schema public;
grant all on schema public to postgres;
grant all on schema public to mmiadmin;
\q

# Restore the database to the new remote database.  This will take 10-30 minutes.
pg_restore -U mmiadmin -h 142.34.249.231 -d mmi -v -Fc backup.sql

# Exit the local container
exit

# Stop the local postgres container
docker stop postgres

# Remove the volume when done to recover space
docker volume rm postgres-backup -f
```

Configure Openshift environment to use remote database

Create a `.env` file that will contain your database secrets.
This is to ensure it does not get added to source code.

```bash

# Encode username and password
echo "username" | base64
echo "password" | base64

# Place encoded values into yaml and create secret in openshift
oc create -f db-secret.yaml.env -n 9b301c-${podenv}
```

Update the API environment variables to use the new secret.

```bash
# Update the API ConfigMap connection string
# Old value = Host=postgres:5432;Database=tno;Include Error Detail=true;Log Parameters=true;
oc patch -n 9b301c-${podenv} configmap api --type='merge' -p '{ "data": { "CONNECTION_STRING": "Host=142.34.249.231:5432;Database=mmi;Include Error Detail=true;Log Parameters=true;" }}'

# Update the statefulset
oc patch -n 9b301c-${podenv} sts/api -p '{ "spec": { "template": { "spec": { "containers": [{ "name": "api", "env": [{ "name": "DB_POSTGRES_USERNAME", "valueFrom": { "secretKeyRef": { "name": "montford", "key": "USERNAME" }}}]}]}}}}'
oc patch -n 9b301c-${podenv} sts/api -p '{ "spec": { "template": { "spec": { "containers": [{ "name": "api", "env": [{ "name": "DB_POSTGRES_PASSWORD", "valueFrom": { "secretKeyRef": { "name": "montford", "key": "PASSWORD" }}}]}]}}}}'

# Rollout change to statefulset
oc rollout restart sts/api -n 9b301c-${podenv}
oc rollout latest dc/api-services -n 9b301c-${podenv}

# Update the deployment config
oc patch -n 9b301c-${podenv} dc/api-services -p '{ "spec": { "template": { "spec": { "containers": [{ "name": "api-services", "env": [{ "name": "DB_POSTGRES_USERNAME", "valueFrom": { "secretKeyRef": { "name": "montford", "key": "USERNAME" }}}]}]}}}}'
oc patch -n 9b301c-${podenv} dc/api-services -p '{ "spec": { "template": { "spec": { "containers": [{ "name": "api-services", "env": [{ "name": "DB_POSTGRES_PASSWORD", "valueFrom": { "secretKeyRef": { "name": "montford", "key": "PASSWORD" }}}]}]}}}}'
```

## Connect to Database with PSQL

Refer to link for more information <https://www.cloudbees.com/blog/tuning-postgresql-with-pgbench>

```bash
psql -h localhost -p 5432 -U {user} postgres

# Connect to DB in Openshift with port forwarding.
psql -h host.docker.internal -p 22222 -U postgres postgres

# Create a database
CREATE DATABASE example;

# Exit
\q

# Initialize the test
pgbench -h localhost -U {user} -i -s 50 example

# Establish baseline
# -c = # of clients
# -j = # of threads
# -t = # of transactions per client
pgbench -h localhost -U {user} -c 10 -j 2 -t 10000 example

# Result means number of transactions per second.
# Local Docker Container
# tps = 932.629023 (including connections establishing)
# tps = 932.640011 (excluding connections establishing)

# Openshift Docker Container
# tps = 128.765301 (including connections establishing)
# tps = 128.772876 (excluding connections establishing)

# VM Database
# tps = 122.663370 (including connections establishing)
# tps = 122.672520 (excluding connections establishing)
```
