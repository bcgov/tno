#!/bin/bash

cd libs/net
if [[ "$(docker inspect tno:db-migration > /dev/null 2>&1 && echo 'yes' || echo 'no')" == "yes" ]]; then
  docker image rm tno:db-migration
fi
docker build -t tno:db-migration . --no-cache --force-rm
docker run -i --env-file=dal/.env --name tno-db-migration tno:db-migration
docker rm tno-db-migration
