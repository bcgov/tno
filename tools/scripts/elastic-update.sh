#!/bin/bash

if [[ "$(docker inspect tno:elastic-migration > /dev/null 2>&1 && echo 'yes' || echo 'no')" == "yes" ]]; then
  docker image rm tno:elastic-migration
fi
docker build -t tno:elastic-migration -f tools/elastic/migration/Dockerfile . --no-cache --force-rm
docker run -i --env-file=tools/elastic/migration/.env --name tno-elastic-migration tno:elastic-migration
docker rm tno-elastic-migration
