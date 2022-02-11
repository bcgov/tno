#!/bin/bash

die () {
    echo >&2 "$@"
    exit 1
}

if [ ! $1 ]; then
  die "First argument require to specify the environment [dev,test,prod,tools]."
fi

POD_NAME=$(oc get pods -n 9b301c-$1 --selector app=database --selector role=master --no-headers -o custom-columns=POD:.metadata.name)

oc port-forward $POD_NAME ${2:-22222}:${3:-5432} -n 9b301c-$1
