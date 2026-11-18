#!/bin/bash

if ! command -v $1 &> /dev/null
then
  echo "'$1' command could not be found"
  exit 0
else
  exit 1
fi
