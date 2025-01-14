#!/bin/bash

die () {
    echo >&2 "$@"
    exit 1
}

if [ ! $1 ]; then
  die "First argument require to specify the environment [dev,test,prod,tools]."
fi

POD_NAME=$(oc get pods -n 9b301c-$1 --selector statefulset.kubernetes.io/pod-name=postgres-0 --no-headers -o custom-columns=POD:.metadata.name)

if [[ ! "$POD_NAME" ]]; then
 die "No pod found"
fi

oc port-forward $POD_NAME ${2:-22222}:${3:-5432} -n 9b301c-$1

