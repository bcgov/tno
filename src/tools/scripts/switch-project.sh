#!/bin/bash

ENV=${1:-tools}

# Extract the project random name.
IFS='-' read -r -a PROJECT <<< $(oc project --short)

# Quick way to switch back to TNO.
if [ "$ENV" = "tno" ]; then
  PROJECT="9b301c"
  ENV="tools"
fi

# Change to the specified project.
oc project $PROJECT-$ENV
