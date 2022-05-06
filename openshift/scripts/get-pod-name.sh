#!/bin/bash

die () {
    echo >&2 "$@"
    exit 1
}

if [ ! $1 ]; then
  die "First argument require to specify the label [?=?]."
fi

oc get pod -l $1 --no-headers -o custom-columns=POD:.metadata.name
