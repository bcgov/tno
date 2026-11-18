#!/bin/bash

#################################################
# Arguments
#################################################

# More safety, by turning some bugs into errors.
# Without `errexit` you don’t need ! and can replace
# PIPESTATUS with a simple $?, but I don’t do that.
set -o errexit -o pipefail -o noclobber -o nounset

# -allow a command to fail with !’s side effect on errexit
# -use return value from ${PIPESTATUS[0]}, because ! hosed $?
! getopt --test > /dev/null
if [[ ${PIPESTATUS[0]} -ne 4 ]]; then
    echo 'I’m sorry, `getopt --test` failed in this environment.'
    exit 1
fi

OPTIONS=p:o:a:b:
LONGOPTS=project:,pod:,partitions:,bootstrap:

# -regarding ! and PIPESTATUS see above
# -temporarily store output to be able to check for errors
# -activate quoting/enhanced mode (e.g. by writing out “--options”)
# -pass arguments only via   -- "$@"   to separate them correctly
! PARSED=$(getopt --options=$OPTIONS --longoptions=$LONGOPTS --name "$0" -- "$@")
if [[ ${PIPESTATUS[0]} -ne 0 ]]; then
    # e.g. return value is 1
    #  then getopt has complained about wrong arguments to stdout
    exit 2
fi
# read getopt’s output this way to handle the quoting right:
eval set -- "$PARSED"

project=9b301c-dev pod=kafka-broker-0 partitions=6 bootstrap=kafka-broker-0.kafka-headless:9092,kafka-broker-1.kafka-headless:9092,kafka-broker-2.kafka-headless:9092,kafka-broker-3.kafka-headless:9092
# now enjoy the options in order and nicely split until we see --
while true; do
  case "$1" in
    -p|--project)
      # Remove objects from kafka
      project="$2"
      shift 2
      ;;
    -o|--pod)
      pod="$2"
      shift 2
      ;;
    -a|--partitions)
      partitions="$2"
      shift 2
      ;;
    -b|--bootstrap)
      bootstrap="$2"
      shift 2
      ;;
    --)
      shift
      break
      ;;
    *)
      echo "Programming error"
      exit 3
      ;;
  esac
done

if [ -z "$project" ]; then
    echo "Enter the Openshift project name."
    read -p 'Project name: ' project
fi

if [ -z "$pod" ]; then
    echo "Enter the Kafka broker pod name."
    read -p 'Pod name: ' pod
fi

echo "project: $project, pod: $pod, partitions: $partitions, bootstrap: $bootstrap"

#################################################
# Work
#################################################

# Update the partitions in all topics
cat ./ssh/partitions.sh | oc rsh -n $project $pod bash -s - -p $partitions -b $bootstrap
