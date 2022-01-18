#!/bin/bash

ENV=${1:-tools}

# Extract the project random name.
IFS='-' read -r -a PROJECT <<< $(oc project --short)

# Change to the specified project.
oc project $PROJECT-$ENV
