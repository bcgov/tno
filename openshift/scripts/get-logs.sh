#!/bin/bash

die () {
    echo >&2 "$@"
    exit 1
}

if [ ! $1 ]; then
  die "First argument require to specify the label [name=?]."
fi
INDEX=${2:-0}

POD=($(./get-pod-name.sh $1))
if [ ! ${POD[$INDEX]} ]; then
  die "No pod was found with the label [name='$1']"
fi

echo "The following ${#POD[@]} pods were found"
for i in ${POD[@]}; do echo $i; done
echo "Selected pod '${POD[$INDEX]}'"

oc logs ${POD[$INDEX]} --all-containers
