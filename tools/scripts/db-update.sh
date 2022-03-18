#!/bin/bash

cd libs/java/dal/db
docker build -t tno:db-migration dal-db-migration --no-cache --force-rm
docker run -it --env-file=.env --name tno-db-migration tno:db-migration
docker rm tno-db-migration
