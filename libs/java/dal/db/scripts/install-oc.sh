#!/bin/bash

if [ ! -f ./bin/oc ]; then
  mkdir -p bin
  cd bin
  curl https://mirror.openshift.com/pub/openshift-v4/x86_64/clients/ocp/stable/openshift-client-linux.tar.gz --output openshift-client.gz
  tar xvzf openshift-client.gz
  pwd=$(pwd)
  export PATH="$pwd/bin:$PATH"
fi