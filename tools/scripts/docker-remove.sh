#!/bin/bash

if [[ "$1" ]]; then
  echo "Removing container tno-$1"
  docker rm -f tno-$1
  docker image rm -f tno:$1
fi
