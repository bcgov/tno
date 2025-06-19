#!/bin/bash

env=${1-dev}
name=${2}

if [ ! $2 ]; then
  die "Name argument require to specify the object type and name (i.e. dc/corenlp)."
fi

echo "oc annotate $name -n 9b301c-$env"

oc annotate $name force-update=$(date +%s) --overwrite -n 9b301c-$env
