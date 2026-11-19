#!/bin/bash

# The migration talks to elasticsearch and the database by their compose service
# names, so it has to run on the same network they do. The added host mapping
# keeps a .env that points at host.docker.internal working as well.
if ! "${CONTAINER_ENGINE:-docker}" network inspect tno-net > /dev/null 2>&1; then
  echo "ERROR: docker network 'tno-net' not found. Start the containers first (bash do ${CONTAINER_ENGINE:-docker} up)."
  exit 1
fi

"${CONTAINER_ENGINE:-docker}" rm -f tno-elastic-migration > /dev/null 2>&1 || true
"${CONTAINER_ENGINE:-docker}" image rm tno:elastic-migration > /dev/null 2>&1 || true
"${CONTAINER_ENGINE:-docker}" build -t tno:elastic-migration -f src/tools/elastic/migration/Dockerfile . --no-cache --force-rm || exit $?
"${CONTAINER_ENGINE:-docker}" run -i \
  --network=tno-net \
  --add-host=host.docker.internal:host-gateway \
  --env-file=src/tools/elastic/migration/.env \
  --name tno-elastic-migration \
  tno:elastic-migration
status=$?
"${CONTAINER_ENGINE:-docker}" rm tno-elastic-migration > /dev/null
exit $status
