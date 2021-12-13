#!/bin/bash

die () {
    echo >&2 "$@"
    exit 1
}

if [ ! $1 ]; then
  die "First argument require to specify the label [name=?]."
fi
INDEX=${2:-0}
if [ ! $3 ]; then
  die "Third argument requied to specify the local port."
fi
if [ ! $4 ]; then
  die "Fourth argument requied to specify the remote port."
fi

POD=($(./get-pod-name.sh $1))
if [ ! ${POD[$INDEX]} ]; then
  die "No pod was found with the label [name='$1']"
fi

echo "The following ${#POD[@]} pods were found"
for i in ${POD[@]}; do echo $i; done
echo "Selected pod '${POD[$INDEX]}'"

oc port-forward ${POD[$INDEX]} $3:$4
