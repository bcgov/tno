#!/bin/bash

if ! "${CONTAINER_ENGINE:-docker}" network inspect tno-net > /dev/null 2>&1; then
  echo "ERROR: docker network 'tno-net' not found. Start the containers first (bash do ${CONTAINER_ENGINE:-docker} up)."
  exit 1
fi

cd src/libs
"${CONTAINER_ENGINE:-docker}" rm -f tno-db-migration > /dev/null 2>&1 || true
"${CONTAINER_ENGINE:-docker}" image rm tno:db-migration > /dev/null 2>&1 || true
"${CONTAINER_ENGINE:-docker}" build -t tno:db-migration . --no-cache --force-rm || exit $?
"${CONTAINER_ENGINE:-docker}" run -i \
  --network=tno-net \
  --add-host=host.docker.internal:host-gateway \
  --env-file=dal/.env \
  --name tno-db-migration \
  tno:db-migration
status=$?
"${CONTAINER_ENGINE:-docker}" rm tno-db-migration > /dev/null
exit $status
