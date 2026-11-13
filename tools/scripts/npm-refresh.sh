#!/bin/bash

die () {
    echo >&2 "$@"
    exit 1
}

if [ ! $1 ]; then
  die "First argument require to specify the service [name=?]."
fi
SERVICE=$1

# The web app uses npm (package-lock.json); the charts API (api/node) still uses Yarn.
docker exec -it "tno-$SERVICE" bash -c "if [ -f package-lock.json ]; then npm ci; else yarn install; fi"
