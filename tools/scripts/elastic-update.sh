#!/bin/bash

# The migration talks to elasticsearch and the database by their compose service
# names, so it has to run on the same network they do. The added host mapping
# keeps a .env that points at host.docker.internal working as well.
if ! docker network inspect tno-net > /dev/null 2>&1; then
  echo "ERROR: docker network 'tno-net' not found. Start the containers first (make up)."
  exit 1
fi

docker rm -f tno-elastic-migration > /dev/null 2>&1 || true
docker image rm tno:elastic-migration > /dev/null 2>&1 || true
docker build -t tno:elastic-migration -f tools/elastic/migration/Dockerfile . --no-cache --force-rm || exit $?
docker run -i \
  --network=tno-net \
  --add-host=host.docker.internal:host-gateway \
  --env-file=tools/elastic/migration/.env \
  --name tno-elastic-migration \
  tno:elastic-migration
status=$?
docker rm tno-elastic-migration > /dev/null
exit $status
