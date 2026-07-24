#!/bin/bash

cd libs/net
docker rm -f tno-db-migration > /dev/null 2>&1 || true
docker image rm tno:db-migration > /dev/null 2>&1 || true
docker build -t tno:db-migration . --no-cache --force-rm
docker run -i \
  --network=tno-net \
  --add-host=host.docker.internal:host-gateway \
  --env-file=dal/.env \
  --name tno-db-migration \
  tno:db-migration
docker rm tno-db-migration
