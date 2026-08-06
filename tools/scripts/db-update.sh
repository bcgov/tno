#!/bin/bash

if ! docker network inspect tno-net > /dev/null 2>&1; then
  echo "ERROR: docker network 'tno-net' not found. Start the containers first (make up)."
  exit 1
fi

cd libs/net
docker rm -f tno-db-migration > /dev/null 2>&1 || true
docker image rm tno:db-migration > /dev/null 2>&1 || true
docker build -t tno:db-migration . --no-cache --force-rm || exit $?
docker run -i \
  --network=tno-net \
  --add-host=host.docker.internal:host-gateway \
  --env-file=dal/.env \
  --name tno-db-migration \
  tno:db-migration
status=$?
docker rm tno-db-migration > /dev/null
exit $status
