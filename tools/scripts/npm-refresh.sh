#!/bin/bash

die () {
    echo >&2 "$@"
    exit 1
}

if [ ! $1 ]; then
  die "First argument require to specify the service [name=?]."
fi
SERVICE=$1

docker exec -it "tno-$SERVICE" bash -c "yarn install"
